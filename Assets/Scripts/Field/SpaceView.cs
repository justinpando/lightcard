using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LightCard.Core;

/// <summary>
/// One board space in the Field scene. Placed in the scene art; given its engine
/// coordinates by FieldViewController at match start. Renders the occupying unit
/// and space effect on its child "Unit Canvas", and reports clicks upward.
/// </summary>
public class SpaceView : MonoBehaviour
{
    public enum Highlight { None, PlayTarget, ShiftTarget, Attack, Selected }

    public int X { get; private set; } = -1;
    public int Y { get; private set; } = -1;
    public int UnitId { get; private set; } = -1;

    public Action<SpaceView> OnClicked;

    private static readonly Color friendlyColor = new Color(0.25f, 0.65f, 0.80f);
    private static readonly Color enemyColor = new Color(0.85f, 0.30f, 0.30f);
    private static readonly Color charmTint = new Color(0.75f, 0.65f, 0.95f);

    private Canvas unitCanvas;
    private Image unitImage;
    private Image effectImage;
    private Image highlightImage;
    private TMP_Text statsText;
    private TMP_Text nameText;
    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;

        if (GetComponent<Collider>() == null)
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(0.7f, 0.1f, 0.7f);
        }

        unitCanvas = GetComponentInChildren<Canvas>(true);
        if (unitCanvas == null) return;

        var imageTransform = unitCanvas.transform.Find("Image");
        unitImage = imageTransform != null ? imageTransform.GetComponent<Image>() : null;

        effectImage = CreateOverlay("Effect Overlay");
        effectImage.transform.SetAsFirstSibling();

        highlightImage = CreateOverlay("Highlight Overlay");

        nameText = CreateText("Unit Name", new Vector2(0f, 0.30f), new Vector2(1f, 0.55f));
        statsText = CreateText("Unit Stats", new Vector2(0f, 0.0f), new Vector2(1f, 0.30f));

        ClearUnit();
        SetSpaceEffect(SpaceEffectType.None);
        SetHighlight(Highlight.None);
    }

    private Image CreateOverlay(string overlayName)
    {
        var go = new GameObject(overlayName, typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)go.transform;
        rect.SetParent(unitCanvas.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var image = go.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    private TMP_Text CreateText(string textName, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(textName, typeof(RectTransform), typeof(TextMeshProUGUI));
        var rect = (RectTransform)go.transform;
        rect.SetParent(unitCanvas.transform, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var text = go.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 0.01f;
        text.fontSizeMax = 400f;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    public void SetCoordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void ShowUnit(GameState state, UnitState unit, int localPlayer)
    {
        UnitId = unit.Id;

        Color color = unit.Owner == localPlayer ? friendlyColor : enemyColor;
        if (unit.IsCharm) color = Color.Lerp(color, charmTint, 0.5f);
        //Spent, sleeping, or just-called units render dimmed
        bool dimmed = unit.Asleep || unit.Flux || unit.AttackedThisTurn || unit.MovedThisTurn;
        color.a = dimmed ? 0.45f : 1f;

        if (unitImage != null)
        {
            unitImage.enabled = true;
            unitImage.color = color;
        }

        if (nameText != null)
        {
            nameText.text = unit.CardId;
            nameText.alpha = color.a;
        }

        if (statsText != null)
        {
            statsText.text = unit.IsCharm
                ? $"{state.CurrentLife(unit)}"
                : $"{state.EffectivePower(unit)}/{state.CurrentLife(unit)}";
            if (unit.Asleep) statsText.text += " z";
            statsText.alpha = color.a;
        }
    }

    public void ClearUnit()
    {
        UnitId = -1;
        if (unitImage != null) unitImage.enabled = false;
        if (nameText != null) nameText.text = "";
        if (statsText != null) statsText.text = "";
    }

    public void SetSpaceEffect(SpaceEffectType effect)
    {
        if (effectImage == null) return;

        switch (effect)
        {
            case SpaceEffectType.Verdant:
                effectImage.color = new Color(0.30f, 0.75f, 0.35f, 0.45f);
                break;
            case SpaceEffectType.Brambled:
                effectImage.color = new Color(0.60f, 0.35f, 0.20f, 0.45f);
                break;
            case SpaceEffectType.Vista:
                effectImage.color = new Color(0.45f, 0.60f, 0.95f, 0.45f);
                break;
            default:
                effectImage.color = Color.clear;
                break;
        }
    }

    public void SetHighlight(Highlight highlight)
    {
        if (highlightImage == null) return;

        switch (highlight)
        {
            case Highlight.PlayTarget:
                highlightImage.color = new Color(0.35f, 0.95f, 0.45f, 0.40f);
                break;
            case Highlight.ShiftTarget:
                highlightImage.color = new Color(0.35f, 0.65f, 0.95f, 0.40f);
                break;
            case Highlight.Attack:
                highlightImage.color = new Color(0.95f, 0.35f, 0.25f, 0.45f);
                break;
            case Highlight.Selected:
                highlightImage.color = new Color(0.95f, 0.85f, 0.30f, 0.45f);
                break;
            default:
                highlightImage.color = Color.clear;
                break;
        }

        transform.localScale = highlight == Highlight.None ? baseScale : baseScale * 1.08f;
    }

    private void OnMouseUpAsButton()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        OnClicked?.Invoke(this);
    }
}
