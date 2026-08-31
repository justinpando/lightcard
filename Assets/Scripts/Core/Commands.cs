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
        /// <summary>Second target space for two-target cards (FriendlyUnitThenEnemyUnit); -1 otherwise.</summary>
        public int Target2X = -1;
        public int Target2Y = -1;
    }

    /// <summary>The Shift power: spend 1 energy to move a unit one space. Shares the once-per-turn power action with Clear.</summary>
    public class ShiftCommand : Command
    {
        public int UnitId;
        public MoveDirection Direction;
    }

    /// <summary>The Clear power: spend 2 energy to remove a space effect. Shares the once-per-turn power action with Shift.</summary>
    public class ClearCommand : Command
    {
        public int X;
        public int Y;
    }

    /// <summary>
    /// Activate a unit's or charm's activatable ability (rules-v3: attacks are
    /// automatic at end of turn; activations are the manual per-unit action).
    /// </summary>
    public class ActivateCommand : Command
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
