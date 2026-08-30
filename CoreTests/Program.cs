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
                AssertEqual(GameConfig.StartingHandSize + GameConfig.SecondPlayerBonusCards, state.Players[1].Hand.Count, "p1 opening hand includes going-second bonus");
                AssertEqual(0, state.Players[0].Energy, "no automatic energy - Replace is the only ramp");
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
                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
                AssertTrue(!unit.AttackedThisTurn, "units in Flux do not auto-attack");

                var offSide = GameEngine.Execute(state, Play(state, 0, "Conscript", 0, 3));
                AssertTrue(!offSide.Success, "cannot call onto the enemy half");
            });

            Test("Attack: unblocked lane hits the player", () =>
            {
                var state = EmptyGame();
                var unit = PlayUnit(state, 0, "Conscript", 0, 2);
                unit.Flux = false;

                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
                AssertEqual(GameConfig.StartingLife - 1, state.Players[1].Life, "opponent took 1 damage");
                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
                AssertEqual(GameConfig.StartingLife - 1, state.Players[1].Life, "one attack per turn");
            });

            Test("Attack: blocked lane hits the front-most enemy, armor applies", () =>
            {
                var state = EmptyGame();
                var attacker = PlayUnit(state, 0, "Spearbearer", 1, 2); //2 power, Pierce 1
                attacker.Flux = false;
                var front = PlayUnit(state, 1, "Shieldbearer", 1, 3);   //Armor 1, 4 life
                var back = PlayUnit(state, 1, "Conscript", 1, 4);       //2 life

                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
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

                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
                AssertTrue(!behind.AttackedThisTurn, "unit behind a friendly does not auto-attack");
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

                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
                AssertEqual(1, attacker.Damage, "attacker took retaliation damage");
            });

            Test("Siege Knight pushes on attack, collisions damage both", () =>
            {
                var state = EmptyGame();
                var knight = PlayUnit(state, 0, "Siege Knight", 0, 2);
                knight.Flux = false;
                var target = PlayUnit(state, 1, "Shieldbearer", 0, 3);

                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
                AssertEqual(4, target.Y, "surviving target pushed back a row");

                knight.AttackedThisTurn = false;
                var blocker = PlayUnit(state, 1, "Conscript", 0, 5);
                target.Y = 3;
                blocker.Y = 4;
                int targetDamageBefore = target.Damage;

                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
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
                AssertEqual(1, state.Players[0].MaxEnergy, "max energy ramped from 0");
                AssertEqual(1, state.Players[0].Energy, "current energy ramped from 0");
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
                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
                AssertTrue(!unit.AttackedThisTurn, "asleep units do not auto-attack");

                var enemy = PlayUnit(state, 1, "Conscript", 0, 3);
                enemy.Flux = false;
                GameEngine.ResolveAutoAttacks(state, 1, new List<GameEvent>());
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
                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());

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
                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
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

            Test("Affinity gating: card blocked until AL met (rules-v2)", () =>
            {
                var state = EmptyGame();
                state.Players[0].Hand.Add("Siege Knight");    //cost 4 -> default AL requirement 3
                state.Players[0].Energy = 4;

                var blocked = GameEngine.Execute(state, new PlayCardCommand { Player = 0, HandIndex = 0, TargetX = 0, TargetY = 0 });
                AssertTrue(!blocked.Success, "play blocked without affinity");

                state.Players[0].Affinity[Archetype.Expedition] = 3;
                var allowed = GameEngine.Execute(state, new PlayCardCommand { Player = 0, HandIndex = 0, TargetX = 0, TargetY = 0 });
                AssertTrue(allowed.Success, allowed.Error);
            });

            Test("Fatigue: empty-deck draws deal escalating damage (rules-v2)", () =>
            {
                var state = EmptyGame();
                int lifeBefore = state.Players[1].Life;

                GameEngine.Execute(state, new EndTurnCommand { Player = 0 });   //p1 turn start: 2 missed draws
                AssertEqual(lifeBefore - (1 + 2), state.Players[1].Life, "1 then 2 fatigue damage");

                GameEngine.Execute(state, new EndTurnCommand { Player = 1 });   //p0: 1+2
                GameEngine.Execute(state, new EndTurnCommand { Player = 0 });   //p1: 3+4
                AssertEqual(lifeBefore - (1 + 2 + 3 + 4), state.Players[1].Life, "fatigue keeps escalating");
                AssertTrue(!state.IsOver, "still alive at 10 fatigue damage");
            });

            Test("Lances: sweep decays/grows per unit hit, friend or foe (Atelier)", () =>
            {
                var state = EmptyGame();
                var mine = PlayUnit(state, 0, "Automaton", 0, 1);          //1/1, friendly fire
                var frontEnemy = PlayUnit(state, 1, "Workshop Guardian", 0, 3); //Resist 1, 6 life
                var backEnemy = PlayUnit(state, 1, "Automaton", 0, 5);     //1/1

                PlayAbility(state, 0, "Diminishing Lance", 0, 0);           //3 dmg, -1 per hit
                AssertTrue(!state.Units.Contains(mine), "own unit hit first for 3 and died");
                AssertEqual(1, frontEnemy.Damage, "second hit: 2 damage minus Resist 1");
                AssertTrue(!state.Units.Contains(backEnemy), "third hit: 1 damage killed it");

                var a = PlayUnit(state, 1, "Art Critic", 1, 3);             //Resist 1, 4 life
                var b = PlayUnit(state, 1, "Automaton", 1, 4);              //1/1
                PlayAbility(state, 0, "Magnifying Lance", 1, 0);            //1 dmg, +1 per hit
                AssertEqual(0, a.Damage, "first hit: 1 damage fully resisted (still counts)");
                AssertTrue(!state.Units.Contains(b), "second hit: 2 damage killed it");
            });

            Test("Primed: +2 ability damage, consumed (Atelier)", () =>
            {
                var state = EmptyGame();
                PlayAbility(state, 0, "Splash of Primer", 0, 3);
                AssertEqual(SpaceEffectType.Primed, state.SpaceEffects[2, 3], "whole row primed");

                var victim = PlayUnit(state, 1, "Workshop Guardian", 0, 3); //Resist 1
                PlayAbility(state, 0, "Magnifying Lance", 0, 0);            //1 +2 primed -1 resist = 2
                AssertEqual(2, victim.Damage, "primed boost applied through resist");
                AssertEqual(SpaceEffectType.None, state.SpaceEffects[0, 3], "primed consumed");
                AssertEqual(SpaceEffectType.Primed, state.SpaceEffects[1, 3], "untouched spaces stay primed");
            });

            Test("Ability-play triggers: Diligent Student and Lightning Rod (Atelier)", () =>
            {
                var state = EmptyGame();
                var student = PlayUnit(state, 0, "Diligent Student", 2, 0);
                var rod = PlayUnit(state, 1, "Lightning Rod", 0, 3);        //enemy charm watching us
                var ourFront = PlayUnit(state, 0, "Automaton", 0, 2);       //nearest to the rod in its lane

                PlayAbility(state, 0, "Splash of Primer", 2, 5);
                AssertEqual(1, student.BonusLife, "student grew on our ability");
                AssertEqual(1, ourFront.Damage, "rod zapped our nearest unit in its lane");
                AssertEqual(0, rod.Damage, "rod untouched");

                PlayUnit(state, 0, "Automaton", 1, 0);
                AssertEqual(1, student.BonusLife, "unit plays do not trigger ability watchers");
            });

            Test("Sharpen Edge grants Pierce; Master Painter primes what it strikes (Atelier)", () =>
            {
                var state = EmptyGame();
                var painter = PlayUnit(state, 0, "Master Painter", 1, 2);
                painter.Flux = false;
                PlayAbility(state, 0, "Sharpen Edge", 1, 2);
                AssertEqual(4, state.EffectivePower(painter), "2 base +2 sharpened");

                var front = PlayUnit(state, 1, "Workshop Guardian", 1, 3);  //Armor 1, 6 life
                var back = PlayUnit(state, 1, "Automaton", 1, 4);
                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
                AssertEqual(3, front.Damage, "4 power - 1 armor");
                AssertEqual(SpaceEffectType.Primed, state.SpaceEffects[1, 3], "struck space primed");
                AssertTrue(!state.Units.Contains(back), "granted Pierce carried the attack through");
            });

            Test("Erasure: clears first, then damages - no Primed bonus on itself (Atelier)", () =>
            {
                var state = EmptyGame();
                var victim = PlayUnit(state, 1, "Art Critic", 2, 3);        //Resist 1, 4 life
                state.SpaceEffects[2, 3] = SpaceEffectType.Primed;

                PlayAbility(state, 0, "Erasure", 2, 3);
                AssertEqual(SpaceEffectType.None, state.SpaceEffects[2, 3], "effect removed");
                AssertEqual(2, victim.Damage, "3 - resist 1, no primed bonus");

                var untouched = PlayUnit(state, 1, "Automaton", 0, 3);
                PlayAbility(state, 0, "Erasure", 0, 3);
                AssertEqual(0, untouched.Damage, "no effect removed, no damage");
            });

            Test("Movement keywords: Agile, Parry, Evade, temp buffs (rules-v2)", () =>
            {
                var state = EmptyGame();
                var duelist = PlayUnit(state, 0, "Duelist", 0, 1);
                duelist.Flux = false;
                state.Players[0].Energy = 5;
                GameEngine.Execute(state, new ShiftCommand { Player = 0, UnitId = duelist.Id, Direction = MoveDirection.Forward });
                AssertEqual(4, state.EffectivePower(duelist), "shift granted +2/0");
                AssertEqual(1, state.EffectiveParry(duelist), "shift granted Parry");

                var attacker = PlayUnit(state, 1, "Spearbearer", 0, 3);
                attacker.Flux = false;
                state.ActivePlayer = 1;
                GameEngine.ResolveAutoAttacks(state, 1, new List<GameEvent>());
                AssertEqual(0, duelist.Damage, "combat damage parried");

                GameEngine.Execute(state, new EndTurnCommand { Player = 1 });   //p0 turn start: temp grants expire
                AssertEqual(2, state.EffectivePower(duelist), "temp power expired");
                AssertEqual(0, state.EffectiveParry(duelist), "temp parry expired");

                var dancer = PlayUnit(state, 0, "Dancer", 2, 1);
                var plain = PlayUnit(state, 0, "Conscript", 1, 1);
                dancer.Flux = false;
                plain.Flux = false;
                state.Players[0].Energy = 5;
                state.Players[0].ShiftUsedThisTurn = false;
                GameEngine.Execute(state, new ShiftCommand { Player = 0, UnitId = dancer.Id, Direction = MoveDirection.Forward });
                plain.MovedThisTurn = true; //simulate having moved without spending the shift
                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
                AssertTrue(dancer.AttackedThisTurn, "Agile units auto-attack after moving");
                AssertTrue(!plain.AttackedThisTurn, "moved non-Agile units sit the attack out");

                //Move/attack exclusivity: a non-Agile unit that attacked cannot Shift; an Agile one can
                plain.MovedThisTurn = false;
                plain.AttackedThisTurn = true;
                state.Players[0].ShiftUsedThisTurn = false;
                state.Players[0].Energy = 5;
                AssertTrue(!GameEngine.Execute(state, new ShiftCommand { Player = 0, UnitId = plain.Id, Direction = MoveDirection.Back }).Success,
                    "non-Agile units cannot move after attacking");
                AssertTrue(GameEngine.Execute(state, new ShiftCommand { Player = 0, UnitId = dancer.Id, Direction = MoveDirection.Back }).Success,
                    "Agile units may move after attacking");
            });

            Test("Overdraw burns: full hand still depletes the deck (rules-v2)", () =>
            {
                var state = EmptyGame();
                for (int n = 0; n < GameConfig.MaxHandSize; n++) state.Players[1].Hand.Add("Conscript");
                state.Players[1].Deck.Add("Conscript");
                state.Players[1].Deck.Add("Conscript");

                GameEngine.Execute(state, new EndTurnCommand { Player = 0 });   //p1 turn start: draws 2 into a full hand
                AssertEqual(GameConfig.MaxHandSize, state.Players[1].Hand.Count, "hand stays at cap");
                AssertEqual(0, state.Players[1].Deck.Count, "both drawn cards burned from the deck");
                AssertEqual(0, state.Players[1].Fatigue, "no fatigue while the deck lasts");
            });

            Test("Garden terrain package: tokens, pull, static space condition (rules-v2)", () =>
            {
                var state = EmptyGame();

                //CallUnit: Hedge Maze places three hedges on the owner's half, deterministically
                PlayAbility(state, 0, "Hedge Maze", 0, 0);
                AssertEqual(3, state.Units.Count, "three hedges called");
                AssertTrue(state.Units.All(u => GameState.SideOfRow(u.Y) == 0), "tokens stay on the owner's half");

                //Fertile Soil fills only empty own-half Verdant spaces
                state.SpaceEffects[0, 5] = SpaceEffectType.Verdant;      //enemy half: ignored
                for (int x = 0; x < GameConfig.Lanes; x++)               //first empty own-half space
                {
                    bool done = false;
                    for (int y = 0; y < GameConfig.RowsPerSide && !done; y++)
                        if (state.GetUnitAt(x, y) == null) { state.SpaceEffects[x, y] = SpaceEffectType.Verdant; done = true; }
                    if (done) break;
                }
                int before = state.Units.Count;
                PlayAbility(state, 0, "Fertile Soil", 0, 0);
                AssertEqual(before + 1, state.Units.Count, "one flower on the one empty own-half Verdant space");

                //Static space condition: Constant Gardener only buffed while on Bramble
                var state2 = EmptyGame();
                var gardener = PlayUnit(state2, 0, "Constant Gardener", 0, 1);
                AssertEqual(2, state2.EffectivePower(gardener), "base power off bramble");
                state2.SpaceEffects[0, 1] = SpaceEffectType.Brambled;
                AssertEqual(3, state2.EffectivePower(gardener), "+1 while standing on bramble");

                //Entangling Vines: bramble first, then pull the row toward the caster
                var state3 = EmptyGame();
                var victim = PlayUnit(state3, 1, "Conscript", 1, 4);
                PlayAbility(state3, 0, "Entangling Vines", 1, 4);
                AssertEqual(3, victim.Y, "pulled one space toward the caster");
                AssertEqual(1, victim.Damage, "damaged leaving the freshly brambled space");
            });

            Test("Auto-attack: eligible units attack when their owner ends the turn (rules-v2)", () =>
            {
                var state = EmptyGame();
                var ready = PlayUnit(state, 0, "Conscript", 0, 2);
                ready.Flux = false;
                var influx = PlayUnit(state, 0, "Conscript", 1, 2);   //still in Flux: skipped
                var already = PlayUnit(state, 0, "Conscript", 2, 2);
                already.Flux = false;
                already.AttackedThisTurn = true;                       //spent: skipped

                int enemyLife = state.Players[1].Life;
                GameEngine.Execute(state, new EndTurnCommand { Player = 0 });
                AssertEqual(enemyLife - 1, state.Players[1].Life, "exactly one auto-attack hit the player");
                AssertTrue(ready.AttackedThisTurn, "eligible unit attacked automatically");
            });

            Test("Pin and Poison: lockdown and DoT (Garden, rules-v2)", () =>
            {
                var state = EmptyGame();
                var victim = PlayUnit(state, 1, "Vanguard", 0, 4);          //Auto-Advance, 5 life
                PlayAbility(state, 0, "Pin Down", 0, 4);
                PlayAbility(state, 0, "Pin Prick", 0, 4);
                AssertTrue(victim.Pinned, "pinned");
                AssertEqual(1, victim.Poison, "poisoned");
                AssertEqual(2, victim.Damage, "1+1 immediate damage");

                state.ActivePlayer = 1;
                state.Players[1].Energy = 5;
                AssertTrue(!GameEngine.Execute(state, new ShiftCommand { Player = 1, UnitId = victim.Id, Direction = MoveDirection.Forward }).Success,
                    "pinned units cannot shift");

                GameEngine.Execute(state, new EndTurnCommand { Player = 1 }); //pin expires; p0's turn
                AssertTrue(!victim.Pinned, "pin expired at owner's turn end");
                GameEngine.Execute(state, new EndTurnCommand { Player = 0 }); //p1 turn start: poison ticks, auto-advance runs
                AssertEqual(3, victim.Damage, "poison ticked 1 at owner's turn start");
                AssertEqual(3, victim.Y, "auto-advance works again once unpinned");
            });

            Test("Equip: entering the charm's space consumes and bestows (v3)", () =>
            {
                var state = EmptyGame();
                var lance = PlayUnit(state, 0, "Blood-Tinged Lance", 0, 2);
                var unit = PlayUnit(state, 0, "Conscript", 0, 1);
                unit.Flux = false;
                state.Players[0].Energy = 5;
                GameEngine.Execute(state, new ShiftCommand { Player = 0, UnitId = unit.Id, Direction = MoveDirection.Forward });
                AssertTrue(!state.Units.Contains(lance), "equip consumed");
                AssertEqual(3, state.EffectivePower(unit), "1 base +2 bestowed");
                AssertEqual(1, unit.BonusPierce, "pierce bestowed");
            });

            Test("Spirit Bind: soak, break burst, host grants (Heart, v3)", () =>
            {
                var state = EmptyGame();
                var host = PlayUnit(state, 0, "Novice Attuner", 0, 2);
                PlayAbility(state, 0, "Spirit of Wrath", 0, 2);   //spirit play routes through Play helper
                AssertEqual("Spirit of Wrath", state.GetUnit(host.Id).BoundSpiritCardId, "bonded");
                AssertEqual(1, host.BonusLife, "attuner OnBonded proc");

                var enemy = PlayUnit(state, 1, "Conscript", 0, 3);
                enemy.Flux = false;
                GameEngine.ResolveAutoAttacks(state, 1, new List<GameEvent>());
                AssertEqual(0, host.Damage, "spirit soaked the hit");
                AssertTrue(host.BoundSpiritCardId == null, "1-life spirit broke");
                AssertEqual(1, enemy.Damage, "Wrath burst hit the lane");
            });

            Test("Tower attrition: discard watchers and death value (v3)", () =>
            {
                var state = EmptyGame();
                var keeper = PlayUnit(state, 0, "Keeper of Debts", 0, 0);
                PlayUnit(state, 0, "Mourner's Altar", 1, 0);
                state.Players[0].Hand.Add("Conscript");
                state.Players[0].Deck.Add("Conscript");
                PlayAbility(state, 0, "Forlorn Whisper", 0, 0);
                AssertEqual(1, keeper.BonusPower, "keeper grew on discard");

                int life = state.Players[0].Life;
                var fodder = PlayUnit(state, 0, "Wretch", 2, 0);
                state.ActivePlayer = 1;
                PlayAbility(state, 1, "Burn", 2, 0);
                AssertTrue(!state.Units.Contains(fodder), "wretch burned down");
                AssertEqual(life + 1, state.Players[0].Life, "altar healed on friendly death");
            });

            Test("Ocean: consume, desert drain, deterministic mutations (v3)", () =>
            {
                var state = EmptyGame();
                var meal = PlayUnit(state, 0, "Chimera", 1, 1);
                var fractal = PlayUnit(state, 0, "Fractal", 1, 2);
                AssertTrue(!state.Units.Contains(meal), "fractal consumed its neighbor");
                AssertEqual(1, fractal.BonusPower, "+1/+1 gained");
                AssertTrue(!fractal.Flux, "gained Rush");

                var dune = PlayUnit(state, 1, "Dune Beast", 2, 3);
                AssertEqual(SpaceEffectType.Desert, state.SpaceEffects[2, 3], "deserted on call");
                GameEngine.Execute(state, new EndTurnCommand { Player = 0 });
                GameEngine.Execute(state, new EndTurnCommand { Player = 1 });
                AssertTrue(state.Units.Contains(dune), "bound unit survives on its desert");
                AssertEqual(-1, dune.BonusPower, "desert drained it");
            });

            Test("Lethal: reducing life to 0 ends the game", () =>
            {
                var state = EmptyGame();
                var unit = PlayUnit(state, 0, "Siege Knight", 0, 2);
                unit.Flux = false;
                state.Players[1].Life = 2;

                GameEngine.ResolveAutoAttacks(state, 0, new List<GameEvent>());
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
                var executed = GameEngine.Execute(state, command);

                AssertTrue(executed.Success && state.Winner == 0,
                    $"agent should reach lethal via auto-attack (chose {command.GetType().Name})");
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
                if (playerState.Affinity[definition.Archetype] < definition.AffinityRequirement) continue;

                var target = FindTarget(state, player, definition);
                if (target == null) continue;

                return new PlayCardCommand { Player = player, HandIndex = handIndex, TargetX = target.Value.x, TargetY = target.Value.y };
            }

            //Rules-v3: no manual attacks - combat happens on end turn automatically

            //Rules-v2: Replace is the only energy source, so the bot ramps once a turn
            if (!playerState.ReplaceUsedThisTurn && playerState.Hand.Count > 0)
                return new ReplaceCardCommand { Player = player, HandIndex = 0 };

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

        /// <summary>
        /// A running game with empty decks so tests control every card. Built by
        /// hand rather than via CreateGame: rules-v2 fatigue would damage both
        /// players during setup draws from the empty decks.
        /// </summary>
        private static GameState EmptyGame()
        {
            return new GameState { Seed = 1, ActivePlayer = 0, TurnNumber = 1 };
        }

        private static PlayCardCommand Play(GameState state, int player, string cardId, int x, int y)
        {
            state.Players[player].Hand.Add(cardId);
            var definition = CardCatalogV1.Get(cardId);
            if (state.Players[player].Energy < definition.Cost)
                state.Players[player].Energy = definition.Cost;
            if (state.Players[player].Affinity[definition.Archetype] < definition.AffinityRequirement)
                state.Players[player].Affinity[definition.Archetype] = definition.AffinityRequirement;
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
