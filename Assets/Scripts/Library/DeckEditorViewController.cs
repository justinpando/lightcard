using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditorViewController : MonoBehaviour
{
    public CanvasGroupFader viewFader;
    public Deck selectedDeck;
    private Deck workingDeck;
    
    //Deck View
    public DeckItemView deckHeaderView;
    public RectTransform deckCardsPanel;
    private CardViewController deckCardViewPrefab;
    private List<CardViewController> deckCardViews = new List<CardViewController>();
    public Scrollbar deckScrollBar;
    
    //Collection View
    private CardLibrary library;
    private CardViewController cardViewPrefab;
    private List<CardViewController> cardViews = new List<CardViewController>();
    public Scrollbar libraryScrollBar;
    public RectTransform cardViewCollectionPanel;
    
    public FilterCollectionViewController filters;

    public Button saveButton;
    public Button closeButton;
    /// <summary>Cycles the deck's player power (Shift/Clear). Cloned from saveButton at runtime when not wired in the scene.</summary>
    public Button powerButton;
    public System.Action OnClose;

    private SaveDataManager saveManager;
    
    public void Initialize(CardLibrary library, FilterCollectionViewController filters, 
        CardViewController cardViewPrefab, CardViewController deckCardViewPrefab, SaveDataManager saveManager)
    {
        this.library = library;
        this.filters.Initialize(library, filters.filterViewPrefab);
        this.filters.OnFiltersUpdated += HandleFiltersUpdated;
        
        this.cardViewPrefab = cardViewPrefab;
        this.deckCardViewPrefab = deckCardViewPrefab;

        this.saveManager = saveManager;
        
        workingDeck = new Deck ();

        EnsurePowerButton();

        saveButton.onClick.AddListener(SaveChanges);
        closeButton.onClick.AddListener(CloseDeckEditor);
        
        deckHeaderView.Initialize(library, workingDeck);
        deckHeaderView.nameInputCanvasGroup.interactable = true;
        deckHeaderView.nameInputCanvasGroup.blocksRaycasts = true;
        deckHeaderView.nameInputField.readOnly = false;
        
        InitializeCollectionCards();
    }

    public void Enter(Deck selectedDeck)
    {
        gameObject.SetActive(true);
        viewFader.FadeIn();
        
        this.selectedDeck = selectedDeck;

        workingDeck.name = selectedDeck.name;
        workingDeck.SetCardList(selectedDeck.cards);
        workingDeck.power = string.IsNullOrEmpty(selectedDeck.power) ? "Shift" : selectedDeck.power;
        RefreshPowerLabel();

        libraryScrollBar.value = 1f;
        InitializeDeckCards();
    }

    /// <summary>
    /// The rules-v3 loadout picker: a deck brings Shift OR Clear for the whole
    /// match. Built by cloning the save button so it inherits the scene's styling.
    /// </summary>
    private void EnsurePowerButton()
    {
        if (powerButton == null)
        {
            powerButton = Instantiate(saveButton, saveButton.transform.parent);
            powerButton.name = "PowerButton";

            var rect = (RectTransform)powerButton.transform;
            var saveRect = (RectTransform)saveButton.transform;
            rect.SetSiblingIndex(saveButton.transform.GetSiblingIndex());
            //Without a layout group driving the panel, sit just below the save button
            if (saveButton.transform.parent.GetComponent<UnityEngine.UI.LayoutGroup>() == null)
                rect.anchoredPosition = saveRect.anchoredPosition - new Vector2(0f, saveRect.rect.height + 8f);
        }

        //Drop any listeners cloned from the save button before wiring our own
        powerButton.onClick = new Button.ButtonClickedEvent();
        powerButton.onClick.AddListener(TogglePower);
        RefreshPowerLabel();
    }

    private void TogglePower()
    {
        workingDeck.power = workingDeck.power == "Clear" ? "Shift" : "Clear";
        RefreshPowerLabel();
    }

    private void RefreshPowerLabel()
    {
        if (powerButton == null) return;
        string label = $"Power: {workingDeck.power}";
        var legacyText = powerButton.GetComponentInChildren<Text>();
        if (legacyText != null) { legacyText.text = label; return; }
        var tmpText = powerButton.GetComponentInChildren<TMPro.TMP_Text>();
        if (tmpText != null) tmpText.text = label;
    }

    private void InitializeCollectionCards()
    {
        cardViews = cardViewCollectionPanel.GetComponentsInChildren<CardViewController>().ToList();
        
        for (int n = 0; n < cardViews.Count; n++)
        {
            Destroy(cardViews[n].gameObject);
        }
        
        cardViews.Clear();

        foreach (var card in library.cardCollection.cards)
        {
            AddCardView(card);
        }
        
        libraryScrollBar.value = 1f;
    }
    
    private void InitializeDeckCards()
    {
        deckCardViews = deckCardsPanel.GetComponentsInChildren<CardViewController>().ToList();
        
        for (int n = 0; n < deckCardViews.Count; n++)
        {
            Destroy(deckCardViews[n].gameObject);
        }
        
        deckCardViews.Clear();

        foreach (var card in workingDeck.cards)
        {
            AddDeckCardView(card);
        }

        deckScrollBar.value = 1f;
    }

    private void AddCardView(Card card)
    {
        CardViewController view = Instantiate(cardViewPrefab, cardViewCollectionPanel);
        
        view.Initialize(card, library.classes.Find(x => x.archetype == card.archetype));
        
        view.selectButton.onClick.AddListener(() => AddCardToDeck(view.Card));

        cardViews.Add(view);
    }

    private void HandleFiltersUpdated()
    {
        filters.FilterCardViews(cardViews);
    }

    private void AddCardToDeck(Card card)
    {
        if(workingDeck.AddCard(card)) AddDeckCardView(card);
    }
    
    private void AddDeckCardView(Card card)
    {
        CardViewController view = Instantiate(deckCardViewPrefab, deckCardsPanel);
        
        view.Initialize(card, library.classes.Find(x => x.archetype == card.archetype));
        
        view.selectButton.onClick.AddListener(() => RemoveCardFromDeck(view));
        
        deckCardViews.Add(view);
        
        view.transform.SetSiblingIndex(workingDeck.cards.IndexOf(card));
    }

    private void RemoveCardFromDeck(CardViewController cardView)
    {
        workingDeck.RemoveCard(cardView.Card);
        
        Destroy(cardView.gameObject);
        
        deckCardViews.Remove(cardView);
    }
    
    private void SaveChanges()
    {
        selectedDeck.name = workingDeck.name;
        selectedDeck.SetCardList(workingDeck.cards);
        selectedDeck.power = workingDeck.power;

        saveManager.Save();
    }
    
    private void CloseDeckEditor()
    {
        viewFader.FadeOut();
        OnClose?.Invoke();
    }
}
