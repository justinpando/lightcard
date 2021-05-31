using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "My Assets/CardLibrary")]
public class CardLibrary : ScriptableObject
{
    public List<CardClassData> classes;
    public List<CardTypeData> types;
    public List<CardData> cards;

    public List<DeckData> decks;
    
    public void Initialize()
    {
        cards = cards.Where(x => x != null).ToList();
        decks = decks.Where(x => x != null).ToList();
    }
}
