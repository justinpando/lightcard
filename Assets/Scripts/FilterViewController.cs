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
    
    public CardFilter filter;

    public CanvasGroupFader fader;
    
    public void Initialize(string label, Color primary, Color highlight, Sprite sprite, CardFilter filter)
    {
        categoryImage.sprite = sprite;
        categoryImage.color = primary;
        
        highlightImage.sprite = sprite;
        highlightImage.color = highlight;

        this.label.text = label;
        
        this.filter = filter;
        
        fader.SetVisibility(toggle.isOn);
    }
    
}
