using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LightCard.Core;

/// <summary>
/// Maps the 18 SpaceViews placed in the Field scene onto engine coordinates and
/// renders GameState onto them. Layout convention (from the scene art): the two
/// 3x3 halves sit side by side along world x, so engine rows (y 0..5) run left
/// to right — the local player owns the left half — and engine lanes (x 0..2)
/// run along world z from far to near.
/// </summary>
public class FieldViewController
{
    private readonly SpaceView[,] spaces = new SpaceView[GameConfig.Lanes, GameConfig.Rows];
    private readonly Func<string, Sprite> artResolver;

    public FieldViewController(Action<SpaceView> onSpaceClicked, Func<string, Sprite> artResolver = null)
    {
        this.artResolver = artResolver;
        var all = UnityEngine.Object.FindObjectsByType<SpaceView>(FindObjectsSortMode.None);
        if (all.Length != GameConfig.Lanes * GameConfig.Rows)
            Debug.LogWarning($"FieldViewController: expected {GameConfig.Lanes * GameConfig.Rows} SpaceViews, found {all.Length}.");

        //Row index (engine y) by world x ascending; lane index (engine x) by world z descending
        var columnCenters = all.Select(s => s.transform.position.x).Distinct()
            .GroupBy(x => Mathf.RoundToInt(x * 10f)).Select(g => g.First())
            .OrderBy(x => x).ToList();
        var laneCenters = all.Select(s => s.transform.position.z).Distinct()
            .GroupBy(z => Mathf.RoundToInt(z * 10f)).Select(g => g.First())
            .OrderByDescending(z => z).ToList();

        foreach (var space in all)
        {
            int y = ClosestIndex(columnCenters, space.transform.position.x);
            int x = ClosestIndex(laneCenters, space.transform.position.z);

            if (x < 0 || x >= GameConfig.Lanes || y < 0 || y >= GameConfig.Rows || spaces[x, y] != null)
            {
                Debug.LogWarning($"FieldViewController: could not place SpaceView '{space.name}' at ({x},{y}).");
                continue;
            }

            space.SetCoordinates(x, y);
            space.OnClicked = onSpaceClicked;
            spaces[x, y] = space;
        }
    }

    private static int ClosestIndex(List<float> centers, float value)
    {
        int best = -1;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < centers.Count; i++)
        {
            float distance = Mathf.Abs(centers[i] - value);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }
        return best;
    }

    public SpaceView GetSpace(int x, int y) =>
        GameState.InBounds(x, y) ? spaces[x, y] : null;

    public void Refresh(GameState state, int localPlayer)
    {
        for (int x = 0; x < GameConfig.Lanes; x++)
        {
            for (int y = 0; y < GameConfig.Rows; y++)
            {
                var space = spaces[x, y];
                if (space == null) continue;

                var unit = state.GetUnitAt(x, y);
                if (unit != null) space.ShowUnit(state, unit, localPlayer, artResolver?.Invoke(unit.CardId));
                else space.ClearUnit();

                space.SetSpaceEffect(state.SpaceEffects[x, y]);
            }
        }
    }

    public void ClearHighlights()
    {
        foreach (var space in spaces)
            if (space != null) space.SetHighlight(SpaceView.Highlight.None);
    }

    public void HighlightSpaces(IEnumerable<(int x, int y)> targets, SpaceView.Highlight highlight)
    {
        foreach (var (x, y) in targets)
        {
            var space = GetSpace(x, y);
            if (space != null) space.SetHighlight(highlight);
        }
    }
}
