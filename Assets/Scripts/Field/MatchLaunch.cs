/// <summary>
/// Carries match settings across the Main -> Field scene load. Statics survive
/// scene transitions within a session; scene-placed MatchContext inspector
/// values act as the fallback when the Field scene is entered directly.
/// </summary>
public static class MatchLaunch
{
    /// <summary>Deck the player chose in the menu; null/empty = MatchContext's own default.</summary>
    public static string DeckName;
}
