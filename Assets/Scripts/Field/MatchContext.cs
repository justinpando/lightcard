using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LightCard.Core;
using LightCard.Core.Agents;

/// <summary>
/// Entry point of the Field scene (the phase-2 playable match): owns the
/// engine GameState, translates clicks into Commands, feeds resulting
/// GameEvents to the view layer, and runs the HeuristicAgent opponent.
/// The local player is always engine player 0 (the left half of the board).
/// </summary>
public class MatchContext : MonoBehaviour
{
    public enum AiStyle { Balanced, Formation, Patient, Relentless }

    public CardLibrary library;

    [Tooltip("Saved deck to play with; blank = first saved deck, falling back to a starter deck.")]
    public string playerDeckName = "";
    [Tooltip("Which starter deck the opponent pilots.")]
    public int aiStarterDeckIndex = 0;
    public AiStyle aiStyle = AiStyle.Balanced;
    [Tooltip("0 = random seed each match.")]
    public int seed;
    [Tooltip("Pause between AI actions so its turn is watchable.")]
    public float aiStepDelay = 0.5f;

    private const int LocalPlayer = 0;
    private const int AiPlayer = 1;
    private const int MinimumDeckSize = 10;

    private GameState state;
    private HeuristicAgent aiAgent;
    private FieldViewController fieldView;
    private HandViewController handView;
    private MatchHudController hud;

    private int selectedHandIndex = -1;
    private int selectedUnitId = -1;
    private List<(int x, int y)> playTargets = new List<(int x, int y)>();
    private List<(int x, int y)> shiftTargets = new List<(int x, int y)>();
    private bool attackSelectable;
    private bool aiRunning;

    private void Start()
    {
        var saveManager = new SaveDataManager(library);
        var save = saveManager.Load();

        var playerDeck = BuildPlayerDeck(save);
        var aiDeck = BuildAiDeck(playerDeck);
        int matchSeed = seed != 0 ? seed : Random.Range(1, int.MaxValue);

        var events = new List<GameEvent>();
        state = GameEngine.CreateGame(playerDeck, aiDeck, matchSeed, events);
        aiAgent = new HeuristicAgent(AiPlayer, PersonalityFor(aiStyle));
        Debug.Log($"Match started: seed {matchSeed}, player deck {playerDeck.Count} cards, AI deck {aiDeck.Count} cards ({aiAgent.Personality.Name}).");

        fieldView = new FieldViewController(OnSpaceClicked);

        var uiCanvas = GameObject.Find("UI Canvas");
        handView = new HandViewController(library, uiCanvas.transform.Find("Margin/Hand"));
        handView.OnCardClicked = OnHandCardClicked;

        hud = new MatchHudController(uiCanvas.transform);
        hud.OnEndTurnClicked = OnEndTurnClicked;
        hud.OnReplaceClicked = OnReplaceClicked;

        LogEvents(events);
        RefreshAll();
        hud.SetStatus("Your turn.");
    }

    //---- Deck setup ----

    private List<string> BuildPlayerDeck(SaveData save)
    {
        if (save?.decks != null && save.decks.Count > 0)
        {
            var chosen = !string.IsNullOrEmpty(playerDeckName)
                ? save.decks.FirstOrDefault(d => d.name == playerDeckName)
                : null;
            if (chosen == null) chosen = save.decks[0];

            var deck = FilterToCatalog(chosen.cards, chosen.name);
            if (deck.Count >= MinimumDeckSize) return deck;
            if (deck.Count > 0) return PadFromCatalog(deck, chosen.name);
            Debug.LogWarning($"Deck '{chosen.name}' has no engine-playable cards; using a starter deck instead.");
        }

        return PadFromCatalog(StarterDeck(0), "starter");
    }

    private List<string> BuildAiDeck(List<string> playerDeck)
    {
        var deck = StarterDeck(aiStarterDeckIndex);
        if (deck.Count == 0) return new List<string>(playerDeck);
        return PadFromCatalog(deck, "AI starter");
    }

    /// <summary>
    /// Until the full card set is in the engine catalog, decks lose their
    /// unimplemented cards on the way in. Top thin decks back up to a playable
    /// size with catalog cards, majority archetype first.
    /// </summary>
    private static List<string> PadFromCatalog(List<string> deck, string deckName)
    {
        const int targetSize = 30;
        if (deck.Count >= targetSize) return deck;

        var majority = deck
            .GroupBy(id => CardCatalogV1.Get(id).Archetype)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .DefaultIfEmpty(Archetype.Garden)
            .First();

        var fillOrder = CardCatalogV1.Cards.Values
            .OrderBy(c => c.Archetype == majority ? 0 : 1)
            .ThenBy(c => c.Cost)
            .ThenBy(c => c.Id);

        int added = 0;
        foreach (var card in fillOrder)
        {
            while (deck.Count < targetSize && deck.Count(id => id == card.Id) < GameConfig.IndividualCardLimit)
            {
                deck.Add(card.Id);
                added++;
            }
            if (deck.Count >= targetSize) break;
        }

        if (added > 0) Debug.Log($"Deck '{deckName}': padded with {added} {majority}-leaning catalog cards to {deck.Count}.");
        return deck;
    }

    private List<string> StarterDeck(int index)
    {
        var starters = library.starterDecks?.Where(s => s != null && s.deck != null).ToList();
        if (starters == null || starters.Count == 0)
        {
            Debug.LogError("No starter decks configured on the CardLibrary.");
            return new List<string>();
        }

        var starter = starters[Mathf.Clamp(index, 0, starters.Count - 1)];
        return FilterToCatalog(starter.deck.cards.Where(c => c != null).Select(c => c.name), starter.deck.name);
    }

    private static List<string> FilterToCatalog(IEnumerable<string> cardNames, string deckName)
    {
        var deck = new List<string>();
        foreach (var cardName in cardNames)
        {
            if (CardCatalogV1.Cards.ContainsKey(cardName)) deck.Add(cardName);
            else Debug.Log($"Deck '{deckName}': '{cardName}' is not in the v1 engine catalog yet, skipping.");
        }
        return deck;
    }

    private static AgentPersonality PersonalityFor(AiStyle style)
    {
        switch (style)
        {
            case AiStyle.Formation: return AgentPersonality.Formation();
            case AiStyle.Patient: return AgentPersonality.Patient();
            case AiStyle.Relentless: return AgentPersonality.Relentless();
            default: return AgentPersonality.Balanced();
        }
    }

    //---- Input ----

    private void OnHandCardClicked(int handIndex)
    {
        if (!InputAllowed()) return;

        if (selectedHandIndex == handIndex)
        {
            var definition = CardCatalogV1.Get(state.Players[LocalPlayer].Hand[handIndex]);
            if (definition.PlayTarget == PlayTargetKind.None)
            {
                //Untargeted cards play on the confirming second click
                Submit(new PlayCardCommand { Player = LocalPlayer, HandIndex = handIndex, TargetX = 0, TargetY = 0 });
            }
            else
            {
                ClearSelection();
            }
            return;
        }

        ClearSelection();

        var cardId = state.Players[LocalPlayer].Hand[handIndex];
        var card = CardCatalogV1.Get(cardId);
        if (card.Cost > state.Players[LocalPlayer].Energy)
        {
            hud.SetStatus($"Not enough energy for {cardId} ({card.Cost}). You can still Replace it.");
        }

        selectedHandIndex = handIndex;
        handView.SetSelected(handIndex);
        hud.ShowReplaceTarget(!state.Players[LocalPlayer].ReplaceUsedThisTurn);

        if (card.Cost <= state.Players[LocalPlayer].Energy)
        {
            playTargets = EnumeratePlayTargets(card).ToList();
            fieldView.HighlightSpaces(playTargets, SpaceView.Highlight.PlayTarget);
            hud.SetStatus(card.PlayTarget == PlayTargetKind.None
                ? $"Click {cardId} again to play it."
                : $"Choose a target for {cardId}.");
        }
    }

    private void OnSpaceClicked(SpaceView space)
    {
        if (!InputAllowed()) return;

        if (selectedHandIndex >= 0)
        {
            if (playTargets.Contains((space.X, space.Y)))
            {
                Submit(new PlayCardCommand { Player = LocalPlayer, HandIndex = selectedHandIndex, TargetX = space.X, TargetY = space.Y });
            }
            else
            {
                ClearSelection();
            }
            return;
        }

        if (selectedUnitId >= 0)
        {
            var selected = state.GetUnit(selectedUnitId);
            if (selected != null && space.X == selected.X && space.Y == selected.Y)
            {
                if (attackSelectable)
                    Submit(new AttackCommand { Player = LocalPlayer, UnitId = selectedUnitId });
                else
                    ClearSelection();
                return;
            }

            if (selected != null && shiftTargets.Contains((space.X, space.Y)))
            {
                Submit(new ShiftCommand
                {
                    Player = LocalPlayer,
                    UnitId = selectedUnitId,
                    Direction = DirectionTo(selected, space.X, space.Y)
                });
                return;
            }

            ClearSelection();
            //Fall through: clicking another of our units switches selection
        }

        var unit = state.GetUnitAt(space.X, space.Y);
        if (unit != null && unit.Owner == LocalPlayer) SelectUnit(unit);
    }

    private void OnEndTurnClicked()
    {
        if (!InputAllowed()) return;
        Submit(new EndTurnCommand { Player = LocalPlayer });
    }

    private void OnReplaceClicked()
    {
        if (!InputAllowed() || selectedHandIndex < 0) return;
        Submit(new ReplaceCardCommand { Player = LocalPlayer, HandIndex = selectedHandIndex });
    }

    private bool InputAllowed() => state != null && !state.IsOver && !aiRunning && state.ActivePlayer == LocalPlayer;

    private void SelectUnit(UnitState unit)
    {
        selectedUnitId = unit.Id;

        var playerState = state.Players[LocalPlayer];
        var space = fieldView.GetSpace(unit.X, unit.Y);
        if (space != null) space.SetHighlight(SpaceView.Highlight.Selected);

        //Mirror of the engine's attack legality, so the affordance is honest
        attackSelectable = !unit.IsCharm && !unit.Asleep && !unit.Flux &&
                           !unit.AttackedThisTurn && !unit.MovedThisTurn &&
                           state.EffectivePower(unit) > 0 &&
                           (unit.Definition.Ranged || !HasFriendlyUnitInFront(unit));
        if (attackSelectable && space != null) space.SetHighlight(SpaceView.Highlight.Attack);

        shiftTargets.Clear();
        bool canShift = !unit.IsCharm && !unit.Asleep &&
                        !playerState.ShiftUsedThisTurn && playerState.Energy >= GameConfig.ShiftEnergyCost;
        if (canShift)
        {
            foreach (var (dx, dy) in new[] { (0, 1), (0, -1), (-1, 0), (1, 0) })
            {
                int x = unit.X + dx, y = unit.Y + dy;
                if (!GameState.InBounds(x, y) || GameState.SideOfRow(y) != LocalPlayer) continue;
                var occupant = state.GetUnitAt(x, y);
                if (occupant != null && occupant.IsCharm) continue;
                shiftTargets.Add((x, y));
            }
            fieldView.HighlightSpaces(shiftTargets, SpaceView.Highlight.ShiftTarget);
        }

        hud.SetStatus(attackSelectable
            ? $"{unit.CardId}: click it again to attack, or a blue space to Shift."
            : $"{unit.CardId}: click a blue space to Shift.");
    }

    private bool HasFriendlyUnitInFront(UnitState unit)
    {
        int forward = GameState.ForwardDir(unit.Owner);
        for (int y = unit.Y + forward; GameState.InBounds(unit.X, y) && GameState.SideOfRow(y) == unit.Owner; y += forward)
        {
            var other = state.GetUnitAt(unit.X, y);
            if (other != null && other.Owner == unit.Owner) return true;
        }
        return false;
    }

    private static MoveDirection DirectionTo(UnitState unit, int x, int y)
    {
        int dy = y - unit.Y;
        if (dy == GameState.ForwardDir(unit.Owner)) return MoveDirection.Forward;
        if (dy == -GameState.ForwardDir(unit.Owner)) return MoveDirection.Back;
        return x < unit.X ? MoveDirection.Left : MoveDirection.Right;
    }

    private IEnumerable<(int x, int y)> EnumeratePlayTargets(CardDefinition definition)
    {
        switch (definition.PlayTarget)
        {
            case PlayTargetKind.FriendlyEmptySpace:
                for (int x = 0; x < GameConfig.Lanes; x++)
                    for (int y = 0; y < GameConfig.Rows; y++)
                        if (GameState.SideOfRow(y) == LocalPlayer && state.GetUnitAt(x, y) == null)
                            yield return (x, y);
                break;
            case PlayTargetKind.AnySpace:
                for (int x = 0; x < GameConfig.Lanes; x++)
                    for (int y = 0; y < GameConfig.Rows; y++)
                        yield return (x, y);
                break;
            case PlayTargetKind.AnyUnit:
                foreach (var unit in state.Units)
                    yield return (unit.X, unit.Y);
                break;
        }
    }

    private void ClearSelection()
    {
        selectedHandIndex = -1;
        selectedUnitId = -1;
        attackSelectable = false;
        playTargets.Clear();
        shiftTargets.Clear();
        fieldView.ClearHighlights();
        handView.SetSelected(-1);
        hud.ShowReplaceTarget(false);
        if (state != null && !state.IsOver) hud.SetStatus(state.ActivePlayer == LocalPlayer ? "Your turn." : "Enemy turn...");
    }

    //---- Command execution ----

    private void Submit(Command command)
    {
        var result = GameEngine.Execute(state, command);
        if (!result.Success)
        {
            hud.SetStatus(result.Error);
            ClearSelection();
            return;
        }

        LogEvents(result.Events);
        ClearSelection();
        RefreshAll();

        if (state.IsOver)
        {
            OnGameOver();
        }
        else if (state.ActivePlayer == AiPlayer)
        {
            StartCoroutine(RunAiTurn());
        }
    }

    private IEnumerator RunAiTurn()
    {
        aiRunning = true;
        hud.SetEndTurnEnabled(false);
        hud.SetStatus("Enemy turn...");

        int guard = 0;
        while (!state.IsOver && state.ActivePlayer == AiPlayer && guard++ < 200)
        {
            yield return new WaitForSeconds(aiStepDelay);

            var command = aiAgent.ChooseCommand(state);
            var result = GameEngine.Execute(state, command);
            if (!result.Success)
            {
                //Same fail-safe as MatchRunner: an illegal proposal ends the AI turn
                Debug.LogWarning($"AI proposed an illegal command ({result.Error}); ending its turn.");
                result = GameEngine.Execute(state, new EndTurnCommand { Player = AiPlayer });
                if (!result.Success) break;
            }

            LogEvents(result.Events);
            RefreshAll();
        }

        aiRunning = false;

        if (state.IsOver)
        {
            OnGameOver();
        }
        else
        {
            hud.SetEndTurnEnabled(true);
            hud.SetStatus("Your turn.");
        }
    }

    private void RefreshAll()
    {
        fieldView.Refresh(state, LocalPlayer);
        handView.Render(state.Players[LocalPlayer].Hand);
        hud.Refresh(state, LocalPlayer);
        hud.SetEndTurnEnabled(InputAllowed());
    }

    private void OnGameOver()
    {
        hud.SetEndTurnEnabled(false);
        hud.SetEndTurnLabel(state.Winner == LocalPlayer ? "Victory!" : "Defeat");
        hud.SetStatus(state.Winner == LocalPlayer
            ? "Victory! The enemy's light fades."
            : "Defeat. Your light fades...");
    }

    private static void LogEvents(List<GameEvent> events)
    {
        foreach (var gameEvent in events)
            Debug.Log($"[Match] {gameEvent}");
    }
}
