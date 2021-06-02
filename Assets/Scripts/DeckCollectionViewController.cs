using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeckCollectionViewController : MonoBehaviour
{
    public CardViewController cardViewPrefab;
    private CardLibrary library;
    private DeckItemView deckItemViewPrefab;
    
    public Transform deckCollectionPanel;
    public List<DeckItemView> deckViews;

    public Button addNewDeckButton;
    
    public CanvasGroupFader deckCollectionFader;

    private DeckEditorViewController deckEditor;
    private SaveDataManager saveManager;

    public void Initialize(CardLibrary library, CardViewController cardViewPrefab, DeckItemView deckItemViewPrefab, DeckEditorViewController deckEditor, SaveDataManager saveManager)
    {
        this.saveManager = saveManager;

        this.deckItemViewPrefab = deckItemViewPrefab;
        this.library = library;
        this.cardViewPrefab = cardViewPrefab;
        this.deckEditor = deckEditor;
        
        addNewDeckButton.onClick.AddListener(CreateDeck);
        
        InitializeDeckCollection();

        deckEditor.OnClose += ShowView;
    }

    private void InitializeDeckCollection()
    {
        deckViews = GetComponentsInChildren<DeckItemView>().ToList();
        
        for (int n = 0; n < deckViews.Count; n++)
        {
            Destroy(deckViews[n].gameObject);
        }
        
        deckViews.Clear();
        
        foreach (var deckData in library.starterDecks)
        {
            AddDeckView(deckData.deck);
        }
        
        foreach (var deck in library.Decks)
        {
            AddDeckView(deck);
        }
    }
    
    private void CreateDeck()
    {
        var deck = new Deck();
        deck.Initialize();
        
        library.Decks.Add(deck);

        AddDeckView(deck);
    }

    private void AddDeckView(Deck deck)
    {
        var deckView = Instantiate(deckItemViewPrefab, deckCollectionPanel);
        
        deckView.Initialize(library, deck);
        
        deckViews.Add(deckView);

        deckView.OnSelectButtonPressed += () => { OpenDeckEditor(deckView); };
        deckView.OnDeleteButtonPressed += () => { RemoveDeckView(deckView); };
        
        addNewDeckButton.transform.SetAsLastSibling();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(deckCollectionPanel as RectTransform);
    }

    private void RemoveDeckView(DeckItemView deckView)
    {
        deckViews.Remove(deckView);
        Destroy(deckView.gameObject);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(deckCollectionPanel as RectTransform);
    }
    
    private void ShowView()
    {
        deckCollectionFader.FadeIn();
    }
    
    private void OpenDeckEditor(DeckItemView deckView)
    {
        deckCollectionFader.FadeOut();
        deckEditor.Enter(deckView.deck);
    }

}
