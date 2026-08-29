using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LightCard.Core;

/// <summary>
/// Drives the Field scene's HUD art: both life sliders, the energy readout, the
/// affinity indicators, the End Turn button, the Cycle (replace) drop area, and
/// a runtime-created status line. All elements are discovered by their scene
/// paths under "UI Canvas/Margin".
/// </summary>
public class MatchHudController
{
    private Slider playerLifeSlider;
    private TMP_Text playerLifeText;
    private Slider opponentLifeSlider;
    private TMP_Text opponentLifeText;
    private TMP_Text energyText;
    private TMP_Text[] affinityTexts;
    private Button endTurnButton;
    private TMP_Text endTurnText;
    private GameObject cycleArea;
    private TMP_Text statusText;

    public Action OnEndTurnClicked;
    public Action OnReplaceClicked;

    public MatchHudController(Transform uiCanvas)
    {
        var margin = uiCanvas.Find("Margin");
        if (margin == null)
        {
            Debug.LogError("MatchHudController: 'Margin' not found under UI Canvas.");
            return;
        }

        playerLifeSlider = FindComponent<Slider>(margin, "Life Panel/Slider");
        playerLifeText = FindComponent<TMP_Text>(margin, "Life Panel/Slider/Text (TMP)");
        opponentLifeSlider = FindComponent<Slider>(margin, "Opponent Status Panel/Opponent Life/Slider");
        opponentLifeText = FindComponent<TMP_Text>(margin, "Opponent Status Panel/Opponent Life/Slider/Text (TMP)");
        energyText = FindComponent<TMP_Text>(margin, "Energy Panel/Radial Slider/Text (TMP)");

        var affinityPanel = margin.Find("Energy Panel/Affinity Panel");
        if (affinityPanel != null)
        {
            affinityTexts = new TMP_Text[affinityPanel.childCount];
            for (int i = 0; i < affinityPanel.childCount; i++)
            {
                var label = affinityPanel.GetChild(i).Find("Text (TMP)");
                if (label != null) affinityTexts[i] = label.GetComponent<TMP_Text>();
            }
        }

        endTurnButton = FindComponent<Button>(margin, "End Turn Button");
        if (endTurnButton != null)
        {
            endTurnText = endTurnButton.GetComponentInChildren<TMP_Text>(true);
            endTurnButton.onClick.AddListener(() => OnEndTurnClicked?.Invoke());
        }

        var cycleTransform = margin.Find("Cycle Area");
        if (cycleTransform != null)
        {
            cycleArea = cycleTransform.gameObject;
            var button = cycleArea.GetComponent<Button>();
            if (button == null) button = cycleArea.AddComponent<Button>();
            button.onClick.AddListener(() => OnReplaceClicked?.Invoke());

            var label = cycleArea.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
            {
                var text = CreateText(cycleArea.transform, "Replace Label");
                text.text = "Replace\n(+1 max energy)";
            }
            cycleArea.SetActive(false);
        }

        statusText = CreateText(margin, "Status Text");
        var statusRect = (RectTransform)statusText.transform;
        statusRect.anchorMin = new Vector2(0.25f, 0.90f);
        statusRect.anchorMax = new Vector2(0.75f, 1.00f);
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;
    }

    private static T FindComponent<T>(Transform root, string path) where T : Component
    {
        var child = root.Find(path);
        if (child == null)
        {
            Debug.LogWarning($"MatchHudController: '{path}' not found.");
            return null;
        }
        return child.GetComponent<T>();
    }

    private static TMP_Text CreateText(Transform parent, string textName)
    {
        var go = new GameObject(textName, typeof(RectTransform), typeof(TextMeshProUGUI));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var text = go.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 8f;
        text.fontSizeMax = 28f;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    public void Refresh(GameState state, int localPlayer)
    {
        var player = state.Players[localPlayer];
        var opponent = state.Players[1 - localPlayer];

        SetLife(playerLifeSlider, playerLifeText, player.Life);
        SetLife(opponentLifeSlider, opponentLifeText, opponent.Life);

        if (energyText != null) energyText.text = $"{player.Energy}/{player.MaxEnergy}";

        if (affinityTexts != null)
        {
            //Indicator sibling order follows the Archetype enum order
            var archetypes = (Archetype[])Enum.GetValues(typeof(Archetype));
            for (int i = 0; i < affinityTexts.Length && i < archetypes.Length; i++)
            {
                if (affinityTexts[i] != null)
                    affinityTexts[i].text = player.Affinity[archetypes[i]].ToString();
            }
        }
    }

    private static void SetLife(Slider slider, TMP_Text text, int life)
    {
        if (slider != null)
        {
            slider.maxValue = Mathf.Max(GameConfig.StartingLife, life);
            slider.value = Mathf.Max(0, life);
        }
        if (text != null) text.text = Mathf.Max(0, life).ToString();
    }

    public void SetEndTurnEnabled(bool enabled)
    {
        if (endTurnButton != null) endTurnButton.interactable = enabled;
    }

    public void SetEndTurnLabel(string label)
    {
        if (endTurnText != null) endTurnText.text = label;
    }

    public void ShowReplaceTarget(bool show)
    {
        if (cycleArea != null) cycleArea.SetActive(show);
    }

    public void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}
