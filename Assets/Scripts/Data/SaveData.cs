using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SaveData
{
    public List<DeckSaveData> decks;

    public SaveData(CardLibrary library)
    {
        var deckList = library.Decks.ConvertAll(x => x.SaveData).ToList();
        
        decks = deckList;
    }
}
