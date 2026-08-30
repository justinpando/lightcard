namespace LightCard.Core
{
    /// <summary>
    /// Global rule constants. The board is 3 lanes (x: 0..2) by 6 rows (y: 0..5).
    /// Player 0 owns rows 0..2 (row 2 is their frontline), player 1 owns rows 3..5
    /// (row 3 is their frontline). "Forward" for player 0 is +y, for player 1 is -y.
    /// </summary>
    public static class GameConfig
    {
        public const int Lanes = 3;
        public const int Rows = 6;
        public const int RowsPerSide = 3;

        public const int StartingLife = 20;
        public const int StartingHandSize = 3;
        /// <summary>Extra opening cards for the player going second (rules-v2).</summary>
        public const int SecondPlayerBonusCards = 1;
        public const int CardsDrawnPerTurn = 2;
        public const int MaxHandSize = 10;

        //Rules-v2: there is no automatic energy gain — Replace is the only ramp.
        //Shift and Clear share the once-per-turn player power action (rules-v3).
        public const int ShiftEnergyCost = 1;
        public const int ClearEnergyCost = 2;

        public const int DeckCardLimit = 40;
        public const int IndividualCardLimit = 3;
    }
}
