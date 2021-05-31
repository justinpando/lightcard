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
    public System.Action OnClose;
    
    public void Initialize(CardLibrary library, FilterCollectionViewController filters, 
        CardViewController cardViewPrefab, CardViewController deckCardViewPrefab)
    {
        this.library = library;
        this.filters.Initialize(library, filters.filterViewPrefab);
        this.filters.OnFiltersUpdated += HandleFiltersUpdated;
        
        this.cardViewPrefab = cardViewPrefab;
        this.deckCardViewPrefab = deckCardViewPrefab;
        
        saveButton.onClick.AddListener(SaveDeck);
        closeButton.onClick.AddListener(CloseDeckEditor);
    }

    public void Enter(DeckData selectedDeck)
    {
        gameObject.SetActive(true);
        viewFader.FadeIn();
        
        this.selectedDeck = selectedDeck;
        workingDeck = selectedDeck;
        workingDeck.Initialize();
        
        deckHeaderView.Initialize(library, selectedDeck);
        
        InitializeCollectionCards();
        InitializeDeckCards();
    }

    private void InitializeCollectionCards()
    {
        cardViews = cardViewCollectionPanel.GetComponentsInChildren<CardViewController>().ToList();
        
        for (int n = 0; n < cardViews.Count; n++)
        {
            Destroy(cardViews[n].gameObject);
        }
        
        cardViews.Clear();

        foreach (var card in library.cards)
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

    private void AddCardView(CardData cardData)
    {
        CardViewController view = Instantiate(cardViewPrefab, cardViewCollectionPanel);
        
        view.Initialize(cardData, library.classes.Find(x => x.group == cardData.group));
        
        view.selectButton.onClick.AddListener(() => AddCardToDeck(view.cardData));

        cardViews.Add(view);
    }

    private void HandleFiltersUpdated()
    {
        filters.FilterCardViews(cardViews);
    }

    private void AddCardToDeck(CardData cardData)
    {
        if(workingDeck.AddCard(cardData)) AddDeckCardView(cardData);
    }
    
    private void AddDeckCardView(CardData cardData)
    {
        CardViewController view = Instantiate(deckCardViewPrefab, deckCardsPanel);
        
        view.Initialize(cardData, library.classes.Find(x => x.group == cardData.group));
        
        view.selectButton.onClick.AddListener(() => RemoveCardFromDeck(view));
        
        deckCardViews.Add(view);
        
        view.transform.SetSiblingIndex(workingDeck.cards.IndexOf(cardData));
        
    }

    private void RemoveCardFromDeck(CardViewController cardView)
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
