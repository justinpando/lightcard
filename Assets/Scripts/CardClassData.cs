using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "My Assets/CardClassData")]
public class CardClassData : ScriptableObject
{
    public CardData.Group group;
    [FormerlySerializedAs("color")] public Color primaryColor;
    [FormerlySerializedAs("secondaryColor")] public Color highlightColor;
    public Color bgColor;
    public Sprite symbol;
}
