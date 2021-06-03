using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "My Assets/CardLibrary")]
public class CardLibrary : ScriptableObject
{
    public List<ArchetypeData> classes;
    public List<CardTypeData> types;
    public CardCollection cardCollection;
    public List<DeckData> starterDecks;
    public List<Deck> Decks { get; private set; }
    
    public void Initialize(SaveData save = null)
    {
        cardCollection.Initialize();

        //starterDecks.ForEach(x => x.deck.Initialize());
        
        if(save != null) Decks = save.decks.ConvertAll(x => new Deck(this, x));
        
        Decks = Decks.Where(x => x != null).ToList();
        //Decks.ForEach(x => x.Initialize());
    }
    
}
