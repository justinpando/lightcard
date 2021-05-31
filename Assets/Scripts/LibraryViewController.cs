using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LibraryViewController : MonoBehaviour
{
    private CardLibrary cardLibrary;

    //Cards
    public GridLayoutGroup cardViewCollectionPanel;
    public CardViewController cardViewPrefab;
    private List<CardViewController> cardViews = new List<CardViewController>();
    public Scrollbar scrollBar;

    public FilterCollectionViewController filters;

    public void Initialize(CardLibrary cardLibrary, CardViewController cardViewPrefab,
        FilterCollectionViewController filters)
    {
        this.cardLibrary = cardLibrary;
        this.cardViewPrefab = cardViewPrefab;
        this.filters = filters;

        InitializeCards();
        
        filters.OnFiltersUpdated += HandleFiltersUpdated;
    }

    private void InitializeCards()
    {
        cardViews = GetComponentsInChildren<CardViewController>().ToList();
        
        for (int n = 0; n < cardViews.Count; n++)
        {
            Destroy(cardViews[n].gameObject);
        }
        
        cardViews.Clear();
        
        cardLibrary.cards = cardLibrary.cards.OrderBy(x => x.@group).ToList();
        
        foreach (var card in cardLibrary.cards)
        {
            AddCardView(card);
        }
        
        scrollBar.value = 1f;
    }

    private void AddCardView(CardData cardData)
    {
        CardViewController view = Instantiate(cardViewPrefab, cardViewCollectionPanel.transform);
        
        view.Initialize(cardData, cardLibrary.classes.Find(x => x.group == cardData.group));
        
        cardViews.Add(view);
    }

    private void HandleFiltersUpdated()
    {
        filters.FilterCardViews(cardViews);
    }
    
}
