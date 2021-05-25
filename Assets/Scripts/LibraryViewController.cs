using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class LibraryViewController : MonoBehaviour
{
    private CardLibrary cardLibrary;

    //Cards
    public Transform cardViewCollectionPanel;
    public CardViewController cardViewPrefab;
    private List<CardViewController> cardViews = new List<CardViewController>();
    public Scrollbar scrollBar;
    
    //Filters
    public Transform filterPanel;
    public FilterViewController filterViewPrefab;
    private List<FilterViewController> filterViews = new List<FilterViewController>();
    
    public void Initialize(CardLibrary cardLibrary, CardViewController cardViewPrefab,
        FilterViewController filterViewPrefab)
    {
        this.cardLibrary = cardLibrary;
        this.cardViewPrefab = cardViewPrefab;
        this.filterViewPrefab = filterViewPrefab;
        
        cardViews = GetComponentsInChildren<CardViewController>().ToList();
        
        InitializeCards();
        InitializeFilters();
    }

    private void InitializeCards()
    {
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

    private void InitializeFilters()
    {
        filterViews = GetComponentsInChildren<FilterViewController>().ToList();

        for (int n = 0; n < filterViews.Count; n++)
        {
            Destroy(filterViews[n].gameObject);
        }

        foreach (var classData in cardLibrary.classes)
        {
            FilterViewController filter = Instantiate(filterViewPrefab, filterPanel);
            
            filter.Initialize(classData.@group.ToString(), classData.primaryColor, classData.secondaryColor, classData.symbol, card => card.@group == classData.@group);
        }
    }
    
    private void AddCardView(CardData cardData)
    {
        CardViewController view = Instantiate(cardViewPrefab, cardViewCollectionPanel);
        
        view.Initialize(cardData, cardLibrary.classes.Find(x => x.group == cardData.group));
        
        cardViews.Add(view);
    }

    
}
