using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "My Assets/DeckData")]
public class DeckData : ScriptableObject
{
    public string name = "New Deck";
    public CardData.Group group;
    public string description;
    public List<CardData> cards { get; private set; }

    private int cardLimit = 40;
    private int individualCardLimit = 3;

    public System.Action<string> OnActionComplete;
    
    public void AddCard(CardData card)
    {
        if (cards.Count == cardLimit)
        {
            OnActionComplete?.Invoke($"Already have {cardLimit} cards in deck.");
            return;
        }

        if (GetCardCount(card) > individualCardLimit)
        {
            OnActionComplete?.Invoke($"Already have {individualCardLimit} copies of {card.name}.");
            return;
        }
        
        cards.Add(card);
        
        OnActionComplete?.Invoke($"Added {card.name}.");
    }

    public void RemoveCard(CardData card)
    {
        cards.Remove(card);
        OnActionComplete?.Invoke($"Removed {card.name}.");
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
