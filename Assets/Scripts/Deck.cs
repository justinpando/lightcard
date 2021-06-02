using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Deck 
{
    public string name = "New Deck";
    public string description;

    public DeckSaveData SaveData
    {
        get
        {
            var cardNames = new List<string>();

            cards.ForEach(x => cardNames.Add(x.name));

            return new DeckSaveData(name, description, cardNames);
        }
    }

    [SerializeField]
    public List<Card> cards = new List<Card>();

    private int cardLimit = 40;
    private int individualCardLimit = 3;

    public Action<string> OnMessage;
    public Action OnCardsUpdated;

    public readonly Dictionary<Card.Archetype, int> cardArchetypeCount = new Dictionary<Card.Archetype, int>();
    public readonly Dictionary<Card.Type, int> cardTypeCount = new Dictionary<Card.Type, int>();

    public List<KeyValuePair<Card.Archetype, int>> archetypeValues;

    [NonSerialized]
    private bool initialized;

    public Deck()
    {
        
    }
    
    public Deck(CardLibrary library, DeckSaveData deckSaveData)
    {
        if (deckSaveData != null)
        {
            foreach (var cardName in deckSaveData.cards)
            {
                AddCard(library.cardCollection.cards.First(x => x.name == cardName));
            }
        }
    }
    
    public void Initialize()
    {
        if (initialized) return;
        
        Debug.Log($"Initializing deck: {name}");
        
        cards = new List<Card>(cards.OrderBy(x => x.cost));
        
        foreach( Card.Archetype group in Enum.GetValues(typeof(Card.Archetype)) )
        {
            cardArchetypeCount.Add(group, 0);
        }
        foreach( Card.Type type in Enum.GetValues(typeof(Card.Type)) )
        {
            cardTypeCount.Add(type, 0);
        }

        foreach (var card in cards)
        {
            cardArchetypeCount[card.archetype]++;
            cardTypeCount[card.type]++;
        }
        
        archetypeValues = cardArchetypeCount.OrderByDescending(pair => pair.Value).ToList();

        initialized = true;
    }
    
    public bool AddCard(Card card)
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
        cards = new List<Card>(cards.OrderBy(x => x.cost));
        
        cardArchetypeCount[card.archetype]++;
        cardTypeCount[card.type]++;

        archetypeValues = cardArchetypeCount.OrderByDescending(pair => pair.Value).ToList();
        
        OnMessage?.Invoke($"Added {card.name}.");
        OnCardsUpdated?.Invoke();
        
        Debug.Log($"Added card: {card.name}");

        return true;
    }

    public void RemoveCard(Card card)
    {
        if (!cards.Contains(card)) return;
        
        cards.Remove(card);
        
        cardArchetypeCount[card.archetype]--;
        cardTypeCount[card.type]--;
        
        archetypeValues = cardArchetypeCount.OrderByDescending(pair => pair.Value).ToList();
        
        OnMessage?.Invoke($"Removed {card.name}.");
        OnCardsUpdated?.Invoke();

        Debug.Log($"Removed card: {card.name}");
    }

    public void SetCardList(List<Card> newCards)
    {
        foreach (var card in cards)
        {
            cardArchetypeCount[card.archetype]--;
            cardTypeCount[card.type]--;
        }
        
        cards = newCards;
        
        foreach (var card in cards)
        {
            cardArchetypeCount[card.archetype]++;
            cardTypeCount[card.type]++;
        }
        
        archetypeValues = cardArchetypeCount.OrderByDescending(pair => pair.Value).ToList();
        
        OnCardsUpdated?.Invoke();
    }
    
    private void Shuffle()
    {
        cards = cards.OrderBy(c => Guid.NewGuid())
            .ToList();
    }
    
    public int GetCardCount(Card card)
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
