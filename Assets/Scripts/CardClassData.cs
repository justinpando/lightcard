using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "My Assets/ClassData")]
public class CardClassData : ScriptableObject
{
    public CardData.Group group;
    [FormerlySerializedAs("color")] public Color primaryColor;
    public Color secondaryColor;
    public Sprite symbol;
}
