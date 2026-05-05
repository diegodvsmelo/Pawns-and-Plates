using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class EmployeeTaskSlot : MonoBehaviour, IDropHandler
{
    public event Action OnSlotChanged;

    public EmployeeCardUI CurrentCard { get; private set; }

    public bool IsEmpty => CurrentCard == null;

    public void OnDrop(PointerEventData eventData)
    {
        EmployeeCardDraggable incomingDraggable = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<EmployeeCardDraggable>()
            : null;

        if (incomingDraggable == null || incomingDraggable.EmployeeCardUI == null)
            return;

        EmployeeCardUI incomingCard = incomingDraggable.EmployeeCardUI;

        if (CurrentCard == incomingCard)
        {
            incomingDraggable.MarkDroppedOnValidTarget();
            PlaceCard(incomingDraggable);
            return;
        }

        if (IsEmpty)
        {
            PlaceCard(incomingDraggable);
            return;
        }

        SwapCards(incomingDraggable);
    }

    private void PlaceCard(EmployeeCardDraggable draggable)
    {
        CurrentCard = draggable.EmployeeCardUI;

        RectTransform cardRect = draggable.GetComponent<RectTransform>();

        draggable.transform.SetParent(transform, false);

        if (cardRect != null)
        {
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;
        }

        draggable.MarkDroppedOnValidTarget();

        OnSlotChanged?.Invoke();
    }

    private void SwapCards(EmployeeCardDraggable incomingDraggable)
    {
        EmployeeCardUI oldCard = CurrentCard;

        if (oldCard == null)
        {
            PlaceCard(incomingDraggable);
            return;
        }

        EmployeeCardDraggable oldDraggable = oldCard.GetComponent<EmployeeCardDraggable>();

        if (oldDraggable == null)
        {
            PlaceCard(incomingDraggable);
            return;
        }

        Transform incomingOriginalParent = incomingDraggable.OriginalParent;
        int incomingOriginalSiblingIndex = incomingDraggable.OriginalSiblingIndex;

        PlaceCard(incomingDraggable);

        oldDraggable.MoveToParent(incomingOriginalParent, incomingOriginalSiblingIndex);

        OnSlotChanged?.Invoke();
    }

    public void ClearSlot(EmployeeCardUI card)
    {
        if (CurrentCard != card)
            return;

        CurrentCard = null;
        OnSlotChanged?.Invoke();
    }

    public void ForceClearSlot()
    {
        CurrentCard = null;
        OnSlotChanged?.Invoke();
    }
}