using System;
using System.Collections.Generic;
using System.Linq;
using LightCard.Core;
using LightCard.Core.Agents;

namespace LightCard.CoreTests
{
    /// <summary>
    /// Scenario tests for LightCard.Core, runnable with `dotnet run` from CoreTests/.
    /// No test framework dependency so it runs anywhere the SDK exists.
    /// </summary>
    public static class Program
    {
        private static int passed;
        private static int failed;

        public static int Main()
        {
            Test("Setup: opening hands and first turn", () =>
            {
                var events = new List<GameEvent>();
                var state = GameEngine.CreateGame(SampleDeck(), SampleDeck(), seed: 42, events);

                AssertEqual(GameConfig.StartingHandSize + GameConfig.CardsDrawnPerTurn, state.Players[0].Hand.Count, "p0 hand after turn start");
                AssertEqual(GameConfig.StartingHandSize, state.Players[1].Hand.Count, "p1 opening hand");
                AssertEqual(1, state.Players[0].Energy, "p0 starting energy");
                AssertEqual(0, state.ActivePlayer, "player 0 starts");
            });

            Test("Determinism: same seed, same shuffle", () =>
            {
                var a = GameEngine.CreateGame(SampleDeck(), SampleDeck(), 7, new List<GameEvent>());
                var b = GameEngine.CreateGame(SampleDeck(), SampleDeck(), 7, new List<GameEvent>());
                AssertTrue(a.Players[0].Hand.SequenceEqual(b.Players[0].Hand), "identical p0 hands");
                AssertTrue(a.Players[1].Deck.SequenceEqual(b.Players[1].Deck), "identical p1 decks");

                var c = GameEngine.CreateGame(SampleDeck(), SampleDeck(), 8, new List<GameEvent>());
                AssertTrue(!a.Players[0].Deck.SequenceEqual(c.Players[0].Deck), "different seed shuffles differently");
            });

            Test("Play unit: placement, energy, Flux", () =>
            {
                var state = EmptyGame();
                var unit = PlayUnit(state, 0, "Conscript", 0, 2);

                AssertEqual(0, state.Players[0].Energy, "energy spent");
                AssertTrue(unit.Flux, "freshly called unit is in Flux");
                AssertTrue(!GameEngine.Execute(state, new AttackCommand { Player = 0, UnitId = unit.Id }).Success, "cannot attack while in Flux");

                var offSide = GameEngine.Execute(state, Play(state, 0, "Conscript", 0, 3));
                AssertTrue(!offSide.Success, "cannot call onto the enemy half");
            });

            Test("Attack: unblocked lane hits the player", () =>
            {
                var state = EmptyGame();
                var unit = PlayUnit(state, 0, "Conscript", 0, 2);
                unit.Flux = false;

                var result = GameEngine.Execute(state, new AttackCommand { Player = 0, UnitId = unit.Id });
                AssertTrue(result.Success, result.Error);
                AssertEqual(GameConfig.StartingLife - 1, state.Players[1].Life, "opponent took 1 damage");
                AssertTrue(!GameEngine.Execute(state, new AttackCommand { Player = 0, UnitId = unit.Id }).Success, "one attack per turn");
            });

            Test("Attack: blocked lane hits the front-most enemy, armor applies", () =>
            {
                var state = EmptyGame();
                var attacker = PlayUnit(state, 0, "Spearbearer", 1, 2); //2 power, Pierce 1
                attacker.Flux = false;
                var front = PlayUnit(state, 1, "Shieldbearer", 1, 3);   //Armor 1, 4 life
                var back = PlayUnit(state, 1, "Conscript", 1, 4);       //2 life

                var result = GameEngine.Execute(state, new AttackCommand { Player = 0, UnitId = attacker.Id });
                AssertTrue(result.Success, result.Error);
                AssertEqual(1, front.Damage, "armor reduced 2 damage to 1");
                //the Conscript is adjacent to Shieldbearer, so its aura grants it Armor 1 too
                AssertEqual(1, back.Damage, "pierce damage, reduced by aura armor");
                AssertEqual(GameConfig.StartingLife, state.Players[1].Life, "player untouched when lane is blocked");
            });

            Test("Melee: cannot attack from behind a friendly unit", () =>
            {
                var state = EmptyGame();
                var behind = PlayUnit(state, 0, "Conscript", 2, 1);
                PlayUnit(state, 0, "Conscript", 2, 2);
                behind.Flux = false;

                AssertTrue(!GameEngine.Execute(state, new AttackCommand { Player = 0, UnitId = behind.Id }).Success,
                    "unit behind a friendly cannot attack");
            });

            Test("Shieldbearer aura: adjacent allies gain Armor 1", () =>
            {
                var state = EmptyGame();
                var bearer = PlayUnit(state, 0, "Shieldbearer", 1, 1);
                var ally = PlayUnit(state, 0, "Conscript", 1, 2);
                var far = PlayUnit(state, 0, "Conscript", 0, 0);

                AssertEqual(1, state.EffectiveArmor(ally), "adjacent ally armored");
                AssertEqual(0, state.EffectiveArmor(far), "distant ally not armored");
                AssertEqual(1, state.EffectiveArmor(bearer), "own printed armor only");
            });

            Test("Vanguard: +2/0 only on the frontline", () =>
            {
                var state = EmptyGame();
                var vanguard = PlayUnit(state, 0, "Vanguard", 0, 1);
                AssertEqual(2, state.EffectivePower(vanguard), "base power off the frontline");

                vanguard.Y = 2;
                AssertEqual(4, state.EffectivePower(vanguard), "boosted on the frontline");
            });

            Test("Thorny Hedge retaliates", () =>
            {
                var state = EmptyGame();
                var attacker = PlayUnit(state, 0, "Conscript", 0, 2);
                attacker.Flux = false;
                PlayUnit(state, 1, "Thorny Hedge", 0, 3);

                var result = GameEngine.Execute(state, new AttackCommand { Player = 0, UnitId = attacker.Id });
                AssertTrue(result.Success, result.Error);
                AssertEqual(1, attacker.Damage, "attacker took retaliation damage");
            });

            Test("Siege Knight pushes on attack, collisions damage both", () =>
            {
                var state = EmptyGame();
                var knight = PlayUnit(state, 0, "Siege Knight", 0, 2);
                knight.Flux = false;
                var target = PlayUnit(state, 1, "Shieldbearer", 0, 3);

                GameEngine.Execute(state, new AttackCommand { Player = 0, UnitId = knight.Id });
                AssertEqual(4, target.Y, "surviving target pushed back a row");

                knight.AttackedThisTurn = false;
                var blocker = PlayUnit(state, 1, "Conscript", 0, 5);
                target.Y = 3;
                blocker.Y = 4;
                int targetDamageBefore = target.Damage;

                GameEngine.Execute(state, new AttackCommand { Player = 0, UnitId = knight.Id });
                AssertEqual(3, target.Y, "blocked push does not move the target");
                AssertEqual(targetDamageBefore + 1 + 1, target.Damage, "attack damage plus collision damage");
                AssertEqual(1, blocker.Damage, "collision damages the blocker too");
            });

            Test("Shift: swap places, once per turn, costs energy", () =>
            {
                var state = EmptyGame();
                var a = PlayUnit(state, 0, "Conscript", 0, 1);
                var b = PlayUnit(state, 0, "Conscript", 0, 2);
                state.Players[0].Energy = 2;

                var result = GameEngine.Execute(state, new ShiftCommand { Player = 0, UnitId = a.Id, Direction = MoveDirection.Forward });
                AssertTrue(result.Success, result.Error);
                AssertEqual(2, a.Y, "shifted unit moved forward");
                AssertEqual(1, b.Y, "occupant swapped back");
                AssertEqual(1, state.Players[0].Energy, "shift cost 1 energy");
                AssertTrue(a.MovedThisTurn, "shifted unit is marked moved");
                AssertTrue(!GameEngine.Execute(state, new ShiftCommand { Player = 0, UnitId = b.Id, Direction = MoveDirection.Forward }).Success,
                    "only one shift per turn");
            });

            Test("Replace: ramp energy and affinity", () =>
            {
                var state = EmptyGame();
                state.Players[0].Hand.Add("Conscript");

                var result = GameEngine.Execute(state, new ReplaceCardCommand { Player = 0, HandIndex = 0 });
                AssertTrue(result.Success, result.Error);
                AssertEqual(2, state.Players[0].MaxEnergy, "max energy ramped");
                AssertEqual(2, state.Players[0].Energy, "current energy ramped");
                AssertEqual(1, state.Players[0].Affinity[Archetype.Expedition], "affinity gained");
                AssertTrue(!GameEngine.Execute(state, new ReplaceCardCommand { Player = 0, HandIndex = 0 }).Success,
                    "only one replace per turn");
            });

            Test("Vista: units called there gain +1/+1", () =>
            {
                var state = EmptyGame();
                PlayAbility(state, 0, "Promised Land", 0, 1);
                AssertEqual(SpaceEffectType.Vista, state.SpaceEffects[0, 1], "Vista applied");

                var unit = PlayUnit(state, 0, "Conscript", 0, 1);
                AssertEqual(2, state.EffectivePower(unit), "power boosted by Vista");
                AssertEqual(3, state.EffectiveMaxLife(unit), "life boosted by Vista");
            });

            Test("Auto-Advance: moves at turn start and triggers Advance", () =>
            {
                var state = EmptyGame();
                var recruit = PlayUnit(state, 0, "Eager Recruit", 0, 0);

                GameEngine.Execute(state, new EndTurnCommand { Player = 0 });   //to p1
                GameEngine.Execute(state, new EndTurnCommand { Player = 1 });   //back to p0: auto-advance fires

                AssertEqual(1, recruit.Y, "recruit advanced");
                AssertEqual(1, recruit.BonusPower, "Advance trigger granted +1/0");
            });

            Test("Bramble: damages units entering or leaving", () =>
            {
                var state = EmptyGame();
                var unit = PlayUnit(state, 0, "Shieldbearer", 0, 1);
                state.SpaceEffects[0, 2] = SpaceEffectType.Brambled;
                state.Players[0].Energy = 1;

                GameEngine.Execute(state, new ShiftCommand { Player = 0, UnitId = unit.Id, Direction = MoveDirection.Forward });
                AssertEqual(1, unit.Damage, "damaged entering bramble");

                state.Players[0].ShiftUsedThisTurn = false;
                state.Players[0].Energy = 1;
                GameEngine.Execute(state, new ShiftCommand { Player = 0, UnitId = unit.Id, Direction = MoveDirection.Back });
                AssertEqual(2, unit.Damage, "damaged leaving bramble");
            });

            Test("Garden loop: Sower makes Verdant, Sprout grows, Verdant regens", () =>
            {
                var state = EmptyGame();
                var sower = PlayUnit(state, 0, "Sower of Seeds", 0, 0);
                var sprout = PlayUnit(state, 0, "Growing Sprout", 1, 0);
                state.SpaceEffects[1, 0] = SpaceEffectType.Verdant;
                sprout.Damage = 2;

                GameEngine.Execute(state, new EndTurnCommand { Player = 0 });
                AssertEqual(SpaceEffectType.Verdant, state.SpaceEffects[sower.X, sower.Y], "Sower planted its space");
                AssertEqual(1, sprout.BonusPower, "Sprout grew power on Verdant");
                AssertEqual(1, sprout.BonusLife, "Sprout grew life on Verdant");

                GameEngine.Execute(state, new EndTurnCommand { Player = 1 });   //p0 turn start: Verdant regen
                AssertEqual(1, sprout.Damage, "regenerated 1 at turn start");
            });

            Test("Reverie: sleeping units cannot act, wake on damage", () =>
            {
                var state = EmptyGame();
                var unit = PlayUnit(state, 0, "Conscript", 0, 2);
                unit.Flux = false;
                state.ActivePlayer = 1;
                PlayAbility(state, 1, "Reverie", 0, 2);
                state.ActivePlayer = 0;

                AssertTrue(unit.Asleep, "unit fell asleep");
                AssertTrue(!GameEngine.Execute(state, new AttackCommand { Player = 0, UnitId = unit.Id }).Success, "asleep units cannot attack");

                var enemy = PlayUnit(state, 1, "Conscript", 0, 3);
                enemy.Flux = false;
                state.ActivePlayer = 1;
                GameEngine.Execute(state, new AttackCommand { Player = 1, UnitId = enemy.Id });
                AssertTrue(!unit.Asleep, "damage wakes the unit");
            });

            Test("Kiss From A Rose: heal then bramble", () =>
            {
                var state = EmptyGame();
                var unit = PlayUnit(state, 0, "Shieldbearer", 2, 1);
                unit.Damage = 3;

                PlayAbility(state, 0, "Kiss From A Rose", 2, 1);
                AssertEqual(0, unit.Damage, "healed 3");
                AssertEqual(SpaceEffectType.Brambled, state.SpaceEffects[2, 1], "space brambled");
            });

            Test("Rose Beast: counts board-wide space effects on call", () =>
            {
                var state = EmptyGame();
                state.SpaceEffects[0, 0] = SpaceEffectType.Brambled;
                state.SpaceEffects[1, 4] = SpaceEffectType.Brambled;
                state.SpaceEffects[2, 2] = SpaceEffectType.Verdant;

                var beast = PlayUnit(state, 0, "Rose Beast", 1, 1);
                AssertEqual(3, state.EffectivePower(beast), "1 base + 2 brambled spaces");
                AssertEqual(2, state.EffectiveMaxLife(beast), "1 base + 1 verdant space");
            });

            Test("Fate-Cursed Lover: Verdant row on call, Bramble row on death", () =>
            {
                var state = EmptyGame();
                var lover = PlayUnit(state, 1, "Fate-Cursed Lover", 1, 3);
                AssertEqual(SpaceEffectType.Verdant, state.SpaceEffects[0, 3], "row turned Verdant");

                var attacker = PlayUnit(state, 0, "Siege Knight", 1, 2);
                attacker.Flux = false;
                lover.Damage = 2; //one hit from death
                GameEngine.Execute(state, new AttackCommand { Player = 0, UnitId = attacker.Id });

                AssertTrue(!state.Units.Contains(lover), "lover destroyed");
                AssertEqual(SpaceEffectType.Brambled, state.SpaceEffects[2, 3], "row turned Brambled on death");
            });

            Test("Cosmic Flower: grants energy when destroyed", () =>
            {
                var state = EmptyGame();
                var flower = PlayUnit(state, 1, "Cosmic Flower", 0, 3);
                var attacker = PlayUnit(state, 0, "Spearbearer", 0, 2);
                attacker.Flux = false;
                flower.Damage = 1; //2 life, one hit from death

                int energyBefore = state.Players[1].Energy;
                GameEngine.Execute(state, new AttackCommand { Player = 0, UnitId = attacker.Id });
                AssertTrue(!state.Units.Contains(flower), "flower destroyed");
                AssertEqual(energyBefore + 1, state.Players[1].Energy, "owner gained 1 energy");
            });

            Test("Guest Registry: grows when the opponent summons", () =>
            {
                var state = EmptyGame();
                var registry = PlayUnit(state, 0, "Guest Registry", 0, 0);
                PlayUnit(state, 1, "Conscript", 2, 5);

                AssertEqual(1, registry.BonusLife, "registry grew on enemy summon");
            });

            Test("Guiding Star: only unblocked units gain power", () =>
            {
                var state = EmptyGame();
                var open = PlayUnit(state, 0, "Conscript", 0, 2);
                var blocked = PlayUnit(state, 0, "Conscript", 1, 2);
                PlayUnit(state, 1, "Conscript", 1, 3);

                PlayAbility(state, 0, "Guiding Star", 0, 0);
                AssertEqual(1, open.BonusPower, "unblocked unit buffed");
                AssertEqual(0, blocked.BonusPower, "blocked unit not buffed");
            });

            Test("Lethal: reducing life to 0 ends the game", () =>
            {
                var state = EmptyGame();
                var unit = PlayUnit(state, 0, "Siege Knight", 0, 2);
                unit.Flux = false;
                state.Players[1].Life = 2;

                GameEngine.Execute(state, new AttackCommand { Player = 0, UnitId = unit.Id });
                AssertEqual(0, state.Winner, "player 0 wins");
                AssertTrue(!GameEngine.Execute(state, new EndTurnCommand { Player = 0 }).Success, "no commands after game end");
            });

            Test("Playout: two scripted bots finish deterministically", () =>
            {
                var logA = RunScriptedPlayout(seed: 99);
                var logB = RunScriptedPlayout(seed: 99);
                AssertTrue(logA.SequenceEqual(logB), "identical playouts for identical seeds");
                AssertTrue(logA.Count > 20, "playout actually did things");
            });

            Test("Agent: takes lethal when available", () =>
            {
                var state = EmptyGame();
                var knight = PlayUnit(state, 0, "Siege Knight", 0, 2);
                knight.Flux = false;
                state.Players[1].Life = 2;

                var agent = new HeuristicAgent(0, AgentPersonality.Balanced());
                var command = agent.ChooseCommand(state);

                AssertTrue(command is AttackCommand attack && attack.UnitId == knight.Id,
                    $"expected lethal attack, got {command.GetType().Name}");
            });

            Test("Agent: develops instead of passing on turn one", () =>
            {
                var state = GameEngine.CreateGame(SampleDeck(), SampleDeck(), 5, new List<GameEvent>());
                var agent = new HeuristicAgent(0, AgentPersonality.Balanced());

                var command = agent.ChooseCommand(state);
                AssertTrue(!(command is EndTurnCommand), "turn one should not be a pass");
            });

            Test("Agent match: Expedition vs Garden completes deterministically", () =>
            {
                var a = MatchRunner.PlayMatch(ArchetypeDeck(Archetype.Expedition), ArchetypeDeck(Archetype.Garden),
                    AgentPersonality.Formation(), AgentPersonality.Patient(), seed: 3);
                var b = MatchRunner.PlayMatch(ArchetypeDeck(Archetype.Expedition), ArchetypeDeck(Archetype.Garden),
                    AgentPersonality.Formation(), AgentPersonality.Patient(), seed: 3);

                AssertEqual(0, a.FailedCommands, "agents never propose illegal commands");
                AssertTrue(a.Winner >= 0, $"match should finish (winner {a.Winner} after {a.Turns} turns, {a.CommandsIssued} commands)");
                AssertEqual(a.Winner, b.Winner, "same winner for same seed");
                AssertEqual(a.CommandsIssued, b.CommandsIssued, "same command count for same seed");
                AssertEqual(a.Events.Count, b.Events.Count, "same event count for same seed");
                Console.WriteLine($"      ({a.Turns} turns, {a.CommandsIssued} commands, winner p{a.Winner}: " +
                                  $"{(a.Winner == 0 ? "Formation/Expedition" : "Patient/Garden")})");
            });

            Console.WriteLine();
            Console.WriteLine($"{passed} passed, {failed} failed");
            return failed == 0 ? 0 : 1;
        }

        //---- Scripted playout bot: greedy play-then-attack, no randomness ----

        private static List<string> RunScriptedPlayout(int seed)
        {
            var log = new List<string>();
            var events = new List<GameEvent>();
            var state = GameEngine.CreateGame(SampleDeck(), SampleDeck(), seed, events);
            foreach (var gameEvent in events) log.Add(gameEvent.ToString());

            for (int step = 0; step < 400 && !state.IsOver; step++)
            {
                var command = ChooseCommand(state);
                var result = GameEngine.Execute(state, command);
                if (!result.Success)
                {
                    //A bot bug, not an engine bug — but surface it loudly
                    throw new Exception($"Bot issued invalid command: {result.Error}");
                }
                foreach (var gameEvent in result.Events) log.Add(gameEvent.ToString());
            }

            log.Add($"final p0:{state.Players[0].Life} p1:{state.Players[1].Life} winner:{state.Winner}");
            return log;
        }

        private static Command ChooseCommand(GameState state)
        {
            int player = state.ActivePlayer;
            var playerState = state.Players[player];

            //Play the first affordable, placeable card
            for (int handIndex = 0; handIndex < playerState.Hand.Count; handIndex++)
            {
                var definition = CardCatalogV1.Get(playerState.Hand[handIndex]);
                if (definition.Cost > playerState.Energy) continue;

                var target = FindTarget(state, player, definition);
                if (target == null) continue;

                return new PlayCardCommand { Player = player, HandIndex = handIndex, TargetX = target.Value.x, TargetY = target.Value.y };
            }

            //Attack with the first legal attacker
            foreach (var unit in state.UnitsOf(player))
            {
                if (unit.IsCharm || unit.Flux || unit.Asleep || unit.AttackedThisTurn || unit.MovedThisTurn) continue;
                if (state.EffectivePower(unit) <= 0) continue;
                var probe = GameEngine.Execute(state.Clone(), new AttackCommand { Player = player, UnitId = unit.Id });
                if (probe.Success)
                    return new AttackCommand { Player = player, UnitId = unit.Id };
            }

            return new EndTurnCommand { Player = player };
        }

        private static (int x, int y)? FindTarget(GameState state, int player, CardDefinition definition)
        {
            switch (definition.PlayTarget)
            {
                case PlayTargetKind.None:
                    return (0, 0);
                case PlayTargetKind.FriendlyEmptySpace:
                {
                    for (int y = GameState.FrontlineRow(player); ; y -= GameState.ForwardDir(player))
                    {
                        if (!GameState.InBounds(0, y) || GameState.SideOfRow(y) != player) break;
                        for (int x = 0; x < GameConfig.Lanes; x++)
                            if (state.GetUnitAt(x, y) == null) return (x, y);
                    }
                    return null;
                }
                case PlayTargetKind.AnySpace:
                    return (1, GameState.FrontlineRow(player));
                case PlayTargetKind.AnyUnit:
                {
                    var unit = state.Units.FirstOrDefault();
                    return unit != null ? (unit.X, unit.Y) : ((int x, int y)?)null;
                }
                default:
                    return null;
            }
        }

        //---- Helpers ----

        private static List<string> ArchetypeDeck(Archetype archetype)
        {
            var deck = new List<string>();
            foreach (var card in CardCatalogV1.Cards.Values.Where(c => c.Archetype == archetype))
            {
                deck.Add(card.Id);
                deck.Add(card.Id);
            }
            return deck;
        }

        private static List<string> SampleDeck()
        {
            var deck = new List<string>();
            foreach (var card in CardCatalogV1.Cards.Values)
            {
                deck.Add(card.Id);
                deck.Add(card.Id);
            }
            return deck;
        }

        /// <summary>A started game with empty decks so tests control every card.</summary>
        private static GameState EmptyGame()
        {
            return GameEngine.CreateGame(new List<string>(), new List<string>(), 1, new List<GameEvent>());
        }

        private static PlayCardCommand Play(GameState state, int player, string cardId, int x, int y)
        {
            state.Players[player].Hand.Add(cardId);
            var definition = CardCatalogV1.Get(cardId);
            if (state.Players[player].Energy < definition.Cost)
                state.Players[player].Energy = definition.Cost;
            return new PlayCardCommand { Player = player, HandIndex = state.Players[player].Hand.Count - 1, TargetX = x, TargetY = y };
        }

        private static UnitState PlayUnit(GameState state, int player, string cardId, int x, int y)
        {
            int previousActive = state.ActivePlayer;
            state.ActivePlayer = player;
            var result = GameEngine.Execute(state, Play(state, player, cardId, x, y));
            state.ActivePlayer = previousActive;
            if (!result.Success) throw new Exception($"PlayUnit {cardId} failed: {result.Error}");
            return state.GetUnitAt(x, y) ?? throw new Exception($"{cardId} not found at ({x},{y})");
        }

        private static void PlayAbility(GameState state, int player, string cardId, int x, int y)
        {
            int previousActive = state.ActivePlayer;
            state.ActivePlayer = player;
            var result = GameEngine.Execute(state, Play(state, player, cardId, x, y));
            state.ActivePlayer = previousActive;
            if (!result.Success) throw new Exception($"PlayAbility {cardId} failed: {result.Error}");
        }

        //---- Micro test framework ----

        private static void Test(string name, Action body)
        {
            try
            {
                body();
                passed++;
                Console.WriteLine($"PASS  {name}");
            }
            catch (Exception e)
            {
                failed++;
                Console.WriteLine($"FAIL  {name}");
                Console.WriteLine($"      {e.Message}");
            }
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        private static void AssertEqual(int expected, int actual, string message)
        {
            if (expected != actual) throw new Exception($"{message}: expected {expected}, got {actual}");
        }

        private static void AssertEqual(SpaceEffectType expected, SpaceEffectType actual, string message)
        {
            if (expected != actual) throw new Exception($"{message}: expected {expected}, got {actual}");
        }
    }
}
