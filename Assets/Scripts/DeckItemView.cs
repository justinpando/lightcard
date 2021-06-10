using System.Collections.Generic;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class DeckItemView : MonoBehaviour
{
    public Image[] singleClassIcon;
    public Image[] dualClassIcons;
    public Image[] infoIcons;
    
    public Image highlightImage;
    public Image bgImage;
    public Gradient2 bgGradient;

    public CanvasGroup nameInputCanvasGroup;
    public TMP_InputField nameInputField;
    public TMP_Text nameText;
    public TMP_Text unitText;
    public TMP_Text abilityText;
    public TMP_Text charmText;

    
    public Button selectButton;
    public Button deleteButton;
    public Button copyButton;

    public Deck deck;

    public System.Action OnSelectButtonPressed;
    public System.Action OnDeleteButtonPressed;
    public System.Action OnCopyButtonPressed;
    private CardLibrary library;
    private static readonly int ColorA = Shader.PropertyToID("_ColorA");
    private static readonly int ColorB = Shader.PropertyToID("_ColorB");

    public void Initialize(CardLibrary library, Deck deck)
    {
        this.library = library;
        this.deck = deck;

        deck.OnCardsUpdated += UpdateView;
        
        singleClassIcon.ForEach(x => x.gameObject.SetActive(false));
        dualClassIcons.ForEach(x => x.gameObject.SetActive(false));
        
        UpdateView();
        
        selectButton.onClick.AddListener(() => OnSelectButtonPressed?.Invoke());
        deleteButton.onClick.AddListener(() => OnDeleteButtonPressed?.Invoke());
        copyButton.onClick.AddListener(() => OnCopyButtonPressed?.Invoke());
        
        nameInputField.onDeselect.AddListener(value =>
        {
            deck.name = value;
            UpdateView();
            Debug.Log($"Deck name updated: {deck.name}");
        });
        
        Material cardMat = new Material(bgImage.material);
        bgImage.material = cardMat;
    }

    private void UpdateView()
    {
        Debug.Log($"Updating deck item view for: {deck.name}", gameObject);
        
        nameInputField.SetTextWithoutNotify(deck.name);
        //nameText.text = deck.name;
        
        if (deck.cards.Count == 0)
        {
            foreach (Image image in singleClassIcon)
            {
                image.gameObject.SetActive(false);
            }

            unitText.text = "0";
            abilityText.text = "0";
            charmText.text = "0";
            
            bgGradient.enabled = false;
            
            return;
        }

        int unitCount = deck.cardTypeCount[Card.Type.Unit];
        int abilityCount = deck.cardTypeCount[Card.Type.Ability];
        int charmCount = deck.cardTypeCount[Card.Type.Charm];
        
        unitText.text = unitCount.ToString();
        abilityText.text = abilityCount.ToString();
        charmText.text = charmCount.ToString();
        
        UpdateArchetypeIcons(deck.archetypeValues[0], deck.archetypeValues[1]);
    }

    void UpdateArchetypeIcons(KeyValuePair<Card.Archetype, int> archetype1, KeyValuePair<Card.Archetype, int> archetype2)
    {
        var archetypeData1 = library.classes.Find(x => x.archetype == archetype1.Key);
        
        bgGradient.enabled = false;
        highlightImage.color = archetypeData1.highlightColor;
        
        infoIcons.ForEach(x => x.color = archetypeData1.highlightColor);
        
        //If there's only one archetype, 
        if (archetype2.Value == 0)
        {
            dualClassIcons.ForEach(x => x.gameObject.SetActive(false));
            
            //Show the single archetype icon
            foreach (Image image in singleClassIcon)
            {
                if (!image.gameObject.activeSelf) image.gameObject.SetActive(true);
                image.sprite = archetypeData1.symbol;
            }

            singleClassIcon[0].color = archetypeData1.highlightColor;
            singleClassIcon[1].color = archetypeData1.baseColor;

            bgImage.material.SetColor(ColorA, archetypeData1.deckColor);
            bgImage.material.SetColor(ColorB, archetypeData1.accentColor);
            
            // bgGradient.EffectGradient.SetKeys(
            //     new GradientColorKey[]
            //     {
            //         new GradientColorKey( archetypeData1.highlightColor, 0f),
            //         new GradientColorKey( archetypeData1.baseColor, 1f)
            //     }, 
            //     new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f) });
        }
        else
        {
            var archetypeData2 = library.classes.Find(x => x.archetype == archetype2.Key);
            
            //Otherwise show the dual archetype icons
            singleClassIcon.ForEach(x => x.gameObject.SetActive(false));
            dualClassIcons.ForEach(x => x.gameObject.SetActive(true));

            dualClassIcons[0].sprite = archetypeData1.symbol;
            dualClassIcons[1].sprite = archetypeData1.symbol;
            dualClassIcons[0].color = archetypeData1.highlightColor;
            dualClassIcons[1].color = archetypeData1.baseColor;
            
            dualClassIcons[2].sprite = archetypeData2.symbol;
            dualClassIcons[3].sprite = archetypeData2.symbol;
            dualClassIcons[2].color = archetypeData2.highlightColor;
            dualClassIcons[3].color = archetypeData2.baseColor;
            
            //borderImage.material = cardMat;
            bgImage.material.SetColor(ColorA, archetypeData1.deckColor);
            bgImage.material.SetColor(ColorB, archetypeData2.accentColor);
            
            // bgGradient.EffectGradient.SetKeys(
            //     new GradientColorKey[]
            //     {
            //         new GradientColorKey( archetypeData1.baseColor, 0f),
            //         new GradientColorKey( archetypeData2.baseColor, 1f)
            //     }, 
            //     new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f) });

        }
        
        bgGradient.enabled = true;
    }
}
