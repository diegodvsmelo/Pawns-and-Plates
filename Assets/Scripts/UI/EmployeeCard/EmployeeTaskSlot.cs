using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class EmployeeTaskSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Animation")]
    [SerializeField] private float placementAnimationDuration = 0.10f;
    [SerializeField] private float swapAnimationDuration = 0.14f;

    public event Action OnSlotChanged;

    public EmployeeCardUI CurrentCard { get; private set; }
    public bool IsEmpty => CurrentCard == null;

    private TaskTeamSelectionUI ownerUI;

    private void Awake()
    {
        ownerUI = GetComponentInParent<TaskTeamSelectionUI>(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ownerUI?.OnTaskSlotClicked(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        EmployeeCardDraggable incomingDraggable = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<EmployeeCardDraggable>()
            : null;

        if (incomingDraggable == null || incomingDraggable.EmployeeCardUI == null)
            return;

        TryReceiveCard(incomingDraggable, animate: true);
    }

    public bool TryReceiveCardByClick(EmployeeCardUI card)
    {
        if (card == null || card.Data == null)
            return false;

        if (!card.Data.CanBeAssignedToTask())
            return false;

        EmployeeCardDraggable draggable = card.GetComponent<EmployeeCardDraggable>();

        if (draggable == null)
            return false;

        return TryReceiveCard(draggable, animate: true);
    }

    private bool TryReceiveCard(EmployeeCardDraggable incomingDraggable, bool animate)
    {
        if (incomingDraggable == null || incomingDraggable.EmployeeCardUI == null)
            return false;

        EmployeeCardUI incomingCard = incomingDraggable.EmployeeCardUI;

        if (incomingCard.Data == null || !incomingCard.Data.CanBeAssignedToTask())
            return false;

        ResolveSource(
            incomingDraggable,
            out Transform sourceParent,
            out int sourceSiblingIndex,
            out EmployeeTaskSlot sourceSlot
        );

        if (sourceSlot == this && CurrentCard == incomingCard)
            return false;

        if (sourceSlot != null && sourceSlot != this && sourceSlot.CurrentCard == incomingCard)
            sourceSlot.ClearSlot(incomingCard);

        if (IsEmpty)
        {
            AssignCardToThisSlot(
                incomingDraggable,
                animate ? placementAnimationDuration : 0f,
                notify: true
            );

            return true;
        }

        return SwapCards(
            incomingDraggable,
            sourceParent,
            sourceSiblingIndex,
            sourceSlot,
            animate
        );
    }

    private bool SwapCards(
        EmployeeCardDraggable incomingDraggable,
        Transform sourceParent,
        int sourceSiblingIndex,
        EmployeeTaskSlot sourceSlot,
        bool animate)
    {
        EmployeeCardUI oldCard = CurrentCard;

        if (oldCard == null)
        {
            AssignCardToThisSlot(
                incomingDraggable,
                animate ? placementAnimationDuration : 0f,
                notify: true
            );

            return true;
        }

        EmployeeCardDraggable oldDraggable = oldCard.GetComponent<EmployeeCardDraggable>();

        if (oldDraggable == null)
        {
            AssignCardToThisSlot(
                incomingDraggable,
                animate ? placementAnimationDuration : 0f,
                notify: true
            );

            return true;
        }

        float duration = animate ? swapAnimationDuration : 0f;

        AssignCardToThisSlot(incomingDraggable, duration, notify: true);

        if (sourceSlot != null && sourceSlot != this)
        {
            sourceSlot.AssignCardToThisSlot(oldDraggable, duration, notify: true);
        }
        else
        {
            if (duration > 0f)
                oldDraggable.MoveToParentAnimated(sourceParent, duration, sourceSiblingIndex);
            else
                oldDraggable.MoveToParent(sourceParent, sourceSiblingIndex);
        }

        return true;
    }

    private void AssignCardToThisSlot(EmployeeCardDraggable draggable, float animationDuration, bool notify)
    {
        if (draggable == null || draggable.EmployeeCardUI == null)
            return;

        if (draggable.EmployeeCardUI.Data == null || !draggable.EmployeeCardUI.Data.CanBeAssignedToTask())
            return;

        CurrentCard = draggable.EmployeeCardUI;

        if (animationDuration > 0f)
            draggable.MoveToParentAnimated(transform, animationDuration);
        else
            draggable.MoveToParent(transform);

        draggable.MarkDroppedOnValidTarget();

        if (notify)
            OnSlotChanged?.Invoke();
    }

    private void ResolveSource(
        EmployeeCardDraggable draggable,
        out Transform sourceParent,
        out int sourceSiblingIndex,
        out EmployeeTaskSlot sourceSlot)
    {
        if (draggable.IsDragging)
        {
            sourceParent = draggable.OriginalParent;
            sourceSiblingIndex = draggable.OriginalSiblingIndex;
        }
        else
        {
            sourceParent = draggable.transform.parent;
            sourceSiblingIndex = draggable.transform.GetSiblingIndex();
        }

        sourceSlot = sourceParent != null
            ? sourceParent.GetComponent<EmployeeTaskSlot>()
            : null;
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