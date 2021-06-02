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
        
        saveButton.onClick.AddListener(SaveChanges);
        closeButton.onClick.AddListener(CloseDeckEditor);
    }

    public void Enter(Deck selectedDeck)
    {
        gameObject.SetActive(true);
        viewFader.FadeIn();
        
        this.selectedDeck = selectedDeck;

        workingDeck = new Deck();
        workingDeck.SetCardList(selectedDeck.cards);
        workingDeck.Initialize();

        deckHeaderView.Initialize(library, workingDeck);
        
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
        selectedDeck.SetCardList(workingDeck.cards);

        saveManager.Save();
    }
    
    private void CloseDeckEditor()
    {
        viewFader.FadeOut();
        OnClose?.Invoke();
    }
}
