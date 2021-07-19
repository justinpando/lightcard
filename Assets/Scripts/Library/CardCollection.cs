using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "My Assets/CardCollection")]
public class CardCollection : ScriptableObject
{
    public List<Card> cards = new List<Card>();

    public void Initialize()
    {
        cards = cards.Where(x => x != null).ToList();
        cards = cards.OrderBy(x => x.archetype).ThenBy(x => x.cost).ToList();
    }
}
