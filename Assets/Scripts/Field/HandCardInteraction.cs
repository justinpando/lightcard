using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Pointer behavior for one card in the match hand: hover raises it above its
/// neighbors, dragging carries it toward the board or the Attune (energy)
/// area. Pure input relay — HandViewController owns the visuals and
/// MatchContext owns the game logic.
/// </summary>
public class HandCardInteraction : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int Index;
    public Action<int, bool> OnHoverChanged;
    public Func<int, bool> OnDragStart;
    public Action<int, Vector2> OnDropped;

    private bool dragging;
    private Vector3 dragStartPosition;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!dragging) OnHoverChanged?.Invoke(Index, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!dragging) OnHoverChanged?.Invoke(Index, false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (OnDragStart == null || !OnDragStart(Index)) return;
        dragging = true;
        dragStartPosition = transform.position;
        OnHoverChanged?.Invoke(Index, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        var rect = (RectTransform)transform.parent;
        Vector3 world;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out world))
            transform.position = world;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        transform.position = dragStartPosition;
        OnHoverChanged?.Invoke(Index, false);
        OnDropped?.Invoke(Index, eventData.position);
    }
}
