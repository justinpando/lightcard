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

    private TMP_Text historyText;
    private GameObject historyView;
    private GameObject openHistoryButton;
    private GameObject closeHistoryButton;
    private readonly System.Collections.Generic.Queue<string> historyLines = new System.Collections.Generic.Queue<string>();
    private const int HistoryCapacity = 14;

    private GameObject endPanel;
    private TMP_Text endTitle;
    private Button rematchButton;
    private Button menuButton;

    public Action OnEndTurnClicked;
    public Action OnReplaceClicked;
    public Action OnRematchClicked;
    public Action OnMenuClicked;

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

        BuildHistoryLog(margin);
        BuildEndPanel(uiCanvas);
    }

    //---- Match history log (drives the scene's History Panel art) ----

    private void BuildHistoryLog(Transform margin)
    {
        var panel = margin.Find("History Panel");
        if (panel == null) return;

        var open = panel.Find("Open History Button");
        var close = panel.Find("Close History Button");
        var view = panel.Find("History");
        if (view == null) return;

        historyView = view.gameObject;
        openHistoryButton = open != null ? open.gameObject : null;
        closeHistoryButton = close != null ? close.gameObject : null;

        historyText = CreateText(view, "History Text");
        historyText.alignment = TextAlignmentOptions.TopLeft;
        historyText.enableAutoSizing = false;
        historyText.fontSize = 13f;
        historyText.margin = new Vector4(8, 8, 8, 8);
        historyText.overflowMode = TextOverflowModes.Truncate;

        if (openHistoryButton != null)
        {
            var button = openHistoryButton.GetComponent<Button>();
            if (button != null) button.onClick.AddListener(() => SetHistoryOpen(true));
        }
        if (closeHistoryButton != null)
        {
            var button = closeHistoryButton.GetComponent<Button>();
            if (button != null) button.onClick.AddListener(() => SetHistoryOpen(false));
        }

        SetHistoryOpen(false);
    }

    private void SetHistoryOpen(bool openState)
    {
        if (historyView != null) historyView.SetActive(openState);
        if (openHistoryButton != null) openHistoryButton.SetActive(!openState);
        if (closeHistoryButton != null) closeHistoryButton.SetActive(openState);
    }

    public void AddHistoryLine(string line)
    {
        if (historyText == null) return;
        historyLines.Enqueue(line);
        while (historyLines.Count > HistoryCapacity) historyLines.Dequeue();
        historyText.text = string.Join("\n", historyLines);
    }

    //---- Match end panel ----

    private void BuildEndPanel(Transform uiCanvas)
    {
        endPanel = new GameObject("Match End Panel", typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)endPanel.transform;
        rect.SetParent(uiCanvas, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var dim = endPanel.GetComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.72f);
        dim.raycastTarget = true; //blocks board and hand input underneath

        endTitle = CreateText(endPanel.transform, "End Title");
        var titleRect = (RectTransform)endTitle.transform;
        titleRect.anchorMin = new Vector2(0.2f, 0.55f);
        titleRect.anchorMax = new Vector2(0.8f, 0.80f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        endTitle.fontSizeMax = 64f;

        rematchButton = CreateButton(endPanel.transform, "Rematch", new Vector2(0.30f, 0.32f), new Vector2(0.48f, 0.42f));
        rematchButton.onClick.AddListener(() => OnRematchClicked?.Invoke());
        menuButton = CreateButton(endPanel.transform, "Main Menu", new Vector2(0.52f, 0.32f), new Vector2(0.70f, 0.42f));
        menuButton.onClick.AddListener(() => OnMenuClicked?.Invoke());

        endPanel.SetActive(false);
    }

    private static Button CreateButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject($"{label} Button", typeof(RectTransform), typeof(Image), typeof(Button));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(0.16f, 0.18f, 0.24f, 0.95f);

        var text = CreateText(go.transform, "Label");
        text.text = label;
        text.fontSizeMax = 24f;
        return go.GetComponent<Button>();
    }

    public void ShowEndPanel(bool victory)
    {
        if (endPanel == null) return;
        endTitle.text = victory ? "Victory!" : "Defeat";
        endTitle.color = victory ? new Color(0.95f, 0.85f, 0.35f) : new Color(0.75f, 0.45f, 0.45f);
        endPanel.SetActive(true);
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
