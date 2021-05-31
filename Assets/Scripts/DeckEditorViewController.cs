using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditorViewController : MonoBehaviour
{
    public DeckItemView deckView;
    public RectTransform deckCardsPanel;

    public FilterCollectionViewController filters;

    public CardViewController deckCardView;
    private CardLibrary library;

    public Button closeEditorButton;
    
    public CanvasGroupFader viewFader;

    public System.Action OnClose;
    private CardViewController cardViewPrefab;
    private CardViewController deckCardViewPrefab;

    public void Initialize(CardLibrary library, FilterCollectionViewController filterCollectionViewController, 
        CardViewController cardViewPrefab, CardViewController deckCardViewPrefab)
    {
        this.deckCardViewPrefab = deckCardViewPrefab;
        this.cardViewPrefab = cardViewPrefab;
        this.library = library;
        
        closeEditorButton.onClick.AddListener(CloseDeckEditor);
        filters = filterCollectionViewController;
    }
    
    private void CloseDeckEditor()
    {
        //viewFader.FadeOut();
        gameObject.SetActive(false);
        
        OnClose?.Invoke();
    }

    public void Enter(DeckData selectedDeck)
    {
        deckView.Initialize(library, selectedDeck);
    }

    public void AddCard(CardViewController cardView)
    {
        
    }

    public void RemoveCard(CardViewController cardView)
    {
        
    }
    
}
