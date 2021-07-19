using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

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

    public ReorderableList mainReorderableList;
    public ReorderableList deleteDropArea;
    
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

        mainReorderableList.OnElementRemoved.AddListener(HandleElementDragged);
        mainReorderableList.OnElementDropped.AddListener(HandleElementDropped);

        deleteDropArea.OnElementAdded.AddListener(HandleElementDroppedToDelete);
    }

    private void HandleElementDragged(ReorderableList.ReorderableListEventStruct item)
    {
        DeckItemView view = item.SourceObject.GetComponent<DeckItemView>();
        
        Debug.Log($"Dragged deck: {view.deck.name}");
    }

    private void HandleElementDropped(ReorderableList.ReorderableListEventStruct item)
    {
        DeckItemView view = item.DroppedObject.GetComponent<DeckItemView>();
        Debug.Log($"Dropped deck: {view.deck.name} at index {item.ToIndex}");

        deckViews.Remove(view);
        deckViews.Insert(item.ToIndex, view);
        
        library.SortDecks(deckViews);
        saveManager.Save();
    }
    
    private void HandleElementDroppedToDelete(ReorderableList.ReorderableListEventStruct item)
    {
        DeckItemView view = item.SourceObject.GetComponent<DeckItemView>();
        
        Debug.Log($"Dropped deck in delete area: {view.deck.name}");
        
        DeleteDeck(view);
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

        library.Decks.Add(deck);

        AddDeckView(deck);
    }
    
    private void CopyDeck(DeckItemView deckView)
    {
        Debug.Log($"Copying deck: {deckView.name}");
        
        var deck = new Deck {name = deckView.deck.name};
        deck.SetCardList(deckView.deck.cards);

        library.Decks.Add(deck);

        AddDeckView(deck);
    }

    private void AddDeckView(Deck deck)
    {
        var deckView = Instantiate(deckItemViewPrefab, deckCollectionPanel);
        
        deckView.Initialize(library, deck);
        
        deckViews.Add(deckView);

        deckView.OnSelectButtonPressed += () => { OpenDeckEditor(deckView); };
        deckView.OnDeleteButtonPressed += () => { DeleteDeck(deckView); };
        deckView.OnCopyButtonPressed += () => { CopyDeck(deckView); };
        
        addNewDeckButton.transform.SetAsLastSibling();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(deckCollectionPanel as RectTransform);
    }

    private void DeleteDeck(DeckItemView deckView)
    {
        Debug.Log($"Deleting deck: {deckView.name}");
        
        deckViews.Remove(deckView);
        Destroy(deckView.gameObject);

        library.Decks.Remove(deckView.deck);

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
