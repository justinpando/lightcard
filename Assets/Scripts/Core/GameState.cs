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
        public bool Asleep;
        /// <summary>True on the turn a unit is called; units in Flux may not attack.</summary>
        public bool Flux;
        public bool AttackedThisTurn;
        public bool MovedThisTurn;

        public CardDefinition Definition => CardCatalogV1.Get(CardId);
        public bool IsCharm => Definition.Type == CardType.Charm;

        public UnitState Clone() => (UnitState)MemberwiseClone();
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
        public bool ShiftUsedThisTurn;

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

    public class GameState
    {
        public PlayerState[] Players = { new PlayerState(), new PlayerState() };
        public List<UnitState> Units = new List<UnitState>();
        public SpaceEffectType[,] SpaceEffects = new SpaceEffectType[GameConfig.Lanes, GameConfig.Rows];
        public int ActivePlayer;
        public int TurnNumber;
        public int NextUnitId = 1;
        public int Seed;
        public int RngCalls;
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
            int power = unit.Definition.Power + unit.BonusPower + StaticContribution(unit, EffectAction.StaticStats, statsPower: true);
            return Math.Max(0, power);
        }

        public int EffectiveMaxLife(UnitState unit)
        {
            int life = unit.Definition.Life + unit.BonusLife + StaticContribution(unit, EffectAction.StaticStats, statsPower: false);
            return Math.Max(1, life);
        }

        public int CurrentLife(UnitState unit) => EffectiveMaxLife(unit) - unit.Damage;

        public int EffectiveArmor(UnitState unit) =>
            unit.Definition.Armor + StaticContribution(unit, EffectAction.StaticArmor, statsPower: false);

        private int StaticContribution(UnitState target, EffectAction action, bool statsPower)
        {
            int total = 0;

            foreach (var source in Units)
            {
                foreach (var effect in source.Definition.Effects)
                {
                    if (effect.Trigger != Trigger.Static || effect.Action != action) continue;
                    if (effect.Condition == EffectCondition.Frontline && source.Y != FrontlineRow(source.Owner)) continue;
                    if (!StaticScopeContains(source, effect.Scope, target)) continue;

                    if (action == EffectAction.StaticArmor) total += effect.Amount;
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
                SpaceEffects = (SpaceEffectType[,])SpaceEffects.Clone(),
                ActivePlayer = ActivePlayer,
                TurnNumber = TurnNumber,
                NextUnitId = NextUnitId,
                Seed = Seed,
                RngCalls = RngCalls,
                Winner = Winner
            };
            return copy;
        }
    }
}
