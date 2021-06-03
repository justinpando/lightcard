using System;
using System.Collections.Generic;

[Serializable]
public class DeckSaveData
{
    public string name;
    public string description;
    public List<string> cards;

    public DeckSaveData(string name, string description, List<string> cards)
    {
        this.name = name;
        this.description = description;
        this.cards = cards;
    }
}
