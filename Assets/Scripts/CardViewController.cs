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

    public CardData cardData { get; private set; }
    
    public void Initialize(CardData cardData, CardClassData classData)
    {
        this.cardData = cardData;
        
        nameText.text = cardData.name;
        typeText.text = $"{cardData.group} {cardData.type}";
        descriptionText.text = cardData.description;
        attackText.text = $"{cardData.attack}";
        defenseText.text = $"{cardData.defense}";
        energyText.text = $"{cardData.cost}";
        
        classText.text = $"{cardData.group}";

        attackText.transform.parent.gameObject.SetActive(cardData.type == CardData.Type.Unit);
        defenseText.transform.parent.gameObject.SetActive(cardData.type != CardData.Type.Ability);

        borderImage.color = classData.primaryColor;
        bgImage.color = classData.secondaryColor;

        classImages[1].color = classData.primaryColor;

        foreach (var image in classImages)
        {
            image.sprite = classData.symbol;
        }
    }
    
}
