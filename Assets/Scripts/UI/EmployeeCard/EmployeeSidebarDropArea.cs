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

        draggable.MoveToParent(transform);
        draggable.MarkDroppedOnValidTarget();
    }
}