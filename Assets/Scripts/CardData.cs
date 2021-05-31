using UnityEngine;

public class CardData : ScriptableObject
{
    public enum Group {Garden, Atelier, Heart, Ocean, Tower, Expedition}
    public Group group;

    public enum Type { Unit, Charm, Ability }
    public Type type;
    
    public Sprite sprite;
    
    public string name;
    
    public int cost;
    
    public int attack;
    public int defense;
    
    public string description;
}
