using UnityEngine;

public class Card : ScriptableObject
{
    public enum Archetype {Garden, Atelier, Heart, Ocean, Tower, Expedition}
    public Archetype archetype;

    public enum Type { Unit, Charm, Ability }
    public Type type;
    
    public Sprite sprite;
    
    public string name;
    
    public int cost;
    
    public int power;
    public int life;
    
    public string description;
}
