using System;
using System.Collections.Generic;
using System.Linq;

namespace LightCard.Core
{
    /// <summary>A unit or charm on the field.</summary>
    public class UnitState
    {
        public int Id;
        public string CardId;
        public int Owner;
        public int X;
        public int Y;
        public int Damage;
        public int BonusPower;
        public int BonusLife;
        public int BonusPierce;
        public int BonusArmor;
        public int BonusParry;
        public int BonusResist;
        public bool GrantedHeavy;
        /// <summary>Extra abilities attached at runtime (equips, mutations). Null until first grant.</summary>
        public List<EffectDef> GrantedEffects;
        //Temp grants last until the start of the owner's next turn (rules-v2)
        public int TempPower;
        public int TempParry;
        public int TempEvade;
        public bool TempOverpower;
        public bool TempPushOnAttack;
        /// <summary>Destroyed at the end of its owner's turn (Give Your All).</summary>
        public bool TempDoomed;
        //Per-turn defensive charges consumed as damage is prevented
        public int ParryUsedThisTurn;
        public int EvadeUsedThisTurn;
        public bool Asleep;
        /// <summary>Pinned units cannot Shift or Auto-Advance; cleared when their owner's turn ends (rules-v2).</summary>
        public bool Pinned;
        /// <summary>Damage taken at the start of the owner's turn; permanent until cured (rules-v2).</summary>
        public int Poison;
        /// <summary>True on the turn a unit is called; units in Flux may not attack.</summary>
        public bool Flux;
        public bool AttackedThisTurn;
        public bool MovedThisTurn;
        public bool ActivatedThisTurn;
        /// <summary>Bound spirit's card id (Heart), or null; the spirit soaks all damage.</summary>
        public string BoundSpiritCardId;
        /// <summary>Damage the bound spirit has absorbed so far.</summary>
        public int SpiritDamage;

        public CardDefinition BoundSpirit => BoundSpiritCardId == null ? null : CardCatalogV1.Get(BoundSpiritCardId);

        public CardDefinition Definition => CardCatalogV1.Get(CardId);
        public bool IsCharm => Definition.Type == CardType.Charm;
        public bool IsHeavy => Definition.Heavy || GrantedHeavy || Definition.Immobile ||
                               (BoundSpirit != null && BoundSpirit.Heavy);

        /// <summary>
        /// Printed effects, runtime-granted abilities, and the bound spirit's
        /// non-break effects (so "Bond: ..." passives like Renewal and Ferocity
        /// live on the host while bonded).
        /// </summary>
        public IEnumerable<EffectDef> AllEffects
        {
            get
            {
                IEnumerable<EffectDef> effects = Definition.Effects;
                if (GrantedEffects != null) effects = effects.Concat(GrantedEffects);
                if (BoundSpirit != null)
                    effects = effects.Concat(BoundSpirit.Effects.Where(e => e.Trigger != Trigger.OnBondBreak));
                return effects;
            }
        }

        public UnitState Clone()
        {
            var copy = (UnitState)MemberwiseClone();
            if (GrantedEffects != null) copy.GrantedEffects = new List<EffectDef>(GrantedEffects);
            return copy;
        }
    }

    public class PlayerState
    {
        public int Life = GameConfig.StartingLife;
        public int Energy;
        public int MaxEnergy;
        public List<string> Deck = new List<string>();
        public List<string> Hand = new List<string>();
        public Dictionary<Archetype, int> Affinity = new Dictionary<Archetype, int>();
        public bool ReplaceUsedThisTurn;
        /// <summary>The once-per-turn player power (Shift or Clear) has been spent (rules-v3).</summary>
        public bool PowerUsedThisTurn;
        public int AbilitiesPlayedThisTurn;
        /// <summary>Missed draws from an empty deck so far; each deals its count in damage (rules-v2).</summary>
        public int Fatigue;
        /// <summary>Stacked discount consumed by the next Ability played (Attenuating Rod, adapted).</summary>
        public int NextAbilityDiscount;
        //Next-call riders (Virtuous Call, Valorous Call): consumed by the next Unit card played
        public int NextCallDiscount;
        public int NextCallPower;
        public int NextCallLife;
        public EffectDef NextCallGranted;

        public PlayerState()
        {
            foreach (Archetype archetype in Enum.GetValues(typeof(Archetype)))
                Affinity[archetype] = 0;
        }

        public PlayerState Clone()
        {
            var copy = (PlayerState)MemberwiseClone();
            copy.Deck = new List<string>(Deck);
            copy.Hand = new List<string>(Hand);
            copy.Affinity = new Dictionary<Archetype, int>(Affinity);
            return copy;
        }
    }

    /// <summary>A queued future effect (Sword of Damocles, Spirit Caller). Struct: cloned by value.</summary>
    public struct PendingAction
    {
        public int Player;
        public int TurnsLeft;
        public int X;
        public int Y;
        public int UnitId;
        public string CardId;
        public bool IsRebind; //false = destroy occupant of (X,Y)
    }

    public class GameState
    {
        public PlayerState[] Players = { new PlayerState(), new PlayerState() };
        public List<UnitState> Units = new List<UnitState>();
        public List<PendingAction> Pending = new List<PendingAction>();
        public SpaceEffectType[,] SpaceEffects = new SpaceEffectType[GameConfig.Lanes, GameConfig.Rows];
        public int ActivePlayer;
        public int TurnNumber;
        public int NextUnitId = 1;
        public int Seed;
        public int RngCalls;
        /// <summary>Id of the most recently resolved card play (Trace).</summary>
        public string LastCardPlayed;
        /// <summary>Unit id of the most recent combat attacker (Spirit of Reprisal); -1 outside combat.</summary>
        public int LastCombatAttackerId = -1;
        /// <summary>Card id of the most recently broken spirit (Soulcatcher).</summary>
        public string LastBrokenSpiritId;
        /// <summary>-1 while the game is running.</summary>
        public int Winner = -1;

        public bool IsOver => Winner >= 0;

        //---- Coordinate helpers ----

        public static bool InBounds(int x, int y) =>
            x >= 0 && x < GameConfig.Lanes && y >= 0 && y < GameConfig.Rows;

        /// <summary>Which player's half a row belongs to.</summary>
        public static int SideOfRow(int y) => y < GameConfig.RowsPerSide ? 0 : 1;

        /// <summary>+1 or -1: the y direction toward the enemy field.</summary>
        public static int ForwardDir(int player) => player == 0 ? 1 : -1;

        public static int FrontlineRow(int player) => player == 0 ? GameConfig.RowsPerSide - 1 : GameConfig.RowsPerSide;

        public static int BacklineRow(int player) => player == 0 ? 0 : GameConfig.Rows - 1;

        //---- Queries ----

        public UnitState GetUnitAt(int x, int y) =>
            Units.FirstOrDefault(u => u.X == x && u.Y == y);

        public UnitState GetUnit(int id) =>
            Units.FirstOrDefault(u => u.Id == id);

        public IEnumerable<UnitState> UnitsOf(int player) =>
            Units.Where(u => u.Owner == player);

        /// <summary>True if the player has no enemy unit or charm in the given lane.</summary>
        public bool LaneUnblockedFor(int player, int lane) =>
            !Units.Any(u => u.Owner != player && u.X == lane);

        /// <summary>
        /// Deterministic RNG: derived from the seed plus a call counter, so cloned
        /// states diverge identically and replays reproduce exactly.
        /// </summary>
        public int NextRandom(int maxExclusive)
        {
            var rng = new Random(Seed * 486187739 + RngCalls);
            RngCalls++;
            return rng.Next(maxExclusive);
        }

        //---- Effective stats (base + permanent bonuses + static effects) ----

        public int EffectivePower(UnitState unit)
        {
            int power = unit.Definition.Power + unit.BonusPower + unit.TempPower + StaticContribution(unit, EffectAction.StaticStats, statsPower: true);
            if (SpaceEffects[unit.X, unit.Y] == SpaceEffectType.Inferno && !unit.IsCharm) power += 1;
            foreach (var effect in unit.AllEffects)
                if (effect.Trigger == Trigger.Static && effect.Action == EffectAction.StaticPowerPerSpace)
                    power += CountSpaces(effect.SpaceEffect) * effect.Power;
            return Math.Max(0, power);
        }

        public int CountSpaces(SpaceEffectType effect)
        {
            int count = 0;
            for (int x = 0; x < GameConfig.Lanes; x++)
                for (int y = 0; y < GameConfig.Rows; y++)
                    if (SpaceEffects[x, y] == effect) count++;
            return count;
        }

        public int EffectiveParry(UnitState unit) =>
            unit.Definition.Parry + unit.BonusParry + unit.TempParry +
            (unit.BoundSpirit != null ? unit.BoundSpirit.Parry : 0) +
            StaticContribution(unit, EffectAction.StaticParry, statsPower: false);

        public int EffectiveResist(UnitState unit) =>
            unit.Definition.Resist + unit.BonusResist + (unit.BoundSpirit != null ? unit.BoundSpirit.Resist : 0);

        public int EffectiveAmplify(UnitState unit) =>
            unit.Definition.Amplify + (unit.BoundSpirit != null ? unit.BoundSpirit.Amplify : 0);

        public int EffectiveRegen(UnitState unit) =>
            StaticContribution(unit, EffectAction.StaticRegen, statsPower: false);

        /// <summary>True while any unit with the Sunlamp flag is on the board (Verdant/Bramble doubled).</summary>
        public bool GardenEffectsBoosted => Units.Any(u => u.Definition.BoostsGardenEffects);

        /// <summary>Card cost after discounts (Scholar, Attenuating Rod, next-call riders, Geo).</summary>
        public int EffectiveCost(int player, CardDefinition definition)
        {
            int cost = definition.Cost;
            if (definition.Type == CardType.Ability)
            {
                if (Players[player].AbilitiesPlayedThisTurn == 0)
                {
                    foreach (var source in UnitsOf(player))
                        foreach (var effect in source.AllEffects)
                            if (effect.Trigger == Trigger.Static && effect.Action == EffectAction.StaticAbilityDiscount)
                                cost -= effect.Amount;
                }
                cost -= Players[player].NextAbilityDiscount;
            }
            if (definition.Type == CardType.Unit) cost -= Players[player].NextCallDiscount;
            if (definition.CostPerOwnSpaceEffect)
            {
                for (int x = 0; x < GameConfig.Lanes; x++)
                    for (int y = 0; y < GameConfig.Rows; y++)
                        if (SideOfRow(y) == player && SpaceEffects[x, y] != SpaceEffectType.None) cost -= 1;
            }
            return Math.Max(0, cost);
        }

        /// <summary>Extra discount for calling to a specific space (Trailblazer/Flagbearer auras).</summary>
        public int CallDiscountAt(int player, int x, int y)
        {
            int discount = 0;
            foreach (var source in UnitsOf(player))
                foreach (var effect in source.AllEffects)
                    if (effect.Trigger == Trigger.Static && effect.Action == EffectAction.StaticCallDiscountBehind &&
                        x == source.X && y == source.Y - ForwardDir(player))
                        discount += effect.Amount;
            return discount;
        }

        /// <summary>True if any static aura (or the unit's own printed keyword) gives it Auto-Advance.</summary>
        public bool HasAutoAdvance(UnitState unit)
        {
            if (unit.Definition.AutoAdvance) return true;
            foreach (var source in Units)
            {
                foreach (var effect in source.AllEffects)
                {
                    if (effect.Trigger != Trigger.Static || effect.Action != EffectAction.StaticAutoAdvance) continue;
                    if (effect.Condition == EffectCondition.Frontline && source.Y != FrontlineRow(source.Owner)) continue;
                    if (effect.SpaceEffect != SpaceEffectType.None && SpaceEffects[source.X, source.Y] != effect.SpaceEffect) continue;
                    if (StaticScopeContains(source, effect.Scope, unit)) return true;
                }
            }
            return false;
        }

        public int EffectiveMaxLife(UnitState unit)
        {
            int life = unit.Definition.Life + unit.BonusLife + StaticContribution(unit, EffectAction.StaticStats, statsPower: false);
            return Math.Max(1, life);
        }

        public int CurrentLife(UnitState unit) => EffectiveMaxLife(unit) - unit.Damage;

        public int EffectiveArmor(UnitState unit) =>
            unit.Definition.Armor + unit.BonusArmor + StaticContribution(unit, EffectAction.StaticArmor, statsPower: false);

        private int StaticContribution(UnitState target, EffectAction action, bool statsPower)
        {
            int total = 0;

            foreach (var source in Units)
            {
                foreach (var effect in source.AllEffects)
                {
                    if (effect.Trigger != Trigger.Static || effect.Action != action) continue;
                    if (effect.Condition == EffectCondition.Frontline && source.Y != FrontlineRow(source.Owner)) continue;
                    if (effect.Condition == EffectCondition.WhileBonded && source.BoundSpiritCardId == null) continue;
                    //SpaceEffect doubles as a standing-on condition for static buffs (rules-v2)
                    if (effect.SpaceEffect != SpaceEffectType.None && SpaceEffects[source.X, source.Y] != effect.SpaceEffect) continue;
                    if (!StaticScopeContains(source, effect.Scope, target)) continue;

                    if (action == EffectAction.StaticArmor || action == EffectAction.StaticParry || action == EffectAction.StaticRegen) total += effect.Amount;
                    else total += statsPower ? effect.Power : effect.Life;
                }
            }

            return total;
        }

        private bool StaticScopeContains(UnitState source, TargetScope scope, UnitState target)
        {
            switch (scope)
            {
                case TargetScope.Self:
                    return source == target;
                case TargetScope.Adjacent:
                    //allies only: front, behind, and to the side
                    return source.Owner == target.Owner && source != target &&
                           Math.Abs(source.X - target.X) + Math.Abs(source.Y - target.Y) == 1;
                case TargetScope.RowInFront:
                    return source.Owner == target.Owner &&
                           target.Y == source.Y + ForwardDir(source.Owner);
                case TargetScope.Nearby:
                    //allies in the eight surrounding spaces
                    return source.Owner == target.Owner && source != target &&
                           Math.Abs(source.X - target.X) <= 1 && Math.Abs(source.Y - target.Y) <= 1;
                default:
                    return false;
            }
        }

        public GameState Clone()
        {
            var copy = new GameState
            {
                Players = new[] { Players[0].Clone(), Players[1].Clone() },
                Units = Units.ConvertAll(u => u.Clone()),
                Pending = new List<PendingAction>(Pending),
                SpaceEffects = (SpaceEffectType[,])SpaceEffects.Clone(),
                ActivePlayer = ActivePlayer,
                TurnNumber = TurnNumber,
                NextUnitId = NextUnitId,
                Seed = Seed,
                RngCalls = RngCalls,
                Winner = Winner,
                LastCardPlayed = LastCardPlayed,
                LastCombatAttackerId = LastCombatAttackerId,
                LastBrokenSpiritId = LastBrokenSpiritId
            };
            return copy;
        }
    }
}
