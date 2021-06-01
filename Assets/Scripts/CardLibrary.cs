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
    
    public void Initialize()
    {
        cards = cards.Where(x => x != null).ToList();
        cards = cards.OrderBy(x => x.archetype).ThenBy(x => x.cost).ToList();
        
        decks = decks.Where(x => x != null).ToList();
        decks.ForEach(x => x.Initialize());
    }
}
