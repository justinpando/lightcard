using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public enum AiStyle { Balanced, Formation, Patient, Relentless, Control, Attrition, Chaotic }
    public enum AiDeckSource { Starter, CatalogExpedition, CatalogGarden, CatalogAtelier, CatalogHeart, CatalogTower, CatalogOcean }

    public CardLibrary library;

    [Tooltip("Saved deck to play with; blank = first saved deck, falling back to a starter deck.")]
    public string playerDeckName = "";
    [Tooltip("Where the opponent's deck comes from: a starter deck asset, or built from the engine catalog.")]
    public AiDeckSource aiDeckSource = AiDeckSource.CatalogAtelier;
    [Tooltip("Which starter deck the opponent pilots when aiDeckSource is Starter.")]
    public int aiStarterDeckIndex = 0;
    public AiStyle aiStyle = AiStyle.Control;
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
    private bool activateSelectable;
    private bool aiRunning;
    private int suppressClickFrame = -1;

    private void Update()
    {
        //Central board-click detection: same raycast path as drag-and-drop.
        //Skipped right after a drop (same mouse-up) and when over UI.
        if (!Input.GetMouseButtonUp(0)) return;
        if (Time.frameCount <= suppressClickFrame + 1) return;
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        var space = RaycastSpace(Input.mousePosition, SpacePreference.Occupied);
        if (space != null) OnSpaceClicked(space);
    }

    private void Start()
    {
        //Deck chosen in the main menu overrides the inspector default
        if (!string.IsNullOrEmpty(MatchLaunch.DeckName))
            playerDeckName = MatchLaunch.DeckName;

        var saveManager = new SaveDataManager(library);
        var save = saveManager.Load();

        var playerDeck = BuildPlayerDeck(save);
        var aiDeck = BuildAiDeck(playerDeck);
        int matchSeed = seed != 0 ? seed : Random.Range(1, int.MaxValue);

        var events = new List<GameEvent>();
        state = GameEngine.CreateGame(playerDeck, aiDeck, matchSeed, events);
        aiAgent = new HeuristicAgent(AiPlayer, PersonalityFor(aiStyle));
        Debug.Log($"Match started: seed {matchSeed}, player deck {playerDeck.Count} cards, AI deck {aiDeck.Count} cards ({aiAgent.Personality.Name}).");

        fieldView = new FieldViewController(OnSpaceClicked, ResolveCardArt);

        var uiCanvas = GameObject.Find("UI Canvas");
        handView = new HandViewController(library, uiCanvas.transform.Find("Margin/Hand"));
        handView.OnCardClicked = OnHandCardClicked;
        handView.OnCardDragStart = OnCardDragStarted;
        handView.OnCardDropped = OnCardDropped;

        hud = new MatchHudController(uiCanvas.transform, library);
        hud.OnEndTurnClicked = OnEndTurnClicked;
        hud.OnReplaceClicked = OnReplaceClicked;
        hud.OnRematchClicked = () => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        hud.OnMenuClicked = () => SceneManager.LoadScene("Main");

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
        if (aiDeckSource != AiDeckSource.Starter)
        {
            var archetype = aiDeckSource == AiDeckSource.CatalogExpedition ? Archetype.Expedition
                          : aiDeckSource == AiDeckSource.CatalogGarden ? Archetype.Garden
                          : aiDeckSource == AiDeckSource.CatalogHeart ? Archetype.Heart
                          : aiDeckSource == AiDeckSource.CatalogTower ? Archetype.Tower
                          : aiDeckSource == AiDeckSource.CatalogOcean ? Archetype.Ocean
                          : Archetype.Atelier;
            var catalogDeck = new List<string>();
            foreach (var card in CardCatalogV1.Cards.Values)
                if (card.Archetype == archetype)
                    for (int copies = 0; copies < GameConfig.IndividualCardLimit; copies++)
                        catalogDeck.Add(card.Id);
            return catalogDeck;
        }

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

    private Sprite ResolveCardArt(string cardId)
    {
        var card = library.cardCollection.cards.FirstOrDefault(c => c != null && c.name == cardId);
        return card != null ? card.sprite : null;
    }

    private static AgentPersonality PersonalityFor(AiStyle style)
    {
        switch (style)
        {
            case AiStyle.Formation: return AgentPersonality.Formation();
            case AiStyle.Patient: return AgentPersonality.Patient();
            case AiStyle.Relentless: return AgentPersonality.Relentless();
            case AiStyle.Control: return AgentPersonality.Control();
            case AiStyle.Attrition: return AgentPersonality.Attrition();
            case AiStyle.Chaotic: return AgentPersonality.Chaotic();
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
        SelectHandCard(handIndex);
    }

    private void SelectHandCard(int handIndex)
    {
        var cardId = state.Players[LocalPlayer].Hand[handIndex];
        var card = CardCatalogV1.Get(cardId);
        var playerState = state.Players[LocalPlayer];
        int cost = state.EffectiveCost(LocalPlayer, card);
        bool playable = cost <= playerState.Energy &&
                        playerState.Affinity[card.Archetype] >= card.AffinityRequirement;
        if (cost > playerState.Energy)
        {
            hud.SetStatus($"Not enough energy for {cardId} ({card.Cost}). Drag it onto the energy dial to Attune it.");
        }
        else if (!playable)
        {
            hud.SetStatus($"{cardId} needs {card.Archetype} Affinity {card.AffinityRequirement} (you have {playerState.Affinity[card.Archetype]}). Attune {card.Archetype} cards to reach it.");
        }

        selectedHandIndex = handIndex;
        handView.SetSelected(handIndex);
        hud.ShowReplaceTarget(!playerState.ReplaceUsedThisTurn);

        if (playable)
        {
            playTargets = EnumeratePlayTargets(card).ToList();
            fieldView.HighlightSpaces(playTargets, SpaceView.Highlight.PlayTarget);
            hud.SetStatus(card.PlayTarget == PlayTargetKind.None
                ? $"Click {cardId} again to play it, or drag it to the board."
                : $"Drag {cardId} to a green space, or click one.");
        }
    }

    private bool DropCardPlayable(CardDefinition card)
    {
        return state.EffectiveCost(LocalPlayer, card) <= state.Players[LocalPlayer].Energy &&
               state.Players[LocalPlayer].Affinity[card.Archetype] >= card.AffinityRequirement;
    }

    private bool OnCardDragStarted(int handIndex)
    {
        if (!InputAllowed()) return false;
        ClearSelection();
        SelectHandCard(handIndex);
        return true;
    }

    private void OnCardDropped(int handIndex, Vector2 screenPosition)
    {
        suppressClickFrame = Time.frameCount;
        if (!InputAllowed() || selectedHandIndex != handIndex)
        {
            ClearSelection();
            return;
        }

        if (hud.IsOverAttuneZone(screenPosition))
        {
            Submit(new ReplaceCardCommand { Player = LocalPlayer, HandIndex = handIndex });
            return;
        }

        var dropCard = CardCatalogV1.Get(state.Players[LocalPlayer].Hand[handIndex]);
        var preference = dropCard.PlayTarget == PlayTargetKind.AnyUnit ? SpacePreference.Occupied
                       : dropCard.PlayTarget == PlayTargetKind.FriendlyEmptySpace ? SpacePreference.Empty
                       : SpacePreference.Nearest;
        var space = RaycastSpace(screenPosition, preference);
        if (space != null)
        {
            var card = dropCard;
            if (card.PlayTarget == PlayTargetKind.None && playTargets.Count == 0 && DropCardPlayable(card))
            {
                //Untargeted cards play when dropped anywhere on the board
                Submit(new PlayCardCommand { Player = LocalPlayer, HandIndex = handIndex, TargetX = 0, TargetY = 0 });
                return;
            }
            if (playTargets.Contains((space.X, space.Y)))
            {
                Submit(new PlayCardCommand { Player = LocalPlayer, HandIndex = handIndex, TargetX = space.X, TargetY = space.Y });
                return;
            }
        }

        ClearSelection();
    }

    private enum SpacePreference { Nearest, Occupied, Empty }

    private SpaceView RaycastSpace(Vector2 screenPosition, SpacePreference preference = SpacePreference.Nearest)
    {
        var camera = Camera.main;
        if (camera == null) return null;
        var ray = camera.ScreenPointToRay(screenPosition);

        if (preference == SpacePreference.Occupied)
        {
            //Clicking a unit: the tall click volumes ARE the unit silhouettes, so
            //raycast them, preferring the nearest occupied one (the visible art
            //belongs to it; an empty space's airspace is invisible).
            var hits = Physics.RaycastAll(ray, 100f);
            SpaceView nearest = null, occupied = null;
            float nearestDistance = float.MaxValue, occupiedDistance = float.MaxValue;
            foreach (var hit in hits)
            {
                var space = hit.collider.GetComponentInParent<SpaceView>();
                if (space == null) continue;
                if (hit.distance < nearestDistance) { nearestDistance = hit.distance; nearest = space; }
                if (space.UnitId >= 0 && hit.distance < occupiedDistance) { occupiedDistance = hit.distance; occupied = space; }
            }
            return occupied != null ? occupied : nearest;
        }

        //Dropping onto a space: project the ray onto the board plane and take
        //the space whose disc is under the pointer - unambiguous regardless of
        //how the tall volumes overlap on screen.
        var plane = new Plane(Vector3.up, Vector3.zero);
        float enter;
        if (!plane.Raycast(ray, out enter)) return null;
        var point = ray.GetPoint(enter);

        SpaceView best = null;
        float bestDistance = 0.45f; //roughly half a space of slack
        for (int x = 0; x < GameConfig.Lanes; x++)
        {
            for (int y = 0; y < GameConfig.Rows; y++)
            {
                var space = fieldView.GetSpace(x, y);
                if (space == null) continue;
                var offset = space.transform.position - point;
                float planar = new Vector2(offset.x, offset.z).magnitude;
                if (planar < bestDistance) { bestDistance = planar; best = space; }
            }
        }
        return best;
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
                if (activateSelectable)
                    Submit(new ActivateCommand { Player = LocalPlayer, UnitId = selectedUnitId });
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

        //Rules-v3: no manual attacks - the per-unit action is Activate
        activateSelectable = unit.Definition.ActivateCost >= 0 && !unit.ActivatedThisTurn &&
                             !unit.Asleep && !unit.Flux &&
                             playerState.Energy >= unit.Definition.ActivateCost;
        if (activateSelectable && space != null) space.SetHighlight(SpaceView.Highlight.Attack);

        shiftTargets.Clear();
        bool canShift = !unit.IsCharm && !unit.Asleep && !unit.Pinned &&
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

        hud.SetStatus(activateSelectable
            ? $"{unit.CardId}: click it again to Activate ({unit.Definition.ActivateCost} energy), or a blue space to Shift. Attacks happen automatically at end of turn."
            : $"{unit.CardId}: click a blue space to Shift. Attacks happen automatically at end of turn.");
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
                {
                    for (int y = 0; y < GameConfig.Rows; y++)
                    {
                        if (GameState.SideOfRow(y) != LocalPlayer) continue;
                        var occupant = state.GetUnitAt(x, y);
                        if (occupant == null || (occupant.Owner == LocalPlayer && occupant.Definition.IsEquip && definition.Type != CardType.Charm))
                            yield return (x, y);
                    }
                }
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
            case PlayTargetKind.FriendlyUnit:
                foreach (var unit in state.Units.Where(u => u.Owner == LocalPlayer && !u.IsCharm && !u.Definition.IsSpirit))
                    yield return (unit.X, unit.Y);
                break;
        }
    }

    private void ClearSelection()
    {
        selectedHandIndex = -1;
        selectedUnitId = -1;
        activateSelectable = false;
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
        hud.ShowEndPanel(state.Winner == LocalPlayer);
    }

    private void LogEvents(List<GameEvent> events)
    {
        foreach (var gameEvent in events)
        {
            Debug.Log($"[Match] {gameEvent}");
            string line = FormatEvent(gameEvent);
            if (line != null) hud?.AddHistoryLine(line);
        }
    }

    /// <summary>Player-facing history line for an event; null for internal noise.</summary>
    private string FormatEvent(GameEvent e)
    {
        string who = e.Player == LocalPlayer ? "You" : "Enemy";
        switch (e.Type)
        {
            case GameEventType.TurnStarted: return $"— Turn {e.Amount}: {(e.Player == LocalPlayer ? "your" : "enemy")} turn —";
            case GameEventType.CardPlayed: return $"{who} played {e.CardId}.";
            case GameEventType.CardReplaced: return $"{who} replaced {e.CardId} (+1 energy).";
            case GameEventType.UnitCalled: return $"{who} called {e.CardId}.";
            case GameEventType.AttackResolved: return $"{(e.Player == LocalPlayer ? "Your" : "Enemy")} {e.CardId} attacks.";
            case GameEventType.UnitDamaged: return $"{e.CardId} took {e.Amount} damage.";
            case GameEventType.UnitDestroyed: return $"{e.CardId} was destroyed.";
            case GameEventType.UnitHealed: return $"{e.CardId} healed {e.Amount}.";
            case GameEventType.UnitFellAsleep: return $"{e.CardId} fell asleep.";
            case GameEventType.UnitPinned: return $"{e.CardId} was pinned.";
            case GameEventType.UnitPoisoned: return $"{e.CardId} was poisoned ({e.Amount}).";
            case GameEventType.PlayerDamaged: return $"{(e.Player == LocalPlayer ? "You" : "Enemy")} took {e.Amount} damage.";
            case GameEventType.FatigueDamage: return $"{who} fatigued: empty deck!";
            case GameEventType.GameEnded: return e.Player == LocalPlayer ? "Victory!" : "Defeat.";
            default: return null;
        }
    }
}
