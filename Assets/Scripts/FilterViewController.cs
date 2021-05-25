using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FilterViewController : MonoBehaviour
{
    public Image categoryImage;
    public Image highlightImage;
    public Toggle toggle;

    public TMP_Text label;
    
    public delegate bool CardFilter(CardData card);
    public CardFilter filter;
    
    public void Initialize(string label, Color primary, Color highlight, Sprite sprite, CardFilter filterAction)
    {
        categoryImage.sprite = sprite;
        categoryImage.color = primary;
        
        highlightImage.sprite = sprite;
        highlightImage.color = highlight;

        this.label.text = label;
        
        filter = filterAction;
    }
    
    
}
