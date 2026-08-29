namespace LightCard.Core
{
    public enum GameEventType
    {
        GameStarted,
        TurnStarted,
        TurnEnded,
        CardDrawn,
        CardPlayed,
        CardReplaced,
        EnergyChanged,
        AffinityGained,
        UnitCalled,
        UnitMoved,
        UnitDamaged,
        UnitHealed,
        UnitDestroyed,
        UnitStatsChanged,
        UnitFellAsleep,
        UnitWoke,
        AttackResolved,
        SpaceEffectApplied,
        PlayerDamaged,
        GameEnded
    }

    /// <summary>
    /// One thing that happened during resolution, in order. The Unity view layer
    /// consumes these to animate; the engine never renders anything itself.
    /// </summary>
    public class GameEvent
    {
        public GameEventType Type;
        public int Player = -1;
        public int UnitId = -1;
        public string CardId;
        public int X = -1;
        public int Y = -1;
        public int ToX = -1;
        public int ToY = -1;
        public int Amount;
        public SpaceEffectType SpaceEffect = SpaceEffectType.None;

        public override string ToString()
        {
            string s = Type.ToString();
            if (CardId != null) s += $" {CardId}";
            if (UnitId >= 0) s += $" unit:{UnitId}";
            if (Player >= 0) s += $" p{Player}";
            if (X >= 0) s += $" ({X},{Y})";
            if (ToX >= 0) s += $"->({ToX},{ToY})";
            if (Amount != 0) s += $" amount:{Amount}";
            if (SpaceEffect != SpaceEffectType.None) s += $" {SpaceEffect}";
            return s;
        }
    }
}
