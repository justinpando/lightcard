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

    private Dictionary<CardData.Group, int> cardClassCount = new Dictionary<CardData.Group, int>();

    public void Initialize()
    {
        foreach( CardData.Group group in Enum.GetValues(typeof(CardData.Group)) )
        {
            cardClassCount.Add(group, 0);
        }
    }
    
    public void AddCard(CardData card)
    {
        if (cards.Count == cardLimit)
        {
            OnMessage?.Invoke($"Already have {cardLimit} cards in deck.");
            return;
        }

        if (GetCardCount(card) > individualCardLimit)
        {
            OnMessage?.Invoke($"Already have {individualCardLimit} copies of {card.name}.");
            return;
        }
        
        cards.Add(card);
        
        cards = new List<CardData>(cards.OrderBy(x => x.cost));
        
        OnMessage?.Invoke($"Added {card.name}.");
        OnCardsUpdated?.Invoke();
    }

    public void RemoveCard(CardData card)
    {
        cards.Remove(card);
        OnMessage?.Invoke($"Removed {card.name}.");
        OnCardsUpdated?.Invoke();
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
