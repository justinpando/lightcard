using System;
using System.Collections.Generic;

[Serializable]
public class DeckSaveData
{
    public string name;
    public string description;
    public List<string> cards;
    /// <summary>The player power this deck brings: "Shift" (default) or "Clear" (rules-v3).</summary>
    public string power;

    public DeckSaveData(string name, string description, List<string> cards, string power = "Shift")
    {
        this.name = name;
        this.description = description;
        this.cards = cards;
        this.power = power;
    }
}
