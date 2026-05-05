using UnityEngine;
using UnityEngine.EventSystems;

public class EmployeeTaskSlot : MonoBehaviour, IDropHandler
{
    [Header("Rules")]
    [SerializeField] private bool allowReplace = false;

    public EmployeeCardUI CurrentCard { get; private set; }

    public bool IsEmpty => CurrentCard == null;

    public void OnDrop(PointerEventData eventData)
    {
        EmployeeCardDraggable draggable = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<EmployeeCardDraggable>()
            : null;

        if (draggable == null || draggable.EmployeeCardUI == null)
            return;

        if (!allowReplace && !IsEmpty)
        {
            draggable.ReturnToOriginalParent();
            return;
        }

        if (!IsEmpty && allowReplace)
        {
            EmployeeCardDraggable currentDraggable = CurrentCard.GetComponent<EmployeeCardDraggable>();

            if (currentDraggable != null)
                currentDraggable.ReturnToOriginalParent();
        }

        PlaceCard(draggable);
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
    }

    public void ClearSlot()
    {
        CurrentCard = null;
    }
}