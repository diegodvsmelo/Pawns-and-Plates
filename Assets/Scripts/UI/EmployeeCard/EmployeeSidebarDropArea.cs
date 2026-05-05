using UnityEngine;
using UnityEngine.EventSystems;

public class EmployeeSidebarDropArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        EmployeeCardDraggable draggable = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<EmployeeCardDraggable>()
            : null;

        if (draggable == null)
            return;

        RectTransform cardRect = draggable.GetComponent<RectTransform>();

        draggable.transform.SetParent(transform, false);

        if (cardRect != null)
            cardRect.anchoredPosition = Vector2.zero;

        draggable.MarkDroppedOnValidTarget();
    }
}