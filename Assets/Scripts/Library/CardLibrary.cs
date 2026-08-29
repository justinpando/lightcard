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

        if (save?.decks != null)
        {
            Decks = save.decks.ConvertAll(x => new Deck(this, x)).Where(x => x != null).ToList();
        }
        else
        {
            //First run: seed the player's collection with copies of the starter decks,
            //so they can be edited, deleted, and saved like any other deck
            Decks = new List<Deck>();

            foreach (var deckData in starterDecks)
            {
                if (deckData == null || deckData.deck == null) continue;

                var copy = new Deck { name = deckData.deck.name, description = deckData.deck.description };
                copy.SetCardList(deckData.deck.cards);

                Decks.Add(copy);
            }
        }
    }

    public void SortDecks(List<DeckItemView> deckViews)
    {
        Decks = new List<Deck>(Decks.OrderBy(deck => deckViews.FindIndex(deckView => deckView.deck == deck)));
    }
    
}
