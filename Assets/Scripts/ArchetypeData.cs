using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "My Assets/ArchetypeData")]
public class ArchetypeData : ScriptableObject
{
    [FormerlySerializedAs("group")] public Card.Archetype archetype;
    [FormerlySerializedAs("color")] public Color primaryColor;
    [FormerlySerializedAs("secondaryColor")] public Color highlightColor;
    public Color bgColor;
    public Sprite symbol;
}
