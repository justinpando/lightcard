using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LibraryViewController : MonoBehaviour
{
    public Transform cardViewCollectionPanel;
    private CardLibrary cardLibrary;

    public CardViewController cardViewPrefab;
    private List<CardViewController> cardViews = new List<CardViewController>();

    public Scrollbar scrollBar;
    
    public void Initialize(CardLibrary cardLibrary, CardViewController cardViewPrefab)
    {
        this.cardLibrary = cardLibrary;
        this.cardViewPrefab = cardViewPrefab;

        cardViews = GetComponentsInChildren<CardViewController>().ToList();

        for (int n = 0; n < cardViews.Count; n++)
        {
            Destroy(cardViews[n].gameObject);
        }
        
        cardViews.Clear();
        
        foreach (var card in cardLibrary.cards)
        {
            AddCardView(card);
        }

        scrollBar.value = 1f;
    }

    private void AddCardView(CardData cardData)
    {
        CardViewController view = Instantiate(cardViewPrefab, cardViewCollectionPanel);
        
        view.Initialize(cardData, cardLibrary.classes.Find(x => x.group == cardData.group));
        
        cardViews.Add(view);
    }

    
}
