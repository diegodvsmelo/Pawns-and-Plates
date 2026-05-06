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

        EmployeeTaskSlot originSlot = incomingDraggable.OriginalParent != null
            ? incomingDraggable.OriginalParent.GetComponent<EmployeeTaskSlot>()
            : null;

        // Caso raro: soltou no mesmo slot de onde saiu
        if (CurrentCard == incomingCard)
        {
            AssignCardToThisSlot(incomingDraggable, notify: true);
            return;
        }

        if (IsEmpty)
        {
            AssignCardToThisSlot(incomingDraggable, notify: true);
            return;
        }

        SwapCards(incomingDraggable, originSlot);
    }

    private void SwapCards(EmployeeCardDraggable incomingDraggable, EmployeeTaskSlot originSlot)
    {
        EmployeeCardUI oldCard = CurrentCard;

        if (oldCard == null)
        {
            AssignCardToThisSlot(incomingDraggable, notify: true);
            return;
        }

        EmployeeCardDraggable oldDraggable = oldCard.GetComponent<EmployeeCardDraggable>();

        if (oldDraggable == null)
        {
            AssignCardToThisSlot(incomingDraggable, notify: true);
            return;
        }

        Transform incomingOriginalParent = incomingDraggable.OriginalParent;
        int incomingOriginalSiblingIndex = incomingDraggable.OriginalSiblingIndex;

        // 1) O novo card entra neste slot
        AssignCardToThisSlot(incomingDraggable, notify: true);

        // 2) O card antigo volta para a origem do card novo
        if (originSlot != null && originSlot != this)
        {
            originSlot.AssignCardToThisSlot(oldDraggable, notify: true);
        }
        else
        {
            oldDraggable.MoveToParent(incomingOriginalParent, incomingOriginalSiblingIndex);
        }
    }

    private void AssignCardToThisSlot(EmployeeCardDraggable draggable, bool notify)
    {
        if (draggable == null || draggable.EmployeeCardUI == null)
            return;

        CurrentCard = draggable.EmployeeCardUI;

        draggable.MoveToParent(transform);
        draggable.MarkDroppedOnValidTarget();

        if (notify)
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
        if (CurrentCard == null)
            return;

        CurrentCard = null;
        OnSlotChanged?.Invoke();
    }
}