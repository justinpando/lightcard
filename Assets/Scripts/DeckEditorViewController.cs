using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditorViewController : MonoBehaviour
{
    public CanvasGroupFader viewFader;
    public DeckData selectedDeck;
    private DeckData workingDeck;
    
    //Deck View
    public DeckItemView deckHeaderView;
    public RectTransform deckCardsPanel;
    private CardViewController deckCardViewPrefab;
    private List<CardViewController> deckCardViews = new List<CardViewController>();
    private Scrollbar deckScrollBar;
    
    //Collection View
    private CardLibrary library;
    private CardViewController cardViewPrefab;
    private List<CardViewController> libraryCardViews = new List<CardViewController>();
    private Scrollbar libraryScrollBar;
    private RectTransform cardViewCollectionPanel;
    
    public FilterCollectionViewController filters;

    public Button saveButton;
    public Button closeButton;
    public System.Action OnClose;
    
    public void Initialize(CardLibrary library, FilterCollectionViewController filters, 
        CardViewController cardViewPrefab, CardViewController deckCardViewPrefab)
    {
        this.library = library;
        this.filters.Initialize(library, filters.filterViewPrefab);
        
        this.cardViewPrefab = cardViewPrefab;
        this.deckCardViewPrefab = deckCardViewPrefab;
        
        saveButton.onClick.AddListener(SaveDeck);
        closeButton.onClick.AddListener(CloseDeckEditor);
        
        filters.OnFiltersUpdated += HandleFiltersUpdated;
    }

    public void Enter(DeckData selectedDeck)
    {
        this.selectedDeck = selectedDeck;
        workingDeck = selectedDeck;
        
        deckHeaderView.Initialize(library, selectedDeck);
        
        InitializeCards();
    }

    private void InitializeCards()
    {
        libraryCardViews = GetComponentsInChildren<CardViewController>().ToList();
        
        for (int n = 0; n < libraryCardViews.Count; n++)
        {
            Destroy(libraryCardViews[n].gameObject);
        }
        
        libraryCardViews.Clear();
        
        library.cards = library.cards.OrderBy(x => x.@group).ToList();
        
        foreach (var card in library.cards)
        {
            AddCardView(card);
        }

        deckScrollBar.value = 1f;
        libraryScrollBar.value = 1f;
    }

    private void AddCardView(CardData cardData)
    {
        CardViewController view = Instantiate(cardViewPrefab, cardViewCollectionPanel);
        
        view.Initialize(cardData, library.classes.Find(x => x.group == cardData.group));
        
        view.selectButton.onClick.AddListener(() => AddCardToDeck(view.cardData));
        
        libraryCardViews.Add(view);
    }

    private void HandleFiltersUpdated()
    {
        filters.FilterCardViews(libraryCardViews);
    }
    
    public void AddCardToDeck(CardData cardData)
    {
        workingDeck.AddCard(cardData);
        
        CardViewController view = Instantiate(deckCardViewPrefab, deckCardsPanel);
        
        view.Initialize(cardData, library.classes.Find(x => x.group == cardData.group));
        
        view.selectButton.onClick.AddListener(() => RemoveCardFromDeck(view));
        
        deckCardViews.Add(view);
    }

    public void RemoveCardFromDeck(CardViewController cardView)
    {
        workingDeck.RemoveCard(cardView.cardData);
        
        Destroy(cardView.gameObject);
        
        deckCardViews.Remove(cardView);
    }
    
    private void SaveDeck()
    {
        selectedDeck = workingDeck;
    }
    
    private void CloseDeckEditor()
    {
        viewFader.FadeOut();
        //gameObject.SetActive(false);
        
        OnClose?.Invoke();
    }
}
