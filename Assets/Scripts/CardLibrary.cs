using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "My Assets/CardLibrary")]
public class CardLibrary : ScriptableObject
{
    public List<CardClassData> classes;
    public List<CardData> cards;
    
    
}