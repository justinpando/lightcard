using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "My Assets/ArchetypeData")]
public class ArchetypeData : ScriptableObject
{
    public Card.Archetype archetype;
    [FormerlySerializedAs("primaryColor")] public Color baseColor;
    public Color highlightColor;
    public Color bgColor;
    public Color accentColor;
    public Sprite symbol;
}
