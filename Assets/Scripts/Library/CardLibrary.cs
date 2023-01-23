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
        
        if(save != null) Decks = save.decks.ConvertAll(x => new Deck(this, x));
        
        Decks = Decks.Where(x => x != null).ToList();
    }

    public void SortDecks(List<DeckItemView> deckViews)
    {
        Decks = new List<Deck>(Decks.OrderBy(deck => deckViews.FindIndex(deckView => deckView.deck == deck)));
    }
    
}