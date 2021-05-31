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
    
    public void Initialize(CardLibrary library, CardViewController cardViewPrefab, DeckItemView deckItemViewPrefab, DeckEditorViewController deckEditor)
    {
        
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
