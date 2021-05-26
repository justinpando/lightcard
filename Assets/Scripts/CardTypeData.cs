using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "My Assets/CardTypeData")]
public class CardTypeData : ScriptableObject
{
    public CardData.Type type;
    public string description;
    public Sprite symbol;
    [FormerlySerializedAs("primaryColor")] public Color baseColor;
    [FormerlySerializedAs("secondaryColor")] public Color highlightColor;
}
