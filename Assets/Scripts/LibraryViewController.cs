using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LibraryViewController : MonoBehaviour
{
    public Transform cardViewCollectionPanel;
    private List<CardData> cardDataCollection;

    public CardViewController cardViewPrefab;
    private List<CardViewController> cardViews = new List<CardViewController>();
    
    public void Initialize(List<CardData> cardCollection, CardViewController cardViewPrefab)
    {
        cardDataCollection = cardCollection;
        this.cardViewPrefab = cardViewPrefab;

        cardViews = GetComponentsInChildren<CardViewController>().ToList();

        for (int n = 0; n < cardViews.Count; n++)
        {
            Destroy(cardViews[n].gameObject);
        }
        
        cardViews.Clear();
        
        foreach (var card in cardCollection)
        {
            AddCardView(card);
        }
    }

    private void AddCardView(CardData cardData)
    {
        CardViewController view = Instantiate(cardViewPrefab, cardViewCollectionPanel);
        
        view.Initialize(cardData);
        
        cardViews.Add(view);
    }

    
}
