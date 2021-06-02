using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "My Assets/CardLibrary")]
public class CardLibrary : ScriptableObject
{
    public List<ArchetypeData> classes;
    public List<CardTypeData> types;
    public List<Card> cards;
    public List<Deck> decks;
    public List<DeckData> starterDecks;
    
    public void Initialize(SaveData save = null)
    {
        cards = cards.Where(x => x != null).ToList();
        cards = cards.OrderBy(x => x.archetype).ThenBy(x => x.cost).ToList();

        if(save != null) decks = save.decks;

        starterDecks.ForEach(x => x.deck.Initialize());
        
        decks = decks.Where(x => x != null).ToList();
        decks.ForEach(x => x.Initialize());
    }
}
