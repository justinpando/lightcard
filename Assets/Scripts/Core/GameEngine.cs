using System;
using System.Collections.Generic;
using System.Linq;

namespace LightCard.Core
{
    /// <summary>
    /// The deterministic rules engine. Pure C#: no UnityEngine, no rendering, no IO.
    /// Same seed + same command sequence = same events, which is what enables AI
    /// simulation, server-side validation, and replays.
    /// Rulings that interpret ambiguous card text live in Docs/design/rules-v1.md.
    /// </summary>
    public static class GameEngine
    {
        //Re-entrancy guard for Rugged knockback (single-threaded engine)
        private static bool resolvingRuggedPush;

        //---- Setup ----

        public static GameState CreateGame(List<string> deck0, List<string> deck1, int seed, List<GameEvent> events)
        {
            var state = new GameState { Seed = seed };
            state.Players[0].Deck = new List<string>(deck0);
            state.Players[1].Deck = new List<string>(deck1);

            Shuffle(state, state.Players[0].Deck);
            Shuffle(state, state.Players[1].Deck);

            events.Add(new GameEvent { Type = GameEventType.GameStarted });

            //Rules-v2: the player going second draws an extra opening card
            for (int p = 0; p < 2; p++)
                DrawCards(state, p, GameConfig.StartingHandSize + (p == 1 ? GameConfig.SecondPlayerBonusCards : 0), events);

            state.ActivePlayer = 0;
            StartTurn(state, events);

            return state;
        }

        private static void Shuffle(GameState state, List<string> deck)
        {
            for (int n = deck.Count - 1; n > 0; n--)
            {
                int k = state.NextRandom(n + 1);
                string tmp = deck[n];
                deck[n] = deck[k];
                deck[k] = tmp;
            }
        }

        //---- Command dispatch ----

        public static CommandResult Execute(GameState state, Command command)
        {
            if (state.IsOver) return CommandResult.Fail("The game is over.");
            if (command.Player != state.ActivePlayer) return CommandResult.Fail("Not your turn.");

            switch (command)
            {
                case PlayCardCommand play: return ExecutePlayCard(state, play);
                case ShiftCommand shift: return ExecuteShift(state, shift);
                case ClearCommand clear: return ExecuteClear(state, clear);
                case ActivateCommand activate: return ExecuteActivate(state, activate);
                case ReplaceCardCommand replace: return ExecuteReplace(state, replace);
                case EndTurnCommand _: return ExecuteEndTurn(state, command.Player);
                default: return CommandResult.Fail("Unknown command.");
            }
        }

        //---- Turn flow ----

        private static void StartTurn(GameState state, List<GameEvent> events)
        {
            state.TurnNumber++;
            int player = state.ActivePlayer;
            var playerState = state.Players[player];

            //Rules-v2: no automatic ramp — refill only; Replace is the sole source of max energy
            playerState.Energy = playerState.MaxEnergy;
            playerState.ReplaceUsedThisTurn = false;
            playerState.PowerUsedThisTurn = false;
            playerState.AbilitiesPlayedThisTurn = 0;

            events.Add(new GameEvent { Type = GameEventType.TurnStarted, Player = player, Amount = state.TurnNumber });
            events.Add(new GameEvent { Type = GameEventType.EnergyChanged, Player = player, Amount = playerState.Energy });

            foreach (var unit in state.UnitsOf(player))
            {
                unit.Flux = false;
                unit.AttackedThisTurn = false;
                unit.MovedThisTurn = false;
                unit.ActivatedThisTurn = false;
                //"Until the start of your next turn" grants expire now; defensive charges refresh
                unit.TempPower = 0;
                unit.TempParry = 0;
                unit.TempEvade = 0;
                unit.TempOverpower = false;
                unit.TempPushOnAttack = false;
                unit.TempDoomed = false;
                unit.ParryUsedThisTurn = 0;
                unit.EvadeUsedThisTurn = 0;
            }

            //Scheduled effects due this turn (Sword of Damocles, Spirit Caller)
            for (int i = state.Pending.Count - 1; i >= 0; i--)
            {
                var pending = state.Pending[i];
                if (pending.Player != player) continue;
                pending.TurnsLeft--;
                if (pending.TurnsLeft > 0) { state.Pending[i] = pending; continue; }
                state.Pending.RemoveAt(i);

                if (pending.IsRebind)
                {
                    var host = state.GetUnit(pending.UnitId);
                    if (host != null && host.BoundSpiritCardId == null && !string.IsNullOrEmpty(pending.CardId))
                    {
                        host.BoundSpiritCardId = pending.CardId;
                        host.SpiritDamage = 0;
                        events.Add(new GameEvent { Type = GameEventType.UnitBonded, UnitId = host.Id, CardId = pending.CardId, X = host.X, Y = host.Y });
                    }
                }
                else
                {
                    var doomed = state.GetUnitAt(pending.X, pending.Y);
                    if (doomed != null) DestroyUnit(state, doomed, events);
                }
            }

            //X-bound (Ocean): destroyed unless standing on the required effect
            foreach (var unit in state.UnitsOf(player).Where(u => u.Definition.BoundTo != SpaceEffectType.None).ToList())
            {
                if (state.Units.Contains(unit) && state.SpaceEffects[unit.X, unit.Y] != unit.Definition.BoundTo)
                    DestroyUnit(state, unit, events);
            }

            //Desert drain (Ocean): -1/-1 at the owner's turn start
            foreach (var unit in state.UnitsOf(player).Where(u => !u.IsCharm && !u.Definition.DesertImmune).ToList())
            {
                if (state.Units.Contains(unit) && state.SpaceEffects[unit.X, unit.Y] == SpaceEffectType.Desert)
                {
                    unit.BonusPower -= 1;
                    unit.BonusLife -= 1;
                    events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = unit.Id, CardId = unit.CardId, Amount = -2 });
                }
            }

            //Poison ticks at the start of the owner's turn (ignores armor and Resist)
            foreach (var unit in state.UnitsOf(player).Where(u => u.Poison > 0).ToList())
            {
                if (state.Units.Contains(unit))
                    DamageUnit(state, unit, unit.Poison, events);
            }

            //Start-of-turn triggers (Living Torrent) for the active player's units
            foreach (var unit in state.UnitsOf(player).ToList())
            {
                if (!state.Units.Contains(unit)) continue;
                foreach (var effect in unit.AllEffects.Where(e => e.Trigger == Trigger.StartOfTurn))
                    ResolveEffect(state, effect, unit, player, unit.X, unit.Y, events);
            }

            //Enemy-turn-start watchers (Grotesque Mirror) belong to the OTHER player
            foreach (var watcher in state.UnitsOf(1 - player).ToList())
            {
                if (!state.Units.Contains(watcher)) continue;
                foreach (var effect in watcher.AllEffects.Where(e => e.Trigger == Trigger.OnEnemyTurnStart))
                    ResolveEffect(state, effect, watcher, watcher.Owner, watcher.X, watcher.Y, events);
            }

            //Verdant regen (doubled under a Sunlamp) and static Regen auras
            int verdantHeal = state.GardenEffectsBoosted ? 2 : 1;
            foreach (var unit in state.UnitsOf(player).ToList())
            {
                if (!state.Units.Contains(unit) || unit.Damage <= 0) continue;
                int heal = 0;
                if (state.SpaceEffects[unit.X, unit.Y] == SpaceEffectType.Verdant) heal += verdantHeal;
                heal += state.EffectiveRegen(unit);
                if (heal > 0) HealUnit(state, unit, heal, events);
            }

            //Auto-Advance (printed or aura-granted), front-most units first so columns compact forward
            var advancers = state.UnitsOf(player)
                .Where(u => !u.IsCharm && !u.Definition.Immobile && state.HasAutoAdvance(u) && !u.Asleep && !u.Pinned)
                .OrderBy(u => player == 0 ? -u.Y : u.Y)
                .ToList();
            foreach (var unit in advancers)
                TryMoveUnit(state, unit, 0, GameState.ForwardDir(player), events);

            DrawCards(state, player, GameConfig.CardsDrawnPerTurn, events);
        }

        private static CommandResult ExecuteEndTurn(GameState state, int player)
        {
            var result = new CommandResult { Success = true };

            //Rules-v3: ALL combat happens here - every eligible unit attacks
            //automatically, front-most first. There are no manual attacks.
            ResolveAutoAttacks(state, player, result.Events);

            if (state.IsOver) return result;

            //End-of-turn triggers for the active player's units, front-most first
            foreach (var unit in state.UnitsOf(player).OrderBy(u => player == 0 ? -u.Y : u.Y).ToList())
            {
                if (!state.Units.Contains(unit)) continue;
                foreach (var effect in unit.AllEffects.Where(e => e.Trigger == Trigger.EndOfTurn))
                    ResolveEffect(state, effect, unit, player, unit.X, unit.Y, result.Events);
            }

            //Give Your All: doomed units burn out now
            foreach (var unit in state.UnitsOf(player).Where(u => u.TempDoomed).ToList())
            {
                if (state.Units.Contains(unit)) DestroyUnit(state, unit, result.Events);
            }

            //Inferno: the ending player's units standing in fire take 1 (Heart)
            foreach (var unit in state.UnitsOf(player).Where(u => !u.IsCharm).ToList())
            {
                if (state.Units.Contains(unit) && state.SpaceEffects[unit.X, unit.Y] == SpaceEffectType.Inferno)
                    DamageUnit(state, unit, 1, result.Events);
            }

            //Pins on the ending player's units expire now (rules-v2)
            foreach (var unit in state.UnitsOf(player))
                unit.Pinned = false;

            result.Events.Add(new GameEvent { Type = GameEventType.TurnEnded, Player = player });

            if (!state.IsOver)
            {
                state.ActivePlayer = 1 - player;
                StartTurn(state, result.Events);
            }

            return result;
        }

        //---- Play card ----

        private static CommandResult ExecutePlayCard(GameState state, PlayCardCommand command)
        {
            var playerState = state.Players[command.Player];

            if (command.HandIndex < 0 || command.HandIndex >= playerState.Hand.Count)
                return CommandResult.Fail("Invalid hand index.");

            string cardId = playerState.Hand[command.HandIndex];
            var definition = CardCatalogV1.Get(cardId);
            int cost = state.EffectiveCost(command.Player, definition);

            //Positional call discounts (Trailblazer, Flagbearer)
            if (definition.Type != CardType.Ability && definition.PlayTarget == PlayTargetKind.FriendlyEmptySpace)
                cost = Math.Max(0, cost - state.CallDiscountAt(command.Player, command.TargetX, command.TargetY));

            if (playerState.Energy < cost)
                return CommandResult.Fail($"Not enough energy for {cardId} (need {cost}, have {playerState.Energy}).");

            if (playerState.Affinity[definition.Archetype] < definition.AffinityRequirement)
                return CommandResult.Fail($"{cardId} requires {definition.Archetype} Affinity {definition.AffinityRequirement} (have {playerState.Affinity[definition.Archetype]}).");

            string targetError = ValidatePlayTarget(state, command.Player, definition, command.TargetX, command.TargetY);
            if (targetError != null) return CommandResult.Fail(targetError);

            var result = new CommandResult { Success = true };

            playerState.Hand.RemoveAt(command.HandIndex);
            playerState.Energy -= cost;
            result.Events.Add(new GameEvent { Type = GameEventType.CardPlayed, Player = command.Player, CardId = cardId, X = command.TargetX, Y = command.TargetY });
            result.Events.Add(new GameEvent { Type = GameEventType.EnergyChanged, Player = command.Player, Amount = playerState.Energy });

            if (definition.Type == CardType.Ability)
            {
                foreach (var effect in definition.Effects.Where(e => e.Trigger == Trigger.OnPlay))
                    ResolveEffect(state, effect, null, command.Player, command.TargetX, command.TargetY, result.Events);

                playerState.AbilitiesPlayedThisTurn++;
                playerState.NextAbilityDiscount = 0; //Attenuating Rod's rebate is spent

                //Ability-play watchers (Diligent Student, Lightning Rod, Combat Bellows)
                foreach (var watcher in state.Units.ToList())
                {
                    if (!state.Units.Contains(watcher)) continue;
                    var trigger = watcher.Owner == command.Player ? Trigger.OnOwnerAbilityPlay : Trigger.OnEnemyAbilityPlay;
                    foreach (var effect in watcher.AllEffects.Where(e => e.Trigger == trigger))
                        ResolveEffect(state, effect, watcher, watcher.Owner, watcher.X, watcher.Y, result.Events);
                }
            }
            else if (definition.IsSpirit)
            {
                //Spirit Bind: attach to the friendly host; rebinding overwrites
                //the old spirit without triggering its break (design notes)
                var host = state.GetUnitAt(command.TargetX, command.TargetY);
                host.BoundSpiritCardId = cardId;
                host.SpiritDamage = 0;
                result.Events.Add(new GameEvent { Type = GameEventType.UnitBonded, UnitId = host.Id, CardId = cardId, X = host.X, Y = host.Y });

                foreach (var effect in host.AllEffects.Where(e => e.Trigger == Trigger.OnBonded).ToList())
                    ResolveEffect(state, effect, host, host.Owner, host.X, host.Y, result.Events);
            }
            else
            {
                CallUnit(state, command.Player, cardId, command.TargetX, command.TargetY, result.Events);

                //Next-call riders (Virtuous Call, Valorous Call) bless this arrival
                if (definition.Type == CardType.Unit &&
                    (playerState.NextCallDiscount > 0 || playerState.NextCallPower != 0 || playerState.NextCallLife != 0 || playerState.NextCallGranted != null))
                {
                    var blessed = state.GetUnitAt(command.TargetX, command.TargetY);
                    if (blessed != null)
                    {
                        blessed.BonusPower += playerState.NextCallPower;
                        blessed.BonusLife += playerState.NextCallLife;
                        if (playerState.NextCallGranted != null)
                        {
                            if (blessed.GrantedEffects == null) blessed.GrantedEffects = new List<EffectDef>();
                            blessed.GrantedEffects.Add(playerState.NextCallGranted);
                        }
                        result.Events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = blessed.Id, CardId = blessed.CardId });
                    }
                    playerState.NextCallDiscount = 0;
                    playerState.NextCallPower = 0;
                    playerState.NextCallLife = 0;
                    playerState.NextCallGranted = null;
                }
            }

            state.LastCardPlayed = cardId; //after resolution: Trace copies the card played before it
            return result;
        }

        private static string ValidatePlayTarget(GameState state, int player, CardDefinition definition, int x, int y)
        {
            switch (definition.PlayTarget)
            {
                case PlayTargetKind.None:
                    return null;
                case PlayTargetKind.FriendlyEmptySpace:
                    if (!GameState.InBounds(x, y)) return "Target space is out of bounds.";
                    if (GameState.SideOfRow(y) != player) return "Units must be called to your half of the field.";
                    if (state.GetUnitAt(x, y) != null && FriendlyEquipAt(state, player, x, y) == null) return "Target space is occupied.";
                    return null;
                case PlayTargetKind.AnySpace:
                    return GameState.InBounds(x, y) ? null : "Target space is out of bounds.";
                case PlayTargetKind.AnyUnit:
                    if (!GameState.InBounds(x, y)) return "Target space is out of bounds.";
                    return state.GetUnitAt(x, y) != null ? null : "No unit on target space.";
                case PlayTargetKind.FriendlyUnit:
                {
                    if (!GameState.InBounds(x, y)) return "Target space is out of bounds.";
                    var host = state.GetUnitAt(x, y);
                    if (host == null || host.Owner != player) return "Spirits bind to your own units.";
                    if (host.IsCharm) return "Spirits cannot bind to charms.";
                    if (host.Definition.IsSpirit) return "Spirits cannot bind to spirits.";
                    return null;
                }
                default:
                    return "Unknown target kind.";
            }
        }

        private static void CallUnit(GameState state, int player, string cardId, int x, int y, List<GameEvent> events)
        {
            var definition = CardCatalogV1.Get(cardId);

            //Calling onto a friendly Equip charm consumes it (Adaptive Armature accepts charms)
            var equip = FriendlyEquipAt(state, player, x, y, forCharm: definition.Type == CardType.Charm);
            if (equip != null)
            {
                state.Units.Remove(equip);
            }

            var unit = new UnitState
            {
                Id = state.NextUnitId++,
                CardId = cardId,
                Owner = player,
                X = x,
                Y = y,
                Flux = definition.Type == CardType.Unit && !definition.Rush
            };
            state.Units.Add(unit);

            events.Add(new GameEvent { Type = GameEventType.UnitCalled, Player = player, UnitId = unit.Id, CardId = cardId, X = x, Y = y });

            //Vista: units called to this space gain +1/+1
            if (state.SpaceEffects[x, y] == SpaceEffectType.Vista)
            {
                unit.BonusPower += 1;
                unit.BonusLife += 1;
                events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = unit.Id, CardId = cardId, Amount = 1 });
            }

            //Flooded: units called here mutate (Ocean)
            if (state.SpaceEffects[x, y] == SpaceEffectType.Flooded && definition.Type != CardType.Charm)
                GrantRandomKeyword(state, unit, events);

            //Consumed equip bestows onto the arrival before anything else reacts
            if (equip != null && state.Units.Contains(unit))
            {
                events.Add(new GameEvent { Type = GameEventType.EquipAttached, UnitId = unit.Id, CardId = equip.CardId, X = x, Y = y });
                foreach (var effect in equip.Definition.Effects.Where(e => e.Trigger == Trigger.OnEquip))
                    ResolveEffect(state, effect, unit, player, x, y, events);
                foreach (var effect in unit.AllEffects.Where(e => e.Trigger == Trigger.OnEquipped).ToList())
                {
                    if (!state.Units.Contains(unit)) break;
                    ResolveEffect(state, effect, unit, player, x, y, events);
                }
            }

            foreach (var effect in definition.Effects.Where(e => e.Trigger == Trigger.OnCall))
                ResolveEffect(state, effect, unit, player, x, y, events);

            //Friendly call-watchers (Squad Leader, Skilled Armorer): target coords are the called unit's space
            foreach (var watcher in state.UnitsOf(player).ToList())
            {
                if (watcher == unit || !state.Units.Contains(watcher)) continue;
                bool adjacent = Math.Abs(watcher.X - x) + Math.Abs(watcher.Y - y) == 1;
                bool inFront = x == watcher.X && y == watcher.Y + GameState.ForwardDir(player);
                foreach (var effect in watcher.AllEffects)
                {
                    if ((effect.Trigger == Trigger.OnAllyCallAdjacent && adjacent) ||
                        (effect.Trigger == Trigger.OnAllyCallInFront && inFront))
                        ResolveEffect(state, effect, watcher, player, x, y, events);
                }
            }

            //Opposing triggers (e.g. Guest Registry) fire when a Unit is summoned
            if (definition.Type == CardType.Unit)
            {
                foreach (var watcher in state.UnitsOf(1 - player).ToList())
                {
                    if (!state.Units.Contains(watcher)) continue;
                    foreach (var effect in watcher.AllEffects.Where(e => e.Trigger == Trigger.OnEnemyCall))
                        ResolveEffect(state, effect, watcher, watcher.Owner, watcher.X, watcher.Y, events);
                }
            }
        }

        //---- Shift ----

        private static CommandResult ExecuteShift(GameState state, ShiftCommand command)
        {
            var playerState = state.Players[command.Player];
            var unit = state.GetUnit(command.UnitId);

            if (unit == null) return CommandResult.Fail("No such unit.");
            if (unit.Owner != command.Player) return CommandResult.Fail("You don't control that unit.");
            if (unit.IsCharm) return CommandResult.Fail("Charms are immobile.");
            if (unit.Definition.Immobile) return CommandResult.Fail("That unit is Immobile.");
            if (unit.Asleep) return CommandResult.Fail("That unit is asleep.");
            if (unit.Pinned) return CommandResult.Fail("That unit is pinned.");
            if (unit.AttackedThisTurn && !unit.Definition.Agile) return CommandResult.Fail("That unit has already attacked this turn.");
            if (playerState.PowerUsedThisTurn) return CommandResult.Fail("Your power action (Shift or Clear) is already spent this turn.");

            int dx = 0, dy = 0;
            switch (command.Direction)
            {
                case MoveDirection.Forward: dy = GameState.ForwardDir(command.Player); break;
                case MoveDirection.Back: dy = -GameState.ForwardDir(command.Player); break;
                case MoveDirection.Left: dx = -1; break;
                case MoveDirection.Right: dx = 1; break;
            }

            int destX = unit.X + dx, destY = unit.Y + dy;
            if (!GameState.InBounds(destX, destY)) return CommandResult.Fail("Cannot Shift off the field.");
            if (GameState.SideOfRow(destY) != command.Player) return CommandResult.Fail("Units must stay on your half of the field.");

            var occupant = state.GetUnitAt(destX, destY);
            bool ontoEquip = occupant != null && FriendlyEquipAt(state, command.Player, destX, destY) != null;
            if (occupant != null && occupant.IsCharm && !ontoEquip) return CommandResult.Fail("Cannot Shift onto a charm.");

            //Rugged terrain: entering costs one extra energy
            int shiftCost = GameConfig.ShiftEnergyCost + (state.SpaceEffects[destX, destY] == SpaceEffectType.Rugged ? 1 : 0);
            if (playerState.Energy < shiftCost) return CommandResult.Fail("Not enough energy to Shift.");

            var result = new CommandResult { Success = true };
            playerState.Energy -= shiftCost;
            playerState.PowerUsedThisTurn = true;
            result.Events.Add(new GameEvent { Type = GameEventType.EnergyChanged, Player = command.Player, Amount = playerState.Energy });

            if (occupant != null && !ontoEquip)
            {
                //Occupied by a friendly unit: the units switch places
                MoveUnitTo(state, occupant, unit.X, unit.Y, result.Events);
                occupant.MovedThisTurn = true;
            }

            MoveUnitTo(state, unit, destX, destY, result.Events);
            unit.MovedThisTurn = true;

            //Shift triggers (Duelist, Dancer) fire for the commanded unit only
            if (state.Units.Contains(unit))
            {
                foreach (var effect in unit.AllEffects.Where(e => e.Trigger == Trigger.OnShift))
                    ResolveEffect(state, effect, unit, unit.Owner, unit.X, unit.Y, result.Events);
            }

            return result;
        }

        //---- Attack ----

        /// <summary>
        /// The end-of-turn combat step (rules-v3: the ONLY combat step): every
        /// eligible unit of the given player attacks, front-most first. Public
        /// so tests and tools can resolve combat without ending a turn.
        /// </summary>
        public static void ResolveAutoAttacks(GameState state, int player, List<GameEvent> events)
        {
            foreach (var unit in state.UnitsOf(player).OrderBy(u => player == 0 ? -u.Y : u.Y).ToList())
            {
                if (state.IsOver) break;
                if (!state.Units.Contains(unit)) continue;
                TryUnitAttack(state, unit, events);
            }
        }

        /// <summary>One unit's auto-attack; silently skips ineligible units.</summary>
        private static void TryUnitAttack(GameState state, UnitState attacker, List<GameEvent> events)
        {
            if (attacker.IsCharm || attacker.Asleep) return;
            bool hasRush = attacker.Definition.Rush || (attacker.BoundSpirit != null && attacker.BoundSpirit.Rush);
            if (attacker.Flux && !hasRush) return;
            if (attacker.AttackedThisTurn) return;
            if (attacker.MovedThisTurn && !attacker.Definition.Agile) return;
            if (state.EffectivePower(attacker) <= 0) return;
            if (!attacker.Definition.Ranged && HasFriendlyUnitInFront(state, attacker)) return;

            attacker.AttackedThisTurn = true;

            int power = state.EffectivePower(attacker);
            int forward = GameState.ForwardDir(attacker.Owner);
            int enemy = 1 - attacker.Owner;

            //Flying (spirit-granted): the attack passes over blockers entirely
            bool flying = attacker.Definition.Flying || (attacker.BoundSpirit != null && attacker.BoundSpirit.Flying);

            //Scan the enemy half of the lane from their frontline backwards
            int firstTargetY = -1;
            if (!flying)
            {
                for (int y = GameState.FrontlineRow(enemy); GameState.SideOfRow(y) == enemy && GameState.InBounds(attacker.X, y); y += forward)
                {
                    if (state.GetUnitAt(attacker.X, y) != null) { firstTargetY = y; break; }
                }
            }

            events.Add(new GameEvent { Type = GameEventType.AttackResolved, Player = attacker.Owner, UnitId = attacker.Id, CardId = attacker.CardId, X = attacker.X, Y = attacker.Y });

            if (firstTargetY < 0)
            {
                //Unblocked lane (or Flying): hit the opposing player directly.
                //Spirit of Opportunity doubles genuinely unblocked hits.
                bool doubled = state.LaneUnblockedFor(attacker.Owner, attacker.X) &&
                               (attacker.Definition.DoubleWhenUnblocked ||
                                (attacker.BoundSpirit != null && attacker.BoundSpirit.DoubleWhenUnblocked));
                DamagePlayer(state, enemy, doubled ? power * 2 : power, events);
                return;
            }

            var target = state.GetUnitAt(attacker.X, firstTargetY);
            StrikeUnit(state, attacker, target, power, events);

            //OnAttack triggers (e.g. Master Painter) aim at the struck unit's space
            if (state.Units.Contains(attacker))
            {
                foreach (var effect in attacker.AllEffects.Where(e => e.Trigger == Trigger.OnAttack))
                    ResolveEffect(state, effect, attacker, attacker.Owner, attacker.X, firstTargetY, events);
            }

            //Pierce: the attack also travels X spaces beyond the target
            for (int n = 1; n <= attacker.Definition.Pierce + attacker.BonusPierce; n++)
            {
                int y = firstTargetY + forward * n;
                if (!GameState.InBounds(attacker.X, y)) break;
                var pierced = state.GetUnitAt(attacker.X, y);
                if (pierced != null && pierced.Owner == enemy)
                    StrikeUnit(state, attacker, pierced, power, events, allowRiders: false);
            }
        }

        //---- Clear (the alternate player power, from the sheet's Powers table) ----

        private static CommandResult ExecuteClear(GameState state, ClearCommand command)
        {
            var playerState = state.Players[command.Player];

            if (playerState.PowerUsedThisTurn) return CommandResult.Fail("Your power action (Shift or Clear) is already spent this turn.");
            if (playerState.Energy < GameConfig.ClearEnergyCost) return CommandResult.Fail("Not enough energy to Clear.");
            if (!GameState.InBounds(command.X, command.Y)) return CommandResult.Fail("Target space is out of bounds.");
            if (state.SpaceEffects[command.X, command.Y] == SpaceEffectType.None) return CommandResult.Fail("No space effect to Clear.");

            var result = new CommandResult { Success = true };
            playerState.Energy -= GameConfig.ClearEnergyCost;
            playerState.PowerUsedThisTurn = true;
            state.SpaceEffects[command.X, command.Y] = SpaceEffectType.None;
            result.Events.Add(new GameEvent { Type = GameEventType.EnergyChanged, Player = command.Player, Amount = playerState.Energy });
            result.Events.Add(new GameEvent { Type = GameEventType.SpaceEffectApplied, X = command.X, Y = command.Y, SpaceEffect = SpaceEffectType.None });
            return result;
        }

        //---- Activation (rules-v3: the manual per-unit action) ----

        private static CommandResult ExecuteActivate(GameState state, ActivateCommand command)
        {
            var unit = state.GetUnit(command.UnitId);
            var playerState = state.Players[command.Player];

            if (unit == null) return CommandResult.Fail("No such unit.");
            if (unit.Owner != command.Player) return CommandResult.Fail("You don't control that unit.");
            if (unit.Definition.ActivateCost < 0) return CommandResult.Fail("That unit has no activatable ability.");
            if (unit.Asleep) return CommandResult.Fail("That unit is asleep.");
            if (unit.Flux) return CommandResult.Fail("That unit was called this turn.");
            if (unit.ActivatedThisTurn) return CommandResult.Fail("Already activated this turn.");
            if (playerState.Energy < unit.Definition.ActivateCost) return CommandResult.Fail("Not enough energy to activate.");

            var result = new CommandResult { Success = true };
            playerState.Energy -= unit.Definition.ActivateCost;
            unit.ActivatedThisTurn = true;
            result.Events.Add(new GameEvent { Type = GameEventType.EnergyChanged, Player = command.Player, Amount = playerState.Energy });

            foreach (var effect in unit.AllEffects.Where(e => e.Trigger == Trigger.OnActivate).ToList())
            {
                if (!state.Units.Contains(unit)) break;
                ResolveEffect(state, effect, unit, unit.Owner, unit.X, unit.Y, result.Events);
            }

            return result;
        }

        private static void StrikeUnit(GameState state, UnitState attacker, UnitState target, int power, List<GameEvent> events, bool allowRiders = true)
        {
            //Guardian: a friendly Guardian beside or behind the target takes the strike
            //in its place (no chains - a redirected strike is not redirected again)
            if (!target.Definition.Guardian)
            {
                var guardian = state.UnitsOf(target.Owner).FirstOrDefault(g =>
                    g.Definition.Guardian && g != target && !g.Asleep &&
                    ((g.Y == target.Y && Math.Abs(g.X - target.X) == 1) ||
                     (g.X == target.X && g.Y == target.Y - GameState.ForwardDir(target.Owner))));
                if (guardian != null)
                {
                    StrikeGuarded(state, attacker, guardian, power, events, allowRiders);
                    if (state.Units.Contains(guardian))
                    {
                        foreach (var effect in guardian.AllEffects.Where(e => e.Trigger == Trigger.OnGuard))
                            ResolveEffect(state, effect, guardian, guardian.Owner, guardian.X, guardian.Y, events);
                    }
                    return;
                }
            }

            StrikeGuarded(state, attacker, target, power, events, allowRiders);
        }

        private static void StrikeGuarded(GameState state, UnitState attacker, UnitState target, int power, List<GameEvent> events, bool allowRiders)
        {
            state.LastCombatAttackerId = attacker.Id; //for Reprisal-style break effects
            int lifeBefore = state.CurrentLife(target);
            int targetX = target.X, targetY = target.Y;
            int damage = Math.Max(0, power - state.EffectiveArmor(target));

            //Parry: prevent combat damage, consuming one charge (riders still apply)
            if (damage > 0 && state.EffectiveParry(target) - target.ParryUsedThisTurn > 0)
            {
                target.ParryUsedThisTurn++;
                damage = 0;
                events.Add(new GameEvent { Type = GameEventType.AttackParried, UnitId = target.Id, CardId = target.CardId });
            }

            if (damage > 0) DamageUnit(state, target, damage, events);

            bool targetAlive = state.Units.Contains(target);

            //Push: after the attack, shove a surviving target one space away
            if (allowRiders && targetAlive && (attacker.Definition.PushOnAttack || attacker.TempPushOnAttack))
                PushUnit(state, target, GameState.ForwardDir(attacker.Owner), events);

            //Overpower (Give Your All): excess kill damage rolls onto the unit behind
            if (!targetAlive && attacker.TempOverpower && damage > lifeBefore)
            {
                var behind = state.GetUnitAt(targetX, targetY + GameState.ForwardDir(attacker.Owner));
                if (behind != null && behind.Owner != attacker.Owner)
                    DamageUnit(state, behind, damage - lifeBefore, events);
            }

            //Retaliate: a surviving defender strikes back (ignores armor: effect damage)
            if (targetAlive && target.Definition.Retaliate > 0 && state.Units.Contains(attacker))
                DamageUnit(state, attacker, target.Definition.Retaliate, events);

            //OnDealtDamage (Ferocity, Reverse Engineer): target coords = victim's space
            if (damage > 0 && state.Units.Contains(attacker))
            {
                foreach (var effect in attacker.AllEffects.Where(e => e.Trigger == Trigger.OnDealtDamage).ToList())
                    ResolveEffect(state, effect, attacker, attacker.Owner, target.X, target.Y, events);
            }

            //OnKill (Prideful Soul): the attacker's strike destroyed the target
            if (!targetAlive && state.Units.Contains(attacker))
            {
                foreach (var effect in attacker.AllEffects.Where(e => e.Trigger == Trigger.OnKill).ToList())
                    ResolveEffect(state, effect, attacker, attacker.Owner, attacker.X, attacker.Y, events);

                //OnFriendlyKill watchers (Covenant of Valor): target coords = the killer
                foreach (var watcher in state.UnitsOf(attacker.Owner).ToList())
                {
                    if (watcher == attacker || !state.Units.Contains(watcher)) continue;
                    foreach (var effect in watcher.AllEffects.Where(e => e.Trigger == Trigger.OnFriendlyKill))
                        ResolveEffect(state, effect, watcher, watcher.Owner, attacker.X, attacker.Y, events);
                }
            }
        }

        private static bool HasFriendlyUnitInFront(GameState state, UnitState unit)
        {
            int forward = GameState.ForwardDir(unit.Owner);
            for (int y = unit.Y + forward; GameState.InBounds(unit.X, y) && GameState.SideOfRow(y) == unit.Owner; y += forward)
            {
                var other = state.GetUnitAt(unit.X, y);
                if (other != null && other.Owner == unit.Owner) return true;
            }
            return false;
        }

        //---- Replace (the ramp economy) ----

        private static CommandResult ExecuteReplace(GameState state, ReplaceCardCommand command)
        {
            var playerState = state.Players[command.Player];

            if (playerState.ReplaceUsedThisTurn) return CommandResult.Fail("Replace has already been used this turn.");
            if (command.HandIndex < 0 || command.HandIndex >= playerState.Hand.Count)
                return CommandResult.Fail("Invalid hand index.");

            string cardId = playerState.Hand[command.HandIndex];
            var definition = CardCatalogV1.Get(cardId);

            playerState.Hand.RemoveAt(command.HandIndex);
            playerState.ReplaceUsedThisTurn = true;
            playerState.MaxEnergy += 1;
            playerState.Energy += 1;
            playerState.Affinity[definition.Archetype] += 1;

            var result = new CommandResult { Success = true };
            result.Events.Add(new GameEvent { Type = GameEventType.CardReplaced, Player = command.Player, CardId = cardId });
            result.Events.Add(new GameEvent { Type = GameEventType.EnergyChanged, Player = command.Player, Amount = playerState.Energy });
            result.Events.Add(new GameEvent { Type = GameEventType.AffinityGained, Player = command.Player, CardId = definition.Archetype.ToString(), Amount = playerState.Affinity[definition.Archetype] });
            return result;
        }

        //---- Effect resolution ----

        private static void ResolveEffect(GameState state, EffectDef effect, UnitState source, int owner, int targetX, int targetY, List<GameEvent> events)
        {
            //Conditions apply to triggered effects too (static effects re-check in GameState)
            if (effect.Condition == EffectCondition.Frontline &&
                (source == null || source.Y != GameState.FrontlineRow(owner))) return;
            if (effect.Condition == EffectCondition.Unblocked &&
                (source == null || !state.LaneUnblockedFor(owner, source.X))) return;

            switch (effect.Action)
            {
                case EffectAction.GainStats:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                    {
                        //SpaceEffect doubles as a standing-on condition for stat gains
                        if (effect.SpaceEffect != SpaceEffectType.None &&
                            state.SpaceEffects[unit.X, unit.Y] != effect.SpaceEffect) continue;

                        unit.BonusPower += effect.Power;
                        unit.BonusLife += effect.Life;
                        events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = unit.Id, CardId = unit.CardId, Amount = effect.Power + effect.Life });
                    }
                    break;
                }
                case EffectAction.Heal:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                        HealUnit(state, unit, effect.Amount, events);
                    break;
                }
                case EffectAction.ApplySpaceEffect:
                {
                    bool appliedAny = false;
                    foreach (var (x, y) in GatherSpaces(state, effect.Scope, source, targetX, targetY))
                    {
                        state.SpaceEffects[x, y] = effect.SpaceEffect;
                        events.Add(new GameEvent { Type = GameEventType.SpaceEffectApplied, X = x, Y = y, SpaceEffect = effect.SpaceEffect });
                        appliedAny = true;
                    }

                    //Space-effect watchers (Navigator) fire once per application effect
                    if (appliedAny)
                    {
                        foreach (var watcher in state.UnitsOf(owner).ToList())
                        {
                            if (!state.Units.Contains(watcher)) continue;
                            foreach (var watcherEffect in watcher.AllEffects.Where(e => e.Trigger == Trigger.OnOwnerSpaceEffect))
                                ResolveEffect(state, watcherEffect, watcher, owner, watcher.X, watcher.Y, events);
                        }
                    }
                    break;
                }
                case EffectAction.AdvanceAllFriendly:
                {
                    var movers = state.UnitsOf(owner)
                        .Where(u => !u.IsCharm && !u.Asleep)
                        .OrderBy(u => owner == 0 ? -u.Y : u.Y)
                        .ToList();
                    foreach (var unit in movers)
                    {
                        if (state.Units.Contains(unit))
                            TryMoveUnit(state, unit, 0, GameState.ForwardDir(owner), events);
                    }
                    break;
                }
                case EffectAction.Draw:
                    DrawCards(state, owner, effect.Amount, events);
                    break;
                case EffectAction.GainEnergy:
                    state.Players[owner].Energy += effect.Amount;
                    events.Add(new GameEvent { Type = GameEventType.EnergyChanged, Player = owner, Amount = state.Players[owner].Energy });
                    break;
                case EffectAction.SetAsleep:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                    {
                        if (unit.IsCharm) continue;
                        unit.Asleep = true;
                        events.Add(new GameEvent { Type = GameEventType.UnitFellAsleep, UnitId = unit.Id, CardId = unit.CardId });
                    }
                    break;
                }
                case EffectAction.GainPowerPerSpaceEffect:
                case EffectAction.GainLifePerSpaceEffect:
                {
                    if (source == null) break;
                    int count = CountSpaces(state, effect.SpaceEffect);
                    if (count == 0) break;
                    if (effect.Action == EffectAction.GainPowerPerSpaceEffect) source.BonusPower += count;
                    else source.BonusLife += count;
                    events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = source.Id, CardId = source.CardId, Amount = count });
                    break;
                }
                case EffectAction.DealDamage:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                        DealAbilityDamage(state, unit, effect.Amount, events);
                    break;
                }
                case EffectAction.LaneDamage:
                {
                    //Sweep the target lane from the caster's side; the traveling damage
                    //changes by Amount per unit hit and the sweep stops when it reaches 0.
                    int damage = effect.Power;
                    int startY = GameState.BacklineRow(owner);
                    int direction = GameState.ForwardDir(owner);
                    for (int y = startY; GameState.InBounds(targetX, y); y += direction)
                    {
                        if (damage <= 0) break;
                        var victim = state.GetUnitAt(targetX, y);
                        if (victim == null) continue;
                        DealAbilityDamage(state, victim, damage, events);
                        damage += effect.Amount;
                    }
                    break;
                }
                case EffectAction.ClearSpaceEffect:
                {
                    if (state.SpaceEffects[targetX, targetY] == SpaceEffectType.None) break;
                    state.SpaceEffects[targetX, targetY] = SpaceEffectType.None;
                    events.Add(new GameEvent { Type = GameEventType.SpaceEffectApplied, X = targetX, Y = targetY, SpaceEffect = SpaceEffectType.None });

                    var occupant = state.GetUnitAt(targetX, targetY);
                    if (occupant != null && effect.Amount > 0)
                        DealAbilityDamage(state, occupant, effect.Amount, events);
                    break;
                }
                case EffectAction.GainPierce:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                    {
                        unit.BonusPierce += effect.Amount;
                        events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = unit.Id, CardId = unit.CardId, Amount = effect.Amount });
                    }
                    break;
                }
                case EffectAction.PushAway:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY).ToList())
                    {
                        if (unit.IsCharm) continue; //charms are immobile
                        if (state.Units.Contains(unit))
                            PushUnit(state, unit, GameState.ForwardDir(owner), events);
                    }
                    break;
                }
                case EffectAction.Pin:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                    {
                        if (unit.IsCharm || !state.Units.Contains(unit)) continue;
                        unit.Pinned = true;
                        events.Add(new GameEvent { Type = GameEventType.UnitPinned, UnitId = unit.Id, CardId = unit.CardId });
                    }
                    break;
                }
                case EffectAction.Poison:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                    {
                        if (unit.IsCharm || !state.Units.Contains(unit)) continue;
                        unit.Poison += effect.Amount;
                        events.Add(new GameEvent { Type = GameEventType.UnitPoisoned, UnitId = unit.Id, CardId = unit.CardId, Amount = unit.Poison });
                    }
                    break;
                }
                case EffectAction.CallUnit:
                {
                    //Candidates: empty spaces on the owner's half, filtered by SpaceEffect
                    var candidates = new List<(int x, int y)>();
                    for (int x = 0; x < GameConfig.Lanes; x++)
                        for (int y = 0; y < GameConfig.Rows; y++)
                            if (GameState.SideOfRow(y) == owner && state.GetUnitAt(x, y) == null &&
                                (effect.SpaceEffect == SpaceEffectType.None || state.SpaceEffects[x, y] == effect.SpaceEffect))
                                candidates.Add((x, y));

                    if (effect.Amount > 0)
                    {
                        //Random picks, deterministic through the seeded RNG
                        for (int n = 0; n < effect.Amount && candidates.Count > 0; n++)
                        {
                            int pick = state.NextRandom(candidates.Count);
                            var (x, y) = candidates[pick];
                            candidates.RemoveAt(pick);
                            CallUnit(state, owner, effect.CalledCardId, x, y, events);
                        }
                    }
                    else
                    {
                        foreach (var (x, y) in candidates)
                            CallUnit(state, owner, effect.CalledCardId, x, y, events);
                    }
                    break;
                }
                case EffectAction.Pull:
                {
                    //One step toward the effect's owner; same collision rules as Push
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY).ToList())
                    {
                        if (unit.IsCharm) continue; //charms are immobile
                        if (state.Units.Contains(unit))
                            PushUnit(state, unit, -GameState.ForwardDir(owner), events);
                    }
                    break;
                }
                case EffectAction.GainTempPower:
                case EffectAction.GainTempParry:
                case EffectAction.GainTempEvade:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                    {
                        if (unit.IsCharm) continue;
                        if (effect.Action == EffectAction.GainTempPower) unit.TempPower += effect.Power;
                        else if (effect.Action == EffectAction.GainTempParry) unit.TempParry += effect.Amount;
                        else unit.TempEvade += effect.Amount;
                        events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = unit.Id, CardId = unit.CardId, Amount = effect.Power + effect.Amount });
                    }
                    break;
                }
                case EffectAction.GainArmor:
                case EffectAction.GainParry:
                case EffectAction.GainResist:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                    {
                        if (effect.Action == EffectAction.GainArmor) unit.BonusArmor += effect.Amount;
                        else if (effect.Action == EffectAction.GainParry) unit.BonusParry += effect.Amount;
                        else unit.BonusResist += effect.Amount;
                        events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = unit.Id, CardId = unit.CardId, Amount = effect.Amount });
                    }
                    break;
                }
                case EffectAction.GrantHeavy:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                    {
                        if (unit.IsCharm) continue;
                        unit.GrantedHeavy = true;
                        events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = unit.Id, CardId = unit.CardId });
                    }
                    break;
                }
                case EffectAction.TutorLowCost:
                {
                    var playerState = state.Players[owner];
                    int index = playerState.Deck.FindIndex(id => CardCatalogV1.Get(id).Cost <= effect.Amount);
                    if (index < 0) break;
                    string found = playerState.Deck[index];
                    playerState.Deck.RemoveAt(index);

                    if (CardCatalogV1.Get(found).Type == CardType.Unit)
                    {
                        //Call it to a random empty space on the owner's half; hand if none
                        var open = new List<(int x, int y)>();
                        for (int x = 0; x < GameConfig.Lanes; x++)
                            for (int y = 0; y < GameConfig.Rows; y++)
                                if (GameState.SideOfRow(y) == owner && state.GetUnitAt(x, y) == null)
                                    open.Add((x, y));
                        if (open.Count > 0)
                        {
                            var (cx, cy) = open[state.NextRandom(open.Count)];
                            CallUnit(state, owner, found, cx, cy, events);
                            break;
                        }
                    }
                    playerState.Hand.Add(found);
                    events.Add(new GameEvent { Type = GameEventType.CardDrawn, Player = owner, CardId = found });
                    break;
                }
                case EffectAction.AddRandomAbility:
                {
                    var abilities = CardCatalogV1.Cards.Values.Where(c => c.Type == CardType.Ability).OrderBy(c => c.Id).ToList();
                    if (abilities.Count == 0) break;
                    string pick = abilities[state.NextRandom(abilities.Count)].Id;
                    state.Players[owner].Hand.Add(pick);
                    events.Add(new GameEvent { Type = GameEventType.CardDrawn, Player = owner, CardId = pick });
                    break;
                }
                case EffectAction.CopyLastCard:
                {
                    if (string.IsNullOrEmpty(state.LastCardPlayed)) break;
                    state.Players[owner].Hand.Add(state.LastCardPlayed);
                    events.Add(new GameEvent { Type = GameEventType.CardDrawn, Player = owner, CardId = state.LastCardPlayed });
                    break;
                }
                case EffectAction.TransformSelf:
                {
                    if (source == null || string.IsNullOrEmpty(effect.CalledCardId) || !state.Units.Contains(source)) break;
                    source.CardId = effect.CalledCardId;
                    events.Add(new GameEvent { Type = GameEventType.UnitTransformed, UnitId = source.Id, CardId = source.CardId, X = source.X, Y = source.Y });
                    break;
                }
                case EffectAction.DamageBothPlayers:
                {
                    DamagePlayer(state, 0, effect.Amount, events);
                    if (!state.IsOver) DamagePlayer(state, 1, effect.Amount, events);
                    break;
                }
                case EffectAction.ApplySpaceEffectMirrored:
                {
                    if (source == null) break;
                    state.SpaceEffects[source.X, source.Y] = effect.SpaceEffect;
                    events.Add(new GameEvent { Type = GameEventType.SpaceEffectApplied, X = source.X, Y = source.Y, SpaceEffect = effect.SpaceEffect });
                    int mirrorY = GameConfig.Rows - 1 - source.Y;
                    state.SpaceEffects[source.X, mirrorY] = effect.SpaceEffect;
                    events.Add(new GameEvent { Type = GameEventType.SpaceEffectApplied, X = source.X, Y = mirrorY, SpaceEffect = effect.SpaceEffect });
                    break;
                }
                case EffectAction.AddRandomSpirit:
                {
                    var spirits = CardCatalogV1.Cards.Values.Where(c => c.IsSpirit).OrderBy(c => c.Id).ToList();
                    if (spirits.Count == 0) break;
                    string pick = spirits[state.NextRandom(spirits.Count)].Id;
                    state.Players[owner].Hand.Add(pick);
                    events.Add(new GameEvent { Type = GameEventType.CardDrawn, Player = owner, CardId = pick });
                    break;
                }
                case EffectAction.SacrificeLaneBurst:
                {
                    var sacrifice = state.GetUnitAt(targetX, targetY);
                    if (sacrifice == null || sacrifice.Owner != owner || sacrifice.IsCharm) break;
                    int burst = state.CurrentLife(sacrifice);
                    DestroyUnit(state, sacrifice, events);
                    foreach (var enemy in state.Units.Where(u => u.Owner != owner && u.X == targetX).ToList())
                    {
                        if (state.Units.Contains(enemy))
                            DealAbilityDamage(state, enemy, burst, events);
                    }
                    break;
                }
                case EffectAction.DiscardRandom:
                    DiscardRandom(state, owner, effect.Amount, events);
                    break;
                case EffectAction.EnemyDiscardRandom:
                    DiscardRandom(state, 1 - owner, effect.Amount, events);
                    break;
                case EffectAction.GainPlayerLife:
                {
                    state.Players[owner].Life += effect.Amount;
                    events.Add(new GameEvent { Type = GameEventType.PlayerHealed, Player = owner, Amount = effect.Amount });
                    break;
                }
                case EffectAction.CallUnitNearby:
                {
                    if (source == null || string.IsNullOrEmpty(effect.CalledCardId)) break;
                    var nearby = new List<(int x, int y)>();
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                            if ((dx != 0 || dy != 0) && GameState.InBounds(source.X + dx, source.Y + dy) &&
                                GameState.SideOfRow(source.Y + dy) == owner && state.GetUnitAt(source.X + dx, source.Y + dy) == null)
                                nearby.Add((source.X + dx, source.Y + dy));
                    if (nearby.Count > 0)
                    {
                        var (nx, ny) = nearby[state.NextRandom(nearby.Count)];
                        CallUnit(state, owner, effect.CalledCardId, nx, ny, events);
                    }
                    break;
                }
                case EffectAction.CallUnitAtTarget:
                {
                    if (string.IsNullOrEmpty(effect.CalledCardId)) break;
                    if (!GameState.InBounds(targetX, targetY) || GameState.SideOfRow(targetY) != owner) break;
                    if (state.GetUnitAt(targetX, targetY) != null) break;
                    CallUnit(state, owner, effect.CalledCardId, targetX, targetY, events);
                    break;
                }
                case EffectAction.GainRandomKeyword:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                    {
                        if (unit.IsCharm) continue;
                        GrantRandomKeyword(state, unit, events);
                    }
                    break;
                }
                case EffectAction.MoveRandomAdjacent:
                {
                    if (source == null || !state.Units.Contains(source)) break;
                    var moves = new List<(int dx, int dy)>();
                    foreach (var (dx, dy) in new[] { (0, 1), (0, -1), (-1, 0), (1, 0) })
                    {
                        int nx = source.X + dx, ny = source.Y + dy;
                        if (GameState.InBounds(nx, ny) && GameState.SideOfRow(ny) == source.Owner && state.GetUnitAt(nx, ny) == null)
                            moves.Add((dx, dy));
                    }
                    if (moves.Count > 0)
                    {
                        var (mx, my) = moves[state.NextRandom(moves.Count)];
                        TryMoveUnit(state, source, mx, my, events);
                    }
                    break;
                }
                case EffectAction.ConsumeAdjacent:
                {
                    if (source == null || !state.Units.Contains(source)) break;
                    var adjacent = state.UnitsOf(owner)
                        .Where(u => u != source && !u.IsCharm && Math.Abs(u.X - source.X) + Math.Abs(u.Y - source.Y) == 1)
                        .OrderBy(u => u.Id).ToList();
                    if (adjacent.Count == 0) break;

                    var meals = effect.Amount == 1
                        ? new List<UnitState> { adjacent[state.NextRandom(adjacent.Count)] }
                        : adjacent;
                    foreach (var meal in meals)
                    {
                        if (!state.Units.Contains(meal) || !state.Units.Contains(source)) break;
                        DestroyUnit(state, meal, events);
                        if (!state.Units.Contains(source)) break;
                        source.BonusPower += 1;
                        source.BonusLife += 1;
                        source.Flux = false;
                        events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = source.Id, CardId = source.CardId, Amount = 2 });
                        if (effect.Amount == 0) GrantRandomKeyword(state, source, events); //Amalgam
                    }
                    break;
                }
                case EffectAction.LaneProjectiles:
                {
                    int enemy = 1 - owner;
                    int forward = GameState.ForwardDir(owner);
                    for (int lane = 0; lane < GameConfig.Lanes; lane++)
                    {
                        for (int y = GameState.FrontlineRow(enemy); GameState.InBounds(lane, y) && GameState.SideOfRow(y) == enemy; y += forward)
                        {
                            var victim = state.GetUnitAt(lane, y);
                            if (victim == null) continue;
                            DealAbilityDamage(state, victim, effect.Amount, events);
                            if (effect.SpaceEffect != SpaceEffectType.None)
                            {
                                state.SpaceEffects[lane, y] = effect.SpaceEffect;
                                events.Add(new GameEvent { Type = GameEventType.SpaceEffectApplied, X = lane, Y = y, SpaceEffect = effect.SpaceEffect });
                            }
                            break;
                        }
                    }
                    break;
                }
                case EffectAction.SwapFloodDesert:
                {
                    for (int x = 0; x < GameConfig.Lanes; x++)
                    {
                        for (int y = 0; y < GameConfig.Rows; y++)
                        {
                            if (state.SpaceEffects[x, y] == SpaceEffectType.Flooded) state.SpaceEffects[x, y] = SpaceEffectType.Desert;
                            else if (state.SpaceEffects[x, y] == SpaceEffectType.Desert) state.SpaceEffects[x, y] = SpaceEffectType.Flooded;
                            else continue;
                            events.Add(new GameEvent { Type = GameEventType.SpaceEffectApplied, X = x, Y = y, SpaceEffect = state.SpaceEffects[x, y] });
                        }
                    }
                    break;
                }
                case EffectAction.ScorchSpace:
                {
                    bool hadEffect = state.SpaceEffects[targetX, targetY] != SpaceEffectType.None;
                    var occupant = state.GetUnitAt(targetX, targetY);
                    if (occupant != null) DealAbilityDamage(state, occupant, 2, events);
                    if (hadEffect)
                    {
                        state.SpaceEffects[targetX, targetY] = SpaceEffectType.Scorched;
                        events.Add(new GameEvent { Type = GameEventType.SpaceEffectApplied, X = targetX, Y = targetY, SpaceEffect = SpaceEffectType.Scorched });
                    }
                    break;
                }
                case EffectAction.DamageBreaker:
                {
                    var breaker = state.GetUnit(state.LastCombatAttackerId);
                    if (breaker != null) DamageUnit(state, breaker, effect.Amount, events);
                    break;
                }
                case EffectAction.HealFull:
                {
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                    {
                        if (unit.Damage > 0 && state.SpaceEffects[unit.X, unit.Y] != SpaceEffectType.Scorched)
                        {
                            events.Add(new GameEvent { Type = GameEventType.UnitHealed, UnitId = unit.Id, CardId = unit.CardId, Amount = unit.Damage });
                            unit.Damage = 0;
                        }
                    }
                    break;
                }
                case EffectAction.AdvanceBehindTarget:
                {
                    int back = -GameState.ForwardDir(owner);
                    var movers = state.UnitsOf(owner)
                        .Where(u => !u.IsCharm && u.X == targetX && (u.Y - targetY) * back > 0)
                        .OrderBy(u => Math.Abs(u.Y - targetY)).ToList();
                    foreach (var mover in movers)
                        if (state.Units.Contains(mover)) TryMoveUnit(state, mover, 0, GameState.ForwardDir(owner), events);
                    break;
                }
                case EffectAction.PercussiveMend:
                {
                    var victim = state.GetUnitAt(targetX, targetY);
                    if (victim == null) break;
                    DealAbilityDamage(state, victim, 2, events);
                    if (state.Units.Contains(victim) && victim.IsCharm && victim.Damage > 0)
                    {
                        events.Add(new GameEvent { Type = GameEventType.UnitHealed, UnitId = victim.Id, CardId = victim.CardId, Amount = victim.Damage });
                        victim.Damage = 0;
                    }
                    break;
                }
                case EffectAction.CopyStruckCharm:
                {
                    var struck = state.GetUnitAt(targetX, targetY);
                    if (struck != null && struck.IsCharm && struck.Owner != owner)
                    {
                        state.Players[owner].Hand.Add(struck.CardId);
                        events.Add(new GameEvent { Type = GameEventType.CardDrawn, Player = owner, CardId = struck.CardId });
                    }
                    break;
                }
                case EffectAction.ReturnToHand:
                {
                    var bounced = state.GetUnitAt(targetX, targetY);
                    if (bounced == null || bounced.Definition.IsSpirit) break;
                    state.Units.Remove(bounced);
                    events.Add(new GameEvent { Type = GameEventType.UnitDestroyed, UnitId = bounced.Id, CardId = bounced.CardId, X = targetX, Y = targetY });
                    state.Players[bounced.Owner].Hand.Add(bounced.CardId);
                    events.Add(new GameEvent { Type = GameEventType.CardDrawn, Player = bounced.Owner, CardId = bounced.CardId });
                    break;
                }
                case EffectAction.ShatterCharm:
                {
                    var charm = state.GetUnitAt(targetX, targetY);
                    if (charm == null || !charm.IsCharm || charm.Owner != owner) break;
                    int burst = state.CurrentLife(charm);
                    DestroyUnit(state, charm, events);
                    foreach (var victim in state.Units.Where(u => u.X == targetX).ToList())
                        if (state.Units.Contains(victim)) DealAbilityDamage(state, victim, burst, events);
                    break;
                }
                case EffectAction.AutoAttune:
                {
                    var hand = state.Players[owner].Hand;
                    if (hand.Count == 0) break;
                    int pick = state.NextRandom(hand.Count);
                    string burned = hand[pick];
                    hand.RemoveAt(pick);
                    state.Players[owner].MaxEnergy += 1;
                    state.Players[owner].Energy += 1;
                    state.Players[owner].Affinity[CardCatalogV1.Get(burned).Archetype] += 1;
                    events.Add(new GameEvent { Type = GameEventType.CardReplaced, Player = owner, CardId = burned });
                    break;
                }
                case EffectAction.LashOut:
                {
                    var martyr = state.GetUnitAt(targetX, targetY);
                    if (martyr == null || martyr.Owner != owner || martyr.IsCharm) break;
                    int lash = state.CurrentLife(martyr);
                    int dir = GameState.ForwardDir(owner);
                    for (int y = targetY + dir; GameState.InBounds(targetX, y); y += dir)
                    {
                        var hit = state.GetUnitAt(targetX, y);
                        if (hit != null && state.Units.Contains(hit)) DealAbilityDamage(state, hit, lash, events);
                    }
                    if (state.Units.Contains(martyr)) DestroyUnit(state, martyr, events);
                    break;
                }
                case EffectAction.SacrificeForEnergy:
                {
                    var offering = state.GetUnitAt(targetX, targetY);
                    if (offering == null || offering.Owner != owner || offering.IsCharm || offering.Definition.IsSpirit) break;
                    int gained = offering.Definition.Cost;
                    DestroyUnit(state, offering, events);
                    state.Players[owner].Energy += gained;
                    events.Add(new GameEvent { Type = GameEventType.EnergyChanged, Player = owner, Amount = state.Players[owner].Energy });
                    ResolveEffect(state, new EffectDef { Trigger = effect.Trigger, Action = EffectAction.AddRandomSpirit }, source, owner, targetX, targetY, events);
                    break;
                }
                case EffectAction.RebirthTarget:
                {
                    var reborn = state.GetUnitAt(targetX, targetY);
                    if (reborn == null || reborn.Owner != owner || reborn.IsCharm) break;
                    string rebornId = reborn.CardId;
                    DestroyUnit(state, reborn, events);
                    if (state.GetUnitAt(targetX, targetY) != null) break; //death triggers filled the space
                    CallUnit(state, owner, rebornId, targetX, targetY, events);
                    var fresh = state.GetUnitAt(targetX, targetY);
                    if (fresh != null)
                    {
                        fresh.BonusPower += 1;
                        if (fresh.GrantedEffects == null) fresh.GrantedEffects = new List<EffectDef>();
                        fresh.GrantedEffects.Add(new EffectDef { Trigger = Trigger.Static, Action = EffectAction.StaticRegen, Scope = TargetScope.Self, Amount = 1 });
                        events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = fresh.Id, CardId = fresh.CardId, Amount = 1 });
                    }
                    break;
                }
                case EffectAction.ReckoningColumn:
                {
                    int forward = GameState.ForwardDir(owner);
                    foreach (var martyr in state.UnitsOf(owner).Where(u => !u.IsCharm && u.X == targetX).ToList())
                    {
                        if (!state.Units.Contains(martyr)) continue;
                        var inFront = state.GetUnitAt(martyr.X, martyr.Y + forward);
                        int blast = state.CurrentLife(martyr);
                        if (inFront != null) DealAbilityDamage(state, inFront, blast, events);
                        if (state.Units.Contains(martyr)) DestroyUnit(state, martyr, events);
                    }
                    break;
                }
                case EffectAction.ReinventSpirit:
                {
                    var vessel = state.GetUnitAt(targetX, targetY);
                    if (vessel == null || vessel.Owner != owner || vessel.BoundSpiritCardId == null) break;
                    BreakBond(state, vessel, events);
                    var playerState = state.Players[owner];
                    if (playerState.Deck.Count == 0) break;
                    string drawn = playerState.Deck[0];
                    playerState.Deck.RemoveAt(0);
                    if (CardCatalogV1.Get(drawn).IsSpirit && state.Units.Contains(vessel))
                    {
                        vessel.BoundSpiritCardId = drawn;
                        vessel.SpiritDamage = 0;
                        events.Add(new GameEvent { Type = GameEventType.UnitBonded, UnitId = vessel.Id, CardId = drawn, X = vessel.X, Y = vessel.Y });
                    }
                    else
                    {
                        playerState.Hand.Add(drawn);
                        events.Add(new GameEvent { Type = GameEventType.CardDrawn, Player = owner, CardId = drawn });
                    }
                    break;
                }
                case EffectAction.DisperseTarget:
                {
                    var dispersed = state.GetUnitAt(targetX, targetY);
                    if (dispersed == null || dispersed.IsCharm) break;
                    int points = state.EffectivePower(dispersed) + state.CurrentLife(dispersed);
                    int heirOwner = dispersed.Owner;
                    DestroyUnit(state, dispersed, events);
                    var heirs = state.UnitsOf(heirOwner).Where(u => !u.IsCharm &&
                        Math.Abs(u.X - targetX) <= 1 && Math.Abs(u.Y - targetY) <= 1).ToList();
                    if (heirs.Count == 0) break;
                    for (int n = 0; n < points; n++)
                    {
                        var heir = heirs[state.NextRandom(heirs.Count)];
                        if (state.NextRandom(2) == 0) heir.BonusPower += 1; else heir.BonusLife += 1;
                    }
                    foreach (var heir in heirs)
                        events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = heir.Id, CardId = heir.CardId });
                    break;
                }
                case EffectAction.FloodBarrage:
                {
                    int shots = state.CountSpaces(SpaceEffectType.Flooded);
                    for (int n = 0; n < shots; n++)
                    {
                        var enemies = state.Units.Where(u => u.Owner != owner).ToList();
                        if (enemies.Count == 0) break;
                        var hit = enemies[state.NextRandom(enemies.Count)];
                        DealAbilityDamage(state, hit, 1, events);
                    }
                    break;
                }
                case EffectAction.DiscountNextAbility:
                    state.Players[owner].NextAbilityDiscount += effect.Amount;
                    break;
                case EffectAction.StaticCallDiscountBehind:
                    break; //consumed statically by EffectiveCost/CallDiscountAt
                case EffectAction.GeoAbsorb:
                {
                    if (source == null) break;
                    int absorbed = 0;
                    for (int x = 0; x < GameConfig.Lanes; x++)
                    {
                        for (int y = 0; y < GameConfig.Rows; y++)
                        {
                            if (GameState.SideOfRow(y) != owner || state.SpaceEffects[x, y] == SpaceEffectType.None) continue;
                            state.SpaceEffects[x, y] = SpaceEffectType.None;
                            events.Add(new GameEvent { Type = GameEventType.SpaceEffectApplied, X = x, Y = y, SpaceEffect = SpaceEffectType.None });
                            absorbed++;
                        }
                    }
                    if (absorbed > 0)
                    {
                        source.BonusPower += absorbed;
                        source.BonusLife += absorbed;
                        events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = source.Id, CardId = source.CardId, Amount = absorbed * 2 });
                    }
                    break;
                }
                case EffectAction.BlessNextCall:
                {
                    var blessing = state.Players[owner];
                    blessing.NextCallDiscount += effect.Amount;
                    blessing.NextCallPower += effect.Power;
                    blessing.NextCallLife += effect.Life;
                    if (effect.Granted != null) blessing.NextCallGranted = effect.Granted;
                    break;
                }
                case EffectAction.ScheduleDoom:
                    state.Pending.Add(new PendingAction { Player = owner, TurnsLeft = 1, X = targetX, Y = targetY });
                    break;
                case EffectAction.ScheduleRebind:
                {
                    if (source == null || string.IsNullOrEmpty(state.LastBrokenSpiritId)) break;
                    state.Pending.Add(new PendingAction { Player = owner, TurnsLeft = 1, UnitId = source.Id, CardId = state.LastBrokenSpiritId, IsRebind = true });
                    break;
                }
                case EffectAction.GiveYourAll:
                {
                    foreach (var soldier in state.UnitsOf(owner).Where(u => !u.IsCharm && u.Y == GameState.FrontlineRow(owner)).ToList())
                    {
                        soldier.TempPower += 2;
                        soldier.TempOverpower = true;
                        soldier.TempDoomed = true;
                        events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = soldier.Id, CardId = soldier.CardId, Amount = 2 });
                    }
                    break;
                }
                case EffectAction.TempPushLane:
                {
                    foreach (var pusher in state.UnitsOf(owner).Where(u => !u.IsCharm && u.X == targetX))
                        pusher.TempPushOnAttack = true;
                    break;
                }
                case EffectAction.AttackWithMoved:
                {
                    foreach (var mover in state.UnitsOf(owner).Where(u => u.MovedThisTurn && !u.IsCharm).ToList())
                    {
                        if (!state.Units.Contains(mover)) continue;
                        mover.MovedThisTurn = false;
                        TryUnitAttack(state, mover, events);
                        mover.MovedThisTurn = true;
                    }
                    break;
                }
                case EffectAction.FireSale:
                {
                    foreach (var charm in state.UnitsOf(owner).Where(u => u.IsCharm).ToList())
                    {
                        state.Units.Remove(charm); //banished, not destroyed: no triggers
                        events.Add(new GameEvent { Type = GameEventType.UnitDestroyed, UnitId = charm.Id, CardId = charm.CardId, X = charm.X, Y = charm.Y });
                        state.Players[owner].Hand.Add("Valuable Coin");
                        events.Add(new GameEvent { Type = GameEventType.CardDrawn, Player = owner, CardId = "Valuable Coin" });
                    }
                    break;
                }
                case EffectAction.AddRandomCheapCharm:
                {
                    var charms = CardCatalogV1.Cards.Values.Where(c => c.Type == CardType.Charm && c.Cost <= effect.Amount).OrderBy(c => c.Id).ToList();
                    if (charms.Count == 0) break;
                    string pick = charms[state.NextRandom(charms.Count)].Id;
                    state.Players[owner].Hand.Add(pick);
                    events.Add(new GameEvent { Type = GameEventType.CardDrawn, Player = owner, CardId = pick });
                    break;
                }
                case EffectAction.ReclaimSpirit:
                {
                    if (string.IsNullOrEmpty(state.LastBrokenSpiritId)) break;
                    state.Players[owner].Hand.Add(state.LastBrokenSpiritId);
                    events.Add(new GameEvent { Type = GameEventType.CardDrawn, Player = owner, CardId = state.LastBrokenSpiritId });
                    break;
                }
                case EffectAction.GrantAbility:
                {
                    if (effect.Granted == null) break;
                    foreach (var unit in GatherUnits(state, effect.Scope, source, owner, targetX, targetY))
                    {
                        if (unit.IsCharm) continue;
                        if (unit.GrantedEffects == null) unit.GrantedEffects = new List<EffectDef>();
                        unit.GrantedEffects.Add(effect.Granted);
                        events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = unit.Id, CardId = unit.CardId });
                    }
                    break;
                }
            }
        }

        /// <summary>Small random buff for Ocean mutations and Flooded calls.</summary>
        private static void GrantRandomKeyword(GameState state, UnitState unit, List<GameEvent> events)
        {
            switch (state.NextRandom(5))
            {
                case 0: unit.BonusArmor += 1; break;
                case 1: unit.BonusPierce += 1; break;
                case 2: unit.BonusParry += 1; break;
                case 3: unit.BonusPower += 1; break;
                default: unit.BonusLife += 1; break;
            }
            events.Add(new GameEvent { Type = GameEventType.UnitStatsChanged, UnitId = unit.Id, CardId = unit.CardId, Amount = 1 });
        }

        /// <summary>
        /// Ability (non-attack) damage: ignores Armor; reduced by the victim's
        /// Resist; +2 if the victim stands on a Primed space, consuming it.
        /// </summary>
        private static bool reflecting; //Dark Mirror re-entrancy guard

        private static void DealAbilityDamage(GameState state, UnitState unit, int amount, List<GameEvent> events)
        {
            //Dark Mirror: ability damage aimed here bounces to the mirrored space
            if (unit.Definition.Reflects && !reflecting)
            {
                var mirrored = state.GetUnitAt(unit.X, GameConfig.Rows - 1 - unit.Y);
                if (mirrored != null)
                {
                    reflecting = true;
                    DealAbilityDamage(state, mirrored, amount, events);
                    reflecting = false;
                }
                return;
            }

            if (state.SpaceEffects[unit.X, unit.Y] == SpaceEffectType.Primed)
            {
                amount += 2;
                state.SpaceEffects[unit.X, unit.Y] = SpaceEffectType.None;
                events.Add(new GameEvent { Type = GameEventType.SpaceEffectApplied, X = unit.X, Y = unit.Y, SpaceEffect = SpaceEffectType.None });
            }

            amount += state.EffectiveAmplify(unit);
            amount -= state.EffectiveResist(unit);
            DamageUnit(state, unit, amount, events);
        }

        private static List<UnitState> GatherUnits(GameState state, TargetScope scope, UnitState source, int owner, int targetX, int targetY)
        {
            switch (scope)
            {
                case TargetScope.Self:
                    return source != null && state.Units.Contains(source) ? new List<UnitState> { source } : new List<UnitState>();
                case TargetScope.TargetUnit:
                {
                    var unit = state.GetUnitAt(targetX, targetY);
                    return unit != null ? new List<UnitState> { unit } : new List<UnitState>();
                }
                case TargetScope.TargetRow:
                    return state.Units.Where(u => u.Y == targetY).ToList();
                case TargetScope.TargetLane:
                    return state.Units.Where(u => u.X == targetX).ToList();
                case TargetScope.SourceRow:
                    return source != null ? state.Units.Where(u => u.Y == source.Y).ToList() : new List<UnitState>();
                case TargetScope.AllFriendlyUnits:
                    return state.UnitsOf(owner).Where(u => !u.IsCharm).ToList();
                case TargetScope.UnblockedFriendlyUnits:
                    return state.UnitsOf(owner).Where(u => !u.IsCharm && state.LaneUnblockedFor(owner, u.X)).ToList();
                case TargetScope.NearestEnemyInLane:
                {
                    if (source == null) return new List<UnitState>();
                    int enemy = 1 - owner;
                    int forward = GameState.ForwardDir(owner);
                    for (int y = GameState.FrontlineRow(enemy); GameState.InBounds(source.X, y) && GameState.SideOfRow(y) == enemy; y += forward)
                    {
                        var unit = state.GetUnitAt(source.X, y);
                        if (unit != null) return new List<UnitState> { unit };
                    }
                    return new List<UnitState>();
                }
                case TargetScope.EnemiesInLane:
                    return source == null ? new List<UnitState>() :
                        state.Units.Where(u => u.Owner != owner && u.X == source.X).ToList();
                case TargetScope.AllEnemyUnits:
                    return state.Units.Where(u => u.Owner != owner).ToList();
                case TargetScope.InFront:
                {
                    if (source == null) return new List<UnitState>();
                    var inFront = state.GetUnitAt(source.X, source.Y + GameState.ForwardDir(owner));
                    return inFront != null ? new List<UnitState> { inFront } : new List<UnitState>();
                }
                case TargetScope.Nearby:
                    return source == null ? new List<UnitState>() :
                        state.UnitsOf(owner).Where(u => u != source &&
                            Math.Abs(u.X - source.X) <= 1 && Math.Abs(u.Y - source.Y) <= 1).ToList();
                case TargetScope.FriendlyUnitsWithAdjacentAlly:
                    return state.UnitsOf(owner).Where(u => !u.IsCharm &&
                        state.UnitsOf(owner).Any(o => o != u && Math.Abs(o.X - u.X) + Math.Abs(o.Y - u.Y) == 1)).ToList();
                default:
                    return new List<UnitState>();
            }
        }

        private static List<(int x, int y)> GatherSpaces(GameState state, TargetScope scope, UnitState source, int targetX, int targetY)
        {
            var spaces = new List<(int, int)>();
            switch (scope)
            {
                case TargetScope.TargetSpace:
                    spaces.Add((targetX, targetY));
                    break;
                case TargetScope.Self:
                    if (source != null) spaces.Add((source.X, source.Y));
                    break;
                case TargetScope.SourceRow:
                {
                    if (source == null) break;
                    for (int x = 0; x < GameConfig.Lanes; x++) spaces.Add((x, source.Y));
                    break;
                }
                case TargetScope.TargetRow:
                {
                    for (int x = 0; x < GameConfig.Lanes; x++) spaces.Add((x, targetY));
                    break;
                }
                case TargetScope.InFront:
                {
                    if (source == null) break;
                    int fy = source.Y + GameState.ForwardDir(source.Owner);
                    if (GameState.InBounds(source.X, fy)) spaces.Add((source.X, fy));
                    break;
                }
                case TargetScope.AdjacentToTarget:
                {
                    foreach (var (dx, dy) in new[] { (0, 1), (0, -1), (-1, 0), (1, 0) })
                        if (GameState.InBounds(targetX + dx, targetY + dy)) spaces.Add((targetX + dx, targetY + dy));
                    break;
                }
                case TargetScope.TargetLane:
                {
                    for (int y = 0; y < GameConfig.Rows; y++) spaces.Add((targetX, y));
                    break;
                }
                case TargetScope.RandomNearbySpace:
                {
                    if (source == null) break;
                    var candidates = new List<(int, int)>();
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                            if ((dx != 0 || dy != 0) && GameState.InBounds(source.X + dx, source.Y + dy))
                                candidates.Add((source.X + dx, source.Y + dy));
                    if (candidates.Count > 0) spaces.Add(candidates[state.NextRandom(candidates.Count)]);
                    break;
                }
            }
            return spaces;
        }

        private static int CountSpaces(GameState state, SpaceEffectType effect)
        {
            int count = 0;
            for (int x = 0; x < GameConfig.Lanes; x++)
                for (int y = 0; y < GameConfig.Rows; y++)
                    if (state.SpaceEffects[x, y] == effect) count++;
            return count;
        }

        //---- Movement ----

        /// <summary>The friendly Equip charm occupying (x,y), if any - a space a unit of that owner may enter.</summary>
        private static UnitState FriendlyEquipAt(GameState state, int owner, int x, int y, bool forCharm = false)
        {
            var occupant = state.GetUnitAt(x, y);
            if (occupant == null || occupant.Owner != owner || !occupant.Definition.IsEquip) return null;
            if (forCharm && !occupant.Definition.EquipsCharms) return null;
            return occupant;
        }

        /// <summary>Voluntary or automatic move; fails silently if blocked.</summary>
        private static bool TryMoveUnit(GameState state, UnitState unit, int dx, int dy, List<GameEvent> events)
        {
            int destX = unit.X + dx, destY = unit.Y + dy;
            if (!GameState.InBounds(destX, destY)) return false;
            if (GameState.SideOfRow(destY) != unit.Owner) return false;
            var occupant = state.GetUnitAt(destX, destY);
            if (occupant != null && FriendlyEquipAt(state, unit.Owner, destX, destY) == null) return false;

            MoveUnitTo(state, unit, destX, destY, events);
            return true;
        }

        private static void MoveUnitTo(GameState state, UnitState unit, int destX, int destY, List<GameEvent> events)
        {
            //Entering a friendly Equip charm's space consumes it and bestows its effects
            var equip = !unit.IsCharm ? FriendlyEquipAt(state, unit.Owner, destX, destY) : null;
            if (equip != null)
            {
                state.Units.Remove(equip);
                events.Add(new GameEvent { Type = GameEventType.EquipAttached, UnitId = unit.Id, CardId = equip.CardId, X = destX, Y = destY });
            }

            int fromX = unit.X, fromY = unit.Y;
            bool leftBramble = state.SpaceEffects[fromX, fromY] == SpaceEffectType.Brambled;

            unit.X = destX;
            unit.Y = destY;
            events.Add(new GameEvent { Type = GameEventType.UnitMoved, UnitId = unit.Id, CardId = unit.CardId, X = fromX, Y = fromY, ToX = destX, ToY = destY });

            int forward = GameState.ForwardDir(unit.Owner);
            bool advanced = (destY - fromY) * forward > 0;
            bool retreated = (destY - fromY) * forward < 0;

            //Brambled: units take damage entering or leaving (doubled under a Sunlamp)
            int brambleDamage = state.GardenEffectsBoosted ? 2 : 1;
            bool enteredBramble = state.SpaceEffects[destX, destY] == SpaceEffectType.Brambled;
            if (leftBramble) DamageUnit(state, unit, brambleDamage, events);
            if (enteredBramble && state.Units.Contains(unit)) DamageUnit(state, unit, brambleDamage, events);

            if (!state.Units.Contains(unit)) return;

            if (equip != null)
            {
                foreach (var effect in equip.Definition.Effects.Where(e => e.Trigger == Trigger.OnEquip))
                    ResolveEffect(state, effect, unit, unit.Owner, unit.X, unit.Y, events);
                foreach (var effect in unit.AllEffects.Where(e => e.Trigger == Trigger.OnEquipped).ToList())
                {
                    if (!state.Units.Contains(unit)) break;
                    ResolveEffect(state, effect, unit, unit.Owner, unit.X, unit.Y, events);
                }
            }

            if (!state.Units.Contains(unit)) return;

            foreach (var effect in unit.AllEffects)
            {
                if ((advanced && effect.Trigger == Trigger.OnAdvance) ||
                    (retreated && effect.Trigger == Trigger.OnRetreat))
                {
                    ResolveEffect(state, effect, unit, unit.Owner, unit.X, unit.Y, events);
                }
            }
        }

        /// <summary>Forced movement away from the pusher; collisions deal 1 damage to both.</summary>
        private static void PushUnit(GameState state, UnitState unit, int pushDir, List<GameEvent> events)
        {
            if (unit.IsHeavy) return; //Heavy: cannot be moved by Push or Pull

            int destY = unit.Y + pushDir;

            //Pushed off the back of the field: no move
            if (!GameState.InBounds(unit.X, destY) || GameState.SideOfRow(destY) != unit.Owner) return;

            var occupant = state.GetUnitAt(unit.X, destY);
            if (occupant != null && FriendlyEquipAt(state, unit.Owner, unit.X, destY) == null)
            {
                DamageUnit(state, unit, 1, events);
                if (state.Units.Contains(occupant)) DamageUnit(state, occupant, 1, events);
                return;
            }

            MoveUnitTo(state, unit, unit.X, destY, events);
        }

        //---- Damage, healing, death ----

        private static void DamageUnit(GameState state, UnitState unit, int amount, List<GameEvent> events)
        {
            if (amount <= 0 || !state.Units.Contains(unit)) return;

            //Spirit Bind: the bound spirit takes all damage in the host's place.
            //When it breaks, its OnBondBreak effects fire with the HOST as source;
            //excess damage is lost.
            if (unit.BoundSpiritCardId != null)
            {
                var spirit = unit.BoundSpirit;
                unit.SpiritDamage += amount;
                events.Add(new GameEvent { Type = GameEventType.UnitDamaged, UnitId = unit.Id, CardId = unit.BoundSpiritCardId, Amount = amount });

                if (unit.SpiritDamage >= spirit.Life)
                    BreakBond(state, unit, events);
                return;
            }

            //Evade: prevent any damage instance, consuming one charge
            if (unit.TempEvade - unit.EvadeUsedThisTurn > 0)
            {
                unit.EvadeUsedThisTurn++;
                events.Add(new GameEvent { Type = GameEventType.DamageEvaded, UnitId = unit.Id, CardId = unit.CardId });
                return;
            }

            unit.Damage += amount;
            events.Add(new GameEvent { Type = GameEventType.UnitDamaged, UnitId = unit.Id, CardId = unit.CardId, Amount = amount });

            if (unit.Asleep)
            {
                unit.Asleep = false;
                events.Add(new GameEvent { Type = GameEventType.UnitWoke, UnitId = unit.Id, CardId = unit.CardId });
            }

            if (state.CurrentLife(unit) <= 0)
            {
                DestroyUnit(state, unit, events);
                return;
            }

            //Rugged: damaged survivors are knocked toward their own backline.
            //The guard stops collision damage from re-triggering the knockback.
            if (state.SpaceEffects[unit.X, unit.Y] == SpaceEffectType.Rugged && !unit.IsCharm && !resolvingRuggedPush)
            {
                resolvingRuggedPush = true;
                PushUnit(state, unit, -GameState.ForwardDir(unit.Owner), events);
                resolvingRuggedPush = false;
            }

            //OnDamaged (Guilt-Wracked Soul, Fire-spitter, Floppy Fish): survived the hit
            foreach (var effect in unit.AllEffects.Where(e => e.Trigger == Trigger.OnDamaged).ToList())
            {
                if (!state.Units.Contains(unit)) break;
                ResolveEffect(state, effect, unit, unit.Owner, unit.X, unit.Y, events);
            }
        }

        /// <summary>Break a unit's spirit bond, firing the spirit's break effects and the owner's bond-break watchers.</summary>
        private static void BreakBond(GameState state, UnitState unit, List<GameEvent> events)
        {
            if (unit.BoundSpiritCardId == null) return;
            var spirit = unit.BoundSpirit;
            string spiritId = unit.BoundSpiritCardId;
            unit.BoundSpiritCardId = null;
            unit.SpiritDamage = 0;
            state.LastBrokenSpiritId = spiritId;
            events.Add(new GameEvent { Type = GameEventType.BondBroken, UnitId = unit.Id, CardId = spiritId, X = unit.X, Y = unit.Y });

            foreach (var effect in spirit.Effects.Where(e => e.Trigger == Trigger.OnBondBreak))
            {
                if (!state.Units.Contains(unit)) break;
                ResolveEffect(state, effect, unit, unit.Owner, unit.X, unit.Y, events);
            }

            //Host's own-break effects (Spirit Caller schedules a rebind)
            if (state.Units.Contains(unit))
            {
                foreach (var effect in unit.AllEffects.Where(e => e.Trigger == Trigger.OnOwnBondBreak).ToList())
                    ResolveEffect(state, effect, unit, unit.Owner, unit.X, unit.Y, events);
            }

            //Owner's bond-break watchers (Soulcatcher)
            foreach (var watcher in state.UnitsOf(unit.Owner).ToList())
            {
                if (!state.Units.Contains(watcher)) continue;
                foreach (var effect in watcher.AllEffects.Where(e => e.Trigger == Trigger.OnOwnerBondBreak))
                    ResolveEffect(state, effect, watcher, watcher.Owner, watcher.X, watcher.Y, events);
            }
        }

        private static void HealUnit(GameState state, UnitState unit, int amount, List<GameEvent> events)
        {
            //Scorched ground: no healing (provisional Heart ruling)
            if (state.SpaceEffects[unit.X, unit.Y] == SpaceEffectType.Scorched) return;

            int healed = Math.Min(amount, unit.Damage);
            if (healed <= 0) return;

            unit.Damage -= healed;
            events.Add(new GameEvent { Type = GameEventType.UnitHealed, UnitId = unit.Id, CardId = unit.CardId, Amount = healed });
        }

        private static void DestroyUnit(GameState state, UnitState unit, List<GameEvent> events)
        {
            state.Units.Remove(unit);
            events.Add(new GameEvent { Type = GameEventType.UnitDestroyed, UnitId = unit.Id, CardId = unit.CardId, X = unit.X, Y = unit.Y });

            foreach (var effect in unit.AllEffects.Where(e => e.Trigger == Trigger.OnDestroy))
                ResolveEffect(state, effect, unit, unit.Owner, unit.X, unit.Y, events);

            //Death watchers (Mourner's Altar, Font of Sorrows, Dinner Bell)
            foreach (var watcher in state.UnitsOf(unit.Owner).ToList())
            {
                if (!state.Units.Contains(watcher) || watcher == unit) continue;
                bool nearby = Math.Abs(watcher.X - unit.X) <= 1 && Math.Abs(watcher.Y - unit.Y) <= 1;
                foreach (var effect in watcher.AllEffects)
                {
                    if (effect.Trigger == Trigger.OnFriendlyDestroyed ||
                        (effect.Trigger == Trigger.OnFriendlyDestroyedNearby && nearby))
                        ResolveEffect(state, effect, watcher, watcher.Owner, unit.X, unit.Y, events);
                }
            }
        }

        private static void DamagePlayer(GameState state, int player, int amount, List<GameEvent> events)
        {
            if (amount <= 0) return;

            state.Players[player].Life -= amount;
            events.Add(new GameEvent { Type = GameEventType.PlayerDamaged, Player = player, Amount = amount });

            //Player-damage watchers (Flagellant's Charm)
            foreach (var watcher in state.UnitsOf(player).ToList())
            {
                if (!state.Units.Contains(watcher)) continue;
                foreach (var effect in watcher.AllEffects.Where(e => e.Trigger == Trigger.OnOwnerPlayerDamaged))
                    ResolveEffect(state, effect, watcher, player, watcher.X, watcher.Y, events);
            }

            if (state.Players[player].Life <= 0 && !state.IsOver)
            {
                state.Winner = 1 - player;
                events.Add(new GameEvent { Type = GameEventType.GameEnded, Player = state.Winner });
            }
        }

        //---- Drawing ----

        private static void DrawCards(GameState state, int player, int count, List<GameEvent> events)
        {
            var playerState = state.Players[player];

            for (int n = 0; n < count; n++)
            {
                if (playerState.Deck.Count == 0)
                {
                    //Rules-v2 fatigue: each missed draw deals escalating damage
                    playerState.Fatigue++;
                    events.Add(new GameEvent { Type = GameEventType.FatigueDamage, Player = player, Amount = playerState.Fatigue });
                    DamagePlayer(state, player, playerState.Fatigue, events);
                    if (state.IsOver) return;
                    continue;
                }
                string cardId = playerState.Deck[0];
                playerState.Deck.RemoveAt(0);

                //Rules-v2: drawing with a full hand burns the card - the deck
                //still depletes, so hoarding cannot stall the fatigue clock
                if (playerState.Hand.Count >= GameConfig.MaxHandSize)
                {
                    events.Add(new GameEvent { Type = GameEventType.CardBurned, Player = player, CardId = cardId });
                    FireDiscardWatchers(state, player, events);
                    continue;
                }

                playerState.Hand.Add(cardId);
                events.Add(new GameEvent { Type = GameEventType.CardDrawn, Player = player, CardId = cardId });

                //Draw watchers (Seer's Guillotine): the drawer pays
                foreach (var watcher in state.Units.ToList())
                {
                    if (state.IsOver) return;
                    if (!state.Units.Contains(watcher)) continue;
                    foreach (var effect in watcher.AllEffects.Where(e => e.Trigger == Trigger.OnAnyDraw))
                        if (effect.Action == EffectAction.DamageDrawingPlayer)
                            DamagePlayer(state, player, effect.Amount, events);
                }
            }
        }

        /// <summary>Random discard from a player's hand, firing their discard watchers.</summary>
        private static void DiscardRandom(GameState state, int player, int count, List<GameEvent> events)
        {
            var playerState = state.Players[player];
            for (int n = 0; n < count && playerState.Hand.Count > 0; n++)
            {
                int pick = state.NextRandom(playerState.Hand.Count);
                string cardId = playerState.Hand[pick];
                playerState.Hand.RemoveAt(pick);
                events.Add(new GameEvent { Type = GameEventType.CardDiscarded, Player = player, CardId = cardId });
                FireDiscardWatchers(state, player, events);

                //Enemy-discard watchers (Amber Spyglass)
                foreach (var watcher in state.UnitsOf(1 - player).ToList())
                {
                    if (!state.Units.Contains(watcher)) continue;
                    foreach (var effect in watcher.AllEffects.Where(e => e.Trigger == Trigger.OnEnemyDiscard))
                        ResolveEffect(state, effect, watcher, watcher.Owner, watcher.X, watcher.Y, events);
                }
            }
        }

        /// <summary>OnOwnerDiscard watchers (Keeper of Debts); burns count as discards too.</summary>
        private static void FireDiscardWatchers(GameState state, int player, List<GameEvent> events)
        {
            foreach (var watcher in state.UnitsOf(player).ToList())
            {
                if (!state.Units.Contains(watcher)) continue;
                foreach (var effect in watcher.AllEffects.Where(e => e.Trigger == Trigger.OnOwnerDiscard))
                    ResolveEffect(state, effect, watcher, player, watcher.X, watcher.Y, events);
            }
        }
    }
}
