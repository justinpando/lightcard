using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckViewController : MonoBehaviour
{
    public CardViewController cardViewPrefab;
    
    public void Initialize(CardViewController cardViewPrefab)
    {
        this.cardViewPrefab = cardViewPrefab;
    }
}
