using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    [SerializeField]
    public List<Deck> decks;

    public SaveData(CardLibrary library)
    {
        decks = library.decks;
    }
}
