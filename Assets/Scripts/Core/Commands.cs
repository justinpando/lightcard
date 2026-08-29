using System.Collections.Generic;

namespace LightCard.Core
{
    public enum MoveDirection { Forward, Back, Left, Right }

    /// <summary>A player intent. The engine validates and resolves commands into events.</summary>
    public abstract class Command
    {
        public int Player;
    }

    public class PlayCardCommand : Command
    {
        /// <summary>Index into the player's hand.</summary>
        public int HandIndex;
        /// <summary>Target space; ignored for PlayTargetKind.None.</summary>
        public int TargetX;
        public int TargetY;
    }

    /// <summary>The Shift power: once per turn, spend 1 energy to move a unit one space.</summary>
    public class ShiftCommand : Command
    {
        public int UnitId;
        public MoveDirection Direction;
    }

    public class AttackCommand : Command
    {
        public int UnitId;
    }

    /// <summary>
    /// Replace: once per turn, discard a card to permanently gain 1 max energy
    /// and 1 Affinity of the discarded card's archetype. This is the ramp economy
    /// from the design sheet.
    /// </summary>
    public class ReplaceCardCommand : Command
    {
        public int HandIndex;
    }

    public class EndTurnCommand : Command
    {
    }

    public class CommandResult
    {
        public bool Success;
        public string Error;
        public List<GameEvent> Events = new List<GameEvent>();

        public static CommandResult Fail(string error) =>
            new CommandResult { Success = false, Error = error };
    }
}
