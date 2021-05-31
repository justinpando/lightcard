using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "My Assets/DeckData")]
public class DeckData : ScriptableObject
{
    public string name = "New Deck";
    public CardData.Group group;
    public string description;

    public List<CardData> cards = new List<CardData>();

    private int cardLimit = 40;
    private int individualCardLimit = 3;

    public Action<string> OnMessage;
    public Action OnCardsUpdated;

    public Dictionary<CardData.Group, int> cardClassCount = new Dictionary<CardData.Group, int>();
    public Dictionary<CardData.Type, int> cardTypeCount = new Dictionary<CardData.Type, int>();

    public void Initialize()
    {
        foreach( CardData.Group group in Enum.GetValues(typeof(CardData.Group)) )
        {
            cardClassCount.Add(group, 0);
        }
        foreach( CardData.Type type in Enum.GetValues(typeof(CardData.Type)) )
        {
            cardTypeCount.Add(type, 0);
        }

        foreach (var card in cards)
        {
            cardClassCount[card.@group]++;
            cardTypeCount[card.type]++;
        }
    }
    
    public bool AddCard(CardData card)
    {
        if (cards.Count == cardLimit)
        {
            OnMessage?.Invoke($"Already have {cardLimit} cards in deck.");
            return false;
        }

        if (GetCardCount(card) > individualCardLimit)
        {
            OnMessage?.Invoke($"Already have {individualCardLimit} copies of {card.name}.");
            return false;
        }
        
        cards.Add(card);
        cards = new List<CardData>(cards.OrderBy(x => x.cost));
        
        cardClassCount[card.group]++;
        cardTypeCount[card.type]++;
        
        OnMessage?.Invoke($"Added {card.name}.");
        OnCardsUpdated?.Invoke();
        
        Debug.Log($"Added card: {card.name}");

        return true;
    }

    public void RemoveCard(CardData card)
    {
        if (!cards.Contains(card)) return;
        
        cards.Remove(card);
        
        cardClassCount[card.@group]--;
        cardTypeCount[card.type]--;
        
        OnMessage?.Invoke($"Removed {card.name}.");
        OnCardsUpdated?.Invoke();
        
        Debug.Log($"Removed card: {card.name}");
    }

    private void UpdateGroup()
    {
        
    }
    
    public int GetCardCount(CardData card)
    {
        int count = 0;
        
        foreach (var cardData in cards)
        {
            if (cardData.name == card.name)
            {
                count++;
            }
        }

        return count;
    }
}
