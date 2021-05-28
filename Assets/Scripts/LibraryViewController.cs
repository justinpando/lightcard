using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
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
    
    //Filters
    public Transform classFilterPanel;
    public Transform typeFilterPanel;
    public FilterViewController filterViewPrefab;
    private List<FilterViewController> filterViews = new List<FilterViewController>();

    private List<CardFilter> activeFilters = new List<CardFilter>();

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

        //Set up class filters
        foreach (var classData in cardLibrary.classes)
        {
            //Create the filter view
            FilterViewController filterView = Instantiate(filterViewPrefab, classFilterPanel);
            
            //Initialize with group data
            filterView.Initialize(classData.@group.ToString(), classData.primaryColor, classData.secondaryColor, classData.symbol, new CardFilter("class", card => card.@group == classData.@group));
            
            //Subscribe to toggle event 
            filterView.toggle.isOn = false;
            filterView.toggle.onValueChanged.AddListener(isOn => FilterCards(filterView.filter, isOn));
        }
        
        //Set up type filters
        foreach (var typeData in cardLibrary.types)
        {
            //Create the filter view
            FilterViewController filterView = Instantiate(filterViewPrefab, typeFilterPanel);
            
            //Initialize with group data
            filterView.Initialize(typeData.type.ToString(), typeData.baseColor, typeData.highlightColor, typeData.symbol, new CardFilter("type", card => card.@type == typeData.type));
            
            //Subscribe to toggle event
            filterView.toggle.isOn = false;
            filterView.toggle.onValueChanged.AddListener(isOn => FilterCards(filterView.filter, isOn));
        }
    }

    private void FilterCards(CardFilter interactedFilter, bool filterEnabled)
    {
        if(filterEnabled) activeFilters.Add(interactedFilter);
        else activeFilters.Remove(interactedFilter);
        
        //build lists of filters, by category
        Dictionary<string, List<CardFilter>> categoryFilters = new Dictionary<string, List<CardFilter>>();

        foreach (var activeFilter in activeFilters)
        {
            if (!categoryFilters.ContainsKey(activeFilter.category))
            {
                categoryFilters.Add(activeFilter.category, new List<CardFilter>());
            }
            
            categoryFilters[activeFilter.category].Add(activeFilter);
        }
        
        //Make all cards visible if there are no active filters
        if (activeFilters.Count == 0)
        {
            cardViews.ForEach(x => x.gameObject.SetActive(true));
        }
        else
        {
            //Otherwise, build a list of filtered cards.
            List<CardViewController> filteredCards = new List<CardViewController>();

            //For each card,
            foreach (var cardView in cardViews)
            {
                //For each category,
                foreach (var category in categoryFilters)
                {
                    if (filteredCards.Contains(cardView)) continue;
                    
                    //Check whether the card is valid
                    bool isValid = false;
                    
                    foreach (var activeFilter in category.Value)
                    {
                        //If the card is valid,
                        if (activeFilter.isValid(cardView.cardData))
                        {
                            isValid = true;
                            break;
                        }
                    }
                    
                    //If the card is filtered, add it to the list of filtered cards
                    if(!isValid) filteredCards.Add(cardView);
                }
            }
            
            //Hide all cards in the filtered card list
            cardViews.ForEach(x => x.gameObject.SetActive(!filteredCards.Contains(x)));
        }
        
        UpdateLayout();
    }

    private void UpdateLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardViewCollectionPanel.transform as RectTransform);
        // cardViewCollectionPanel.enabled = false;
        // cardViewCollectionPanel.enabled = true;
        // cardViewCollectionPanel.CalculateLayoutInputVertical();
        // cardViewCollectionPanel.SetLayoutVertical();
        //Canvas.ForceUpdateCanvases();
    }
    
    private void AddCardView(CardData cardData)
    {
        CardViewController view = Instantiate(cardViewPrefab, cardViewCollectionPanel.transform);
        
        view.Initialize(cardData, cardLibrary.classes.Find(x => x.group == cardData.group));
        
        cardViews.Add(view);
    }

    
}
