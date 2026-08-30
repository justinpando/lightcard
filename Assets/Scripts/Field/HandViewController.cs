using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using LightCard.Core;

/// <summary>
/// Binds the local player's hand (engine card ids) onto the CardViewController
/// prefab instances placed under the Field scene's Hand container, reusing the
/// deck builder's card visuals. Engine card ids match library Card names.
/// </summary>
public class HandViewController
{
    private readonly CardLibrary library;
    private readonly Transform handRoot;
    private readonly List<CardViewController> views;
    private readonly Dictionary<string, Card> runtimeCards = new Dictionary<string, Card>();
    private int selectedIndex = -1;
    private int hoveredIndex = -1;

    public Action<int> OnCardClicked;
    /// <summary>Return false to refuse the drag (e.g. not the player's turn).</summary>
    public Func<int, bool> OnCardDragStart;
    public Action<int, Vector2> OnCardDropped;

    public HandViewController(CardLibrary library, Transform handRoot)
    {
        this.library = library;
        this.handRoot = handRoot;
        views = handRoot.GetComponentsInChildren<CardViewController>(true).ToList();
    }

    public void Render(List<string> hand)
    {
        while (views.Count < hand.Count)
            views.Add(UnityEngine.Object.Instantiate(views[0], views[0].transform.parent));

        for (int i = 0; i < views.Count; i++)
        {
            var view = views[i];
            bool inHand = i < hand.Count;
            view.gameObject.SetActive(inHand);
            if (!inHand) continue;

            var card = FindCard(hand[i]);
            view.Initialize(card, ArchetypeDataFor(card.archetype));

            int index = i;
            view.selectButton.onClick.RemoveAllListeners();
            view.selectButton.onClick.AddListener(() => OnCardClicked?.Invoke(index));

            ConfigureInteraction(view, index);
        }

        hoveredIndex = -1;
        SetSelected(-1);
    }

    private void ConfigureInteraction(CardViewController view, int index)
    {
        //Per-card canvas lets a hovered card sort above its overlapping neighbors
        //without touching sibling order (which would move its grid slot)
        var sortCanvas = view.GetComponent<Canvas>();
        if (sortCanvas == null)
        {
            sortCanvas = view.gameObject.AddComponent<Canvas>();
            view.gameObject.AddComponent<GraphicRaycaster>();
        }
        sortCanvas.overrideSorting = false;

        var interaction = view.GetComponent<HandCardInteraction>();
        if (interaction == null) interaction = view.gameObject.AddComponent<HandCardInteraction>();
        interaction.Index = index;
        interaction.OnHoverChanged = HandleHoverChanged;
        interaction.OnDragStart = i => OnCardDragStart == null || OnCardDragStart(i);
        interaction.OnDropped = (i, screenPos) => OnCardDropped?.Invoke(i, screenPos);
    }

    private void HandleHoverChanged(int index, bool hovered)
    {
        hoveredIndex = hovered ? index : (hoveredIndex == index ? -1 : hoveredIndex);
        ApplyCardVisuals();
    }

    public void SetSelected(int index)
    {
        selectedIndex = index;
        ApplyCardVisuals();
    }

    private void ApplyCardVisuals()
    {
        for (int i = 0; i < views.Count; i++)
        {
            bool hovered = i == hoveredIndex;
            bool selected = i == selectedIndex;
            views[i].transform.localScale = hovered ? Vector3.one * 1.45f
                                          : selected ? Vector3.one * 1.1f
                                          : Vector3.one;
            var sortCanvas = views[i].GetComponent<Canvas>();
            if (sortCanvas != null)
            {
                sortCanvas.overrideSorting = hovered || selected;
                sortCanvas.sortingOrder = hovered ? 220 : selected ? 210 : 0;
            }
        }
    }

    private Card FindCard(string cardId)
    {
        var card = library.cardCollection.cards.FirstOrDefault(c => c != null && c.name == cardId);
        if (card != null) return card;

        //No library asset for this catalog id: build a display-only stand-in
        if (runtimeCards.TryGetValue(cardId, out var cached)) return cached;

        var definition = CardCatalogV1.Get(cardId);
        var standIn = ScriptableObject.CreateInstance<Card>();
        standIn.name = definition.Id;
        standIn.archetype = (Card.Archetype)(int)definition.Archetype;
        standIn.type = (Card.Type)(int)definition.Type;
        standIn.cost = definition.Cost;
        standIn.power = definition.Power;
        standIn.life = definition.Life;
        standIn.description = definition.Text;

        runtimeCards[cardId] = standIn;
        return standIn;
    }

    private ArchetypeData ArchetypeDataFor(Card.Archetype archetype)
    {
        var data = library.classes.FirstOrDefault(c => c != null && c.archetype == archetype);
        return data != null ? data : library.classes.First(c => c != null);
    }
}
