using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditorViewController : MonoBehaviour
{
    public CardViewController cardViewPrefab;
    private CardLibrary library;
    private DeckItemView deckItemViewPrefab;
    private FilterCollectionViewController filterCollectionViewController;

    public Transform deckCollectionPanel;
    public List<DeckItemView> deckViews;

    public Button AddNewDeckButton;
    public Button CloseEditorButton;
    
    public CanvasGroupFader deckCollectionFader;
    public CanvasGroupFader deckEditorFader;

    private DeckItemView selectedDeckView;
    
    public void Initialize(CardLibrary library, CardViewController cardViewPrefab, DeckItemView deckItemViewPrefab,
        FilterCollectionViewController filterCollectionViewController)
    {
        this.filterCollectionViewController = filterCollectionViewController;
        this.deckItemViewPrefab = deckItemViewPrefab;
        this.library = library;
        this.cardViewPrefab = cardViewPrefab;
        
        AddNewDeckButton.onClick.AddListener(CreateDeck);
        CloseEditorButton.onClick.AddListener(CloseDeckEditor);
        
        InitializeDeckCollection();
        
        deckCollectionFader.FadeOut();
        deckEditorFader.FadeIn();
    }

    private void InitializeDeckCollection()
    {
        deckViews = GetComponentsInChildren<DeckItemView>().ToList();
        
        for (int n = 0; n < deckViews.Count; n++)
        {
            Destroy(deckViews[n].gameObject);
        }
        
        deckViews.Clear();
        
        foreach (var deck in library.decks)
        {
            AddDeckView(deck);
        }
    }
    
    private void CreateDeck()
    {
        var deck = ScriptableObject.CreateInstance<DeckData>();
        
        library.decks.Add(deck);
        
        AddDeckView(deck);
    }

    private void AddDeckView(DeckData deck)
    {
        var deckView = Instantiate(deckItemViewPrefab, deckCollectionPanel);
        
        deckView.Initialize(library, deck);
        
        deckViews.Add(deckView);

        deckView.OnSelectButtonPressed += () => { OpenDeckEditor(deckView); };
        deckView.OnDeleteButtonPressed += () => { RemoveDeckView(deckView); };
        
        AddNewDeckButton.transform.SetAsLastSibling();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(deckCollectionPanel as RectTransform);
    }

    private void RemoveDeckView(DeckItemView deckView)
    {
        deckViews.Remove(deckView);
        Destroy(deckView.gameObject);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(deckCollectionPanel as RectTransform);
    }
    
    private void OpenDeckEditor(DeckItemView deckView)
    {
        selectedDeckView = deckView;
        
        deckCollectionFader.FadeOut();
        deckEditorFader.FadeIn();
    }
    
    private void CloseDeckEditor()
    {
        deckCollectionFader.FadeIn();
        deckEditorFader.FadeOut();
    }
    
}
