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
                case AttackCommand attack: return ExecuteAttack(state, attack);
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
            playerState.ShiftUsedThisTurn = false;

            events.Add(new GameEvent { Type = GameEventType.TurnStarted, Player = player, Amount = state.TurnNumber });
            events.Add(new GameEvent { Type = GameEventType.EnergyChanged, Player = player, Amount = playerState.Energy });

            foreach (var unit in state.UnitsOf(player))
            {
                unit.Flux = false;
                unit.AttackedThisTurn = false;
                unit.MovedThisTurn = false;
            }

            //Poison ticks at the start of the owner's turn (ignores armor and Resist)
            foreach (var unit in state.UnitsOf(player).Where(u => u.Poison > 0).ToList())
            {
                if (state.Units.Contains(unit))
                    DamageUnit(state, unit, unit.Poison, events);
            }

            //Verdant: units regen 1 life at the start of their owner's turn
            foreach (var unit in state.UnitsOf(player).ToList())
            {
                if (state.SpaceEffects[unit.X, unit.Y] == SpaceEffectType.Verdant && unit.Damage > 0)
                    HealUnit(state, unit, 1, events);
            }

            //Auto-Advance, front-most units first so columns compact forward
            var advancers = state.UnitsOf(player)
                .Where(u => u.Definition.AutoAdvance && !u.Asleep && !u.Pinned)
                .OrderBy(u => player == 0 ? -u.Y : u.Y)
                .ToList();
            foreach (var unit in advancers)
                TryMoveUnit(state, unit, 0, GameState.ForwardDir(player), events);

            DrawCards(state, player, GameConfig.CardsDrawnPerTurn, events);
        }

        private static CommandResult ExecuteEndTurn(GameState state, int player)
        {
            var result = new CommandResult { Success = true };

            //Rules-v2: every unit that can still legally attack does so
            //automatically at end of turn, front-most first. Manual attacks
            //earlier in the turn remain possible (they mark AttackedThisTurn).
            foreach (var unit in state.UnitsOf(player).OrderBy(u => player == 0 ? -u.Y : u.Y).ToList())
            {
                if (state.IsOver) break;
                if (!state.Units.Contains(unit)) continue;
                var attack = ExecuteAttack(state, new AttackCommand { Player = player, UnitId = unit.Id });
                if (attack.Success) result.Events.AddRange(attack.Events);
            }

            if (state.IsOver) return result;

            //End-of-turn triggers for the active player's units, front-most first
            foreach (var unit in state.UnitsOf(player).OrderBy(u => player == 0 ? -u.Y : u.Y).ToList())
            {
                if (!state.Units.Contains(unit)) continue;
                foreach (var effect in unit.Definition.Effects.Where(e => e.Trigger == Trigger.EndOfTurn))
                    ResolveEffect(state, effect, unit, player, unit.X, unit.Y, result.Events);
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

            if (playerState.Energy < definition.Cost)
                return CommandResult.Fail($"Not enough energy for {cardId} (need {definition.Cost}, have {playerState.Energy}).");

            if (playerState.Affinity[definition.Archetype] < definition.AffinityRequirement)
                return CommandResult.Fail($"{cardId} requires {definition.Archetype} Affinity {definition.AffinityRequirement} (have {playerState.Affinity[definition.Archetype]}).");

            string targetError = ValidatePlayTarget(state, command.Player, definition, command.TargetX, command.TargetY);
            if (targetError != null) return CommandResult.Fail(targetError);

            var result = new CommandResult { Success = true };

            playerState.Hand.RemoveAt(command.HandIndex);
            playerState.Energy -= definition.Cost;
            result.Events.Add(new GameEvent { Type = GameEventType.CardPlayed, Player = command.Player, CardId = cardId, X = command.TargetX, Y = command.TargetY });
            result.Events.Add(new GameEvent { Type = GameEventType.EnergyChanged, Player = command.Player, Amount = playerState.Energy });

            if (definition.Type == CardType.Ability)
            {
                foreach (var effect in definition.Effects.Where(e => e.Trigger == Trigger.OnPlay))
                    ResolveEffect(state, effect, null, command.Player, command.TargetX, command.TargetY, result.Events);

                //Ability-play watchers (Diligent Student, Lightning Rod, Combat Bellows)
                foreach (var watcher in state.Units.ToList())
                {
                    if (!state.Units.Contains(watcher)) continue;
                    var trigger = watcher.Owner == command.Player ? Trigger.OnOwnerAbilityPlay : Trigger.OnEnemyAbilityPlay;
                    foreach (var effect in watcher.Definition.Effects.Where(e => e.Trigger == trigger))
                        ResolveEffect(state, effect, watcher, watcher.Owner, watcher.X, watcher.Y, result.Events);
                }
            }
            else
            {
                CallUnit(state, command.Player, cardId, command.TargetX, command.TargetY, result.Events);
            }

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
                    if (state.GetUnitAt(x, y) != null) return "Target space is occupied.";
                    return null;
                case PlayTargetKind.AnySpace:
                    return GameState.InBounds(x, y) ? null : "Target space is out of bounds.";
                case PlayTargetKind.AnyUnit:
                    if (!GameState.InBounds(x, y)) return "Target space is out of bounds.";
                    return state.GetUnitAt(x, y) != null ? null : "No unit on target space.";
                default:
                    return "Unknown target kind.";
            }
        }

        private static void CallUnit(GameState state, int player, string cardId, int x, int y, List<GameEvent> events)
        {
            var definition = CardCatalogV1.Get(cardId);
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

            foreach (var effect in definition.Effects.Where(e => e.Trigger == Trigger.OnCall))
                ResolveEffect(state, effect, unit, player, x, y, events);

            //Opposing triggers (e.g. Guest Registry) fire when a Unit is summoned
            if (definition.Type == CardType.Unit)
            {
                foreach (var watcher in state.UnitsOf(1 - player).ToList())
                {
                    if (!state.Units.Contains(watcher)) continue;
                    foreach (var effect in watcher.Definition.Effects.Where(e => e.Trigger == Trigger.OnEnemyCall))
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
            if (unit.Asleep) return CommandResult.Fail("That unit is asleep.");
            if (unit.Pinned) return CommandResult.Fail("That unit is pinned.");
            if (playerState.ShiftUsedThisTurn) return CommandResult.Fail("Shift has already been used this turn.");
            if (playerState.Energy < GameConfig.ShiftEnergyCost) return CommandResult.Fail("Not enough energy to Shift.");

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
            if (occupant != null && occupant.IsCharm) return CommandResult.Fail("Cannot Shift onto a charm.");

            var result = new CommandResult { Success = true };
            playerState.Energy -= GameConfig.ShiftEnergyCost;
            playerState.ShiftUsedThisTurn = true;
            result.Events.Add(new GameEvent { Type = GameEventType.EnergyChanged, Player = command.Player, Amount = playerState.Energy });

            if (occupant != null)
            {
                //Occupied by a friendly unit: the units switch places
                MoveUnitTo(state, occupant, unit.X, unit.Y, result.Events);
                occupant.MovedThisTurn = true;
            }

            MoveUnitTo(state, unit, destX, destY, result.Events);
            unit.MovedThisTurn = true;

            return result;
        }

        //---- Attack ----

        private static CommandResult ExecuteAttack(GameState state, AttackCommand command)
        {
            var attacker = state.GetUnit(command.UnitId);

            if (attacker == null) return CommandResult.Fail("No such unit.");
            if (attacker.Owner != command.Player) return CommandResult.Fail("You don't control that unit.");
            if (attacker.IsCharm) return CommandResult.Fail("Charms cannot attack.");
            if (attacker.Asleep) return CommandResult.Fail("That unit is asleep.");
            if (attacker.Flux) return CommandResult.Fail("That unit was called this turn.");
            if (attacker.AttackedThisTurn) return CommandResult.Fail("That unit has already attacked this turn.");
            if (attacker.MovedThisTurn) return CommandResult.Fail("That unit has already moved this turn.");
            if (state.EffectivePower(attacker) <= 0) return CommandResult.Fail("Units with 0 Attack do not attack.");
            if (!attacker.Definition.Ranged && HasFriendlyUnitInFront(state, attacker))
                return CommandResult.Fail("Melee units must be in front of all other friendly units in their lane.");

            var result = new CommandResult { Success = true };
            attacker.AttackedThisTurn = true;

            int power = state.EffectivePower(attacker);
            int forward = GameState.ForwardDir(attacker.Owner);
            int enemy = 1 - attacker.Owner;

            //Scan the enemy half of the lane from their frontline backwards
            int firstTargetY = -1;
            for (int y = GameState.FrontlineRow(enemy); GameState.SideOfRow(y) == enemy && GameState.InBounds(attacker.X, y); y += forward)
            {
                if (state.GetUnitAt(attacker.X, y) != null) { firstTargetY = y; break; }
            }

            result.Events.Add(new GameEvent { Type = GameEventType.AttackResolved, Player = attacker.Owner, UnitId = attacker.Id, CardId = attacker.CardId, X = attacker.X, Y = attacker.Y });

            if (firstTargetY < 0)
            {
                //Unblocked lane: hit the opposing player directly
                DamagePlayer(state, enemy, power, result.Events);
                return result;
            }

            var target = state.GetUnitAt(attacker.X, firstTargetY);
            StrikeUnit(state, attacker, target, power, result.Events);

            //OnAttack triggers (e.g. Master Painter) aim at the struck unit's space
            if (state.Units.Contains(attacker))
            {
                foreach (var effect in attacker.Definition.Effects.Where(e => e.Trigger == Trigger.OnAttack))
                    ResolveEffect(state, effect, attacker, attacker.Owner, attacker.X, firstTargetY, result.Events);
            }

            //Pierce: the attack also travels X spaces beyond the target
            for (int n = 1; n <= attacker.Definition.Pierce + attacker.BonusPierce; n++)
            {
                int y = firstTargetY + forward * n;
                if (!GameState.InBounds(attacker.X, y)) break;
                var pierced = state.GetUnitAt(attacker.X, y);
                if (pierced != null && pierced.Owner == enemy)
                    StrikeUnit(state, attacker, pierced, power, result.Events, allowRiders: false);
            }

            return result;
        }

        private static void StrikeUnit(GameState state, UnitState attacker, UnitState target, int power, List<GameEvent> events, bool allowRiders = true)
        {
            int damage = Math.Max(0, power - state.EffectiveArmor(target));
            if (damage > 0) DamageUnit(state, target, damage, events);

            bool targetAlive = state.Units.Contains(target);

            //Push: after the attack, shove a surviving target one space away
            if (allowRiders && targetAlive && attacker.Definition.PushOnAttack)
                PushUnit(state, target, GameState.ForwardDir(attacker.Owner), events);

            //Retaliate: a surviving defender strikes back (ignores armor: effect damage)
            if (targetAlive && target.Definition.Retaliate > 0 && state.Units.Contains(attacker))
                DamageUnit(state, attacker, target.Definition.Retaliate, events);
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
                    foreach (var (x, y) in GatherSpaces(effect.Scope, source, targetX, targetY))
                    {
                        state.SpaceEffects[x, y] = effect.SpaceEffect;
                        events.Add(new GameEvent { Type = GameEventType.SpaceEffectApplied, X = x, Y = y, SpaceEffect = effect.SpaceEffect });
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
            }
        }

        /// <summary>
        /// Ability (non-attack) damage: ignores Armor; reduced by the victim's
        /// Resist; +2 if the victim stands on a Primed space, consuming it.
        /// </summary>
        private static void DealAbilityDamage(GameState state, UnitState unit, int amount, List<GameEvent> events)
        {
            if (state.SpaceEffects[unit.X, unit.Y] == SpaceEffectType.Primed)
            {
                amount += 2;
                state.SpaceEffects[unit.X, unit.Y] = SpaceEffectType.None;
                events.Add(new GameEvent { Type = GameEventType.SpaceEffectApplied, X = unit.X, Y = unit.Y, SpaceEffect = SpaceEffectType.None });
            }

            amount -= unit.Definition.Resist;
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
                default:
                    return new List<UnitState>();
            }
        }

        private static List<(int x, int y)> GatherSpaces(TargetScope scope, UnitState source, int targetX, int targetY)
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

        /// <summary>Voluntary or automatic move; fails silently if blocked.</summary>
        private static bool TryMoveUnit(GameState state, UnitState unit, int dx, int dy, List<GameEvent> events)
        {
            int destX = unit.X + dx, destY = unit.Y + dy;
            if (!GameState.InBounds(destX, destY)) return false;
            if (GameState.SideOfRow(destY) != unit.Owner) return false;
            if (state.GetUnitAt(destX, destY) != null) return false;

            MoveUnitTo(state, unit, destX, destY, events);
            return true;
        }

        private static void MoveUnitTo(GameState state, UnitState unit, int destX, int destY, List<GameEvent> events)
        {
            int fromX = unit.X, fromY = unit.Y;
            bool leftBramble = state.SpaceEffects[fromX, fromY] == SpaceEffectType.Brambled;

            unit.X = destX;
            unit.Y = destY;
            events.Add(new GameEvent { Type = GameEventType.UnitMoved, UnitId = unit.Id, CardId = unit.CardId, X = fromX, Y = fromY, ToX = destX, ToY = destY });

            int forward = GameState.ForwardDir(unit.Owner);
            bool advanced = (destY - fromY) * forward > 0;
            bool retreated = (destY - fromY) * forward < 0;

            //Brambled: units take 1 damage entering or leaving the space
            bool enteredBramble = state.SpaceEffects[destX, destY] == SpaceEffectType.Brambled;
            if (leftBramble) DamageUnit(state, unit, 1, events);
            if (enteredBramble && state.Units.Contains(unit)) DamageUnit(state, unit, 1, events);

            if (!state.Units.Contains(unit)) return;

            foreach (var effect in unit.Definition.Effects)
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
            int destY = unit.Y + pushDir;

            //Pushed off the back of the field: no move
            if (!GameState.InBounds(unit.X, destY) || GameState.SideOfRow(destY) != unit.Owner) return;

            var occupant = state.GetUnitAt(unit.X, destY);
            if (occupant != null)
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

            unit.Damage += amount;
            events.Add(new GameEvent { Type = GameEventType.UnitDamaged, UnitId = unit.Id, CardId = unit.CardId, Amount = amount });

            if (unit.Asleep)
            {
                unit.Asleep = false;
                events.Add(new GameEvent { Type = GameEventType.UnitWoke, UnitId = unit.Id, CardId = unit.CardId });
            }

            if (state.CurrentLife(unit) <= 0)
                DestroyUnit(state, unit, events);
        }

        private static void HealUnit(GameState state, UnitState unit, int amount, List<GameEvent> events)
        {
            int healed = Math.Min(amount, unit.Damage);
            if (healed <= 0) return;

            unit.Damage -= healed;
            events.Add(new GameEvent { Type = GameEventType.UnitHealed, UnitId = unit.Id, CardId = unit.CardId, Amount = healed });
        }

        private static void DestroyUnit(GameState state, UnitState unit, List<GameEvent> events)
        {
            state.Units.Remove(unit);
            events.Add(new GameEvent { Type = GameEventType.UnitDestroyed, UnitId = unit.Id, CardId = unit.CardId, X = unit.X, Y = unit.Y });

            foreach (var effect in unit.Definition.Effects.Where(e => e.Trigger == Trigger.OnDestroy))
                ResolveEffect(state, effect, unit, unit.Owner, unit.X, unit.Y, events);
        }

        private static void DamagePlayer(GameState state, int player, int amount, List<GameEvent> events)
        {
            if (amount <= 0) return;

            state.Players[player].Life -= amount;
            events.Add(new GameEvent { Type = GameEventType.PlayerDamaged, Player = player, Amount = amount });

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
                if (playerState.Hand.Count >= GameConfig.MaxHandSize) return;

                string cardId = playerState.Deck[0];
                playerState.Deck.RemoveAt(0);
                playerState.Hand.Add(cardId);
                events.Add(new GameEvent { Type = GameEventType.CardDrawn, Player = player, CardId = cardId });
            }
        }
    }
}
