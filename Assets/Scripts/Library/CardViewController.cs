using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardViewController : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text typeText;
    public TMP_Text descriptionText;
    
    public TMP_Text energyText;
    public TMP_Text attackText;
    public TMP_Text defenseText;
    public TMP_Text classText;

    public Image bgImage;
    public Image borderImage;
    public Image cardImage;
    public Image[] classImages;

    public Card Card { get; private set; }

    public Transform unitBG;
    public Transform abilityBG;
    public Transform charmBG;

    public Image[] primaryColorImages;
    public Image[] secondaryColorImages;

    public Button selectButton;
    private static readonly int ColorA = Shader.PropertyToID("_ColorA");
    private static readonly int ColorB = Shader.PropertyToID("_ColorB");


    public void Initialize(Card card, ArchetypeData classData)
    {
        this.Card = card;
        
        nameText.text = card.name;
        typeText.text = $"{card.archetype} {card.type}";
        descriptionText.text = card.description;
        attackText.text = $"{card.power}";
        defenseText.text = $"{card.life}";
        energyText.text = $"{card.cost}";
        
        classText.text = $"{card.archetype}";

        attackText.transform.parent.gameObject.SetActive(card.type == Card.Type.Unit);
        defenseText.transform.parent.gameObject.SetActive(card.type != Card.Type.Ability);

        unitBG.gameObject.SetActive(card.type == Card.Type.Unit);
        abilityBG.gameObject.SetActive(card.type == Card.Type.Ability);
        charmBG.gameObject.SetActive(card.type == Card.Type.Charm);
        
        borderImage.color = classData.highlightColor;
        bgImage.color = classData.bgColor;

        classImages[1].color = classData.baseColor;

        foreach (var image in primaryColorImages)
        {
            //image.color = classData.primaryColor;
            image.color = classData.highlightColor;

        }
        
        foreach (var image in secondaryColorImages)
        {
            image.color = classData.baseColor;
        }
        
        foreach (var image in classImages)
        {
            image.sprite = classData.symbol;
        }


        Material cardMat = new Material(cardImage.material);

        cardImage.material = cardMat;
        borderImage.material = cardMat;
        cardImage.material.SetColor(ColorA, classData.highlightColor);
        cardImage.material.SetColor(ColorB, classData.accentColor);
        
        cardImage.sprite = card.sprite;
    }
    
}
