using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FilterCollectionViewController : MonoBehaviour
{
    private CardLibrary cardLibrary;
    
    //Filters
    public Transform classFilterPanel;
    public Transform typeFilterPanel;
    public FilterViewController filterViewPrefab;
    private List<FilterViewController> filterViews = new List<FilterViewController>();

    private List<CardFilter> activeFilters = new List<CardFilter>();

    public System.Action OnFiltersUpdated;
    
    public void Initialize(CardLibrary cardLibrary, FilterViewController filterViewPrefab)
    {
        this.cardLibrary = cardLibrary;
        this.filterViewPrefab = filterViewPrefab;
        
        InitializeFilters();
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
            filterView.toggle.onValueChanged.AddListener(isOn => UpdateFilters(filterView.filter, isOn));
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
            filterView.toggle.onValueChanged.AddListener(isOn => UpdateFilters(filterView.filter, isOn));
        }
    }

    private void UpdateFilters(CardFilter interactedFilter, bool filterEnabled)
    {
        if(filterEnabled) activeFilters.Add(interactedFilter);
        else activeFilters.Remove(interactedFilter);
        
        OnFiltersUpdated?.Invoke();
    }

    public void FilterCardViews(List<CardViewController> cardViews)
    {
        List<CardViewController> validCards = GetValidCards(cardViews);
        
        //Hide all cards in the filtered card list
        cardViews.ForEach(x => x.gameObject.SetActive(validCards.Contains(x)));
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardViews[0].transform.parent as RectTransform);
    }

    public List<CardViewController> GetValidCards(List<CardViewController> cardViews)
    {
        //build lists of filters, by category
        Dictionary<string, List<CardFilter>> categoryFilters = new Dictionary<string, List<CardFilter>>();

        List<CardViewController> validCards = new List<CardViewController>(cardViews);

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
            //For each card,
            foreach (var cardView in cardViews)
            {
                //For each category,
                foreach (var category in categoryFilters)
                {
                    if (!validCards.Contains(cardView)) continue;

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
                    if (!isValid) validCards.Remove(cardView);
                }
            }
            
        }
        
        return validCards;
    }
}
