using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Deck 
{
    public string name = "New Deck";
    public string description;
    /// <summary>"Shift" or "Clear" — the player power this deck brings (rules-v3).</summary>
    public string power = "Shift";

    public DeckSaveData SaveData
    {
        get
        {
            var cardNames = new List<string>();

            cards.ForEach(x => cardNames.Add(x.name));

            return new DeckSaveData(name, description, cardNames, power);
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

    public Deck()
    {
        Initialize();
    }
    
    public Deck(CardLibrary library, DeckSaveData saveData)
    {
        Initialize();
        
        if (saveData != null)
        {
            name = saveData.name;
            description = saveData.description;
            power = string.IsNullOrEmpty(saveData.power) ? "Shift" : saveData.power;
            
            foreach (var cardName in saveData.cards)
            {
                var card = library.cardCollection.cards.FirstOrDefault(x => x.name == cardName);

                if (card != null) AddCard(card);
                else Debug.LogAssertion($"Library does not contain card: {cardName}, couldn't add to deck {name}");
            }
        }
    }
    
    //Safe to call repeatedly: recomputes counts from the current card list, so decks
    //deserialized by Unity after construction (e.g. inside DeckData assets) can be refreshed
    public void Initialize()
    {
        cards = new List<Card>(cards.Where(x => x != null).OrderBy(x => x.cost));

        cardArchetypeCount.Clear();
        cardTypeCount.Clear();

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
    }
    
    public bool AddCard(Card card)
    {
        if (cards.Count >= cardLimit)
        {
            OnMessage?.Invoke($"Already have {cardLimit} cards in deck.");
            return false;
        }

        if (GetCardCount(card) >= individualCardLimit)
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
        Debug.Log($"Setting cardList for deck: {name}");
        
        foreach (var card in cards)
        {
            cardArchetypeCount[card.archetype]--;
            cardTypeCount[card.type]--;
        }
        
        cards = new List<Card>(newCards);
        
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
