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

    public Image cardImage;
    public Image classImage;
    
    public void Initialize(CardData data)
    {
        nameText.text = data.name;
        typeText.text = $"{data.group} {data.type}";
        descriptionText.text = data.description;
        attackText.text = $"{data.attack}";
        defenseText.text = $"{data.defense}";
        energyText.text = $"{data.cost}";
        
        classText.text = $"{data.group}";
    }
    
}
