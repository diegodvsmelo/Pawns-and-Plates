using UnityEngine;
using UnityEngine.EventSystems;

public class EmployeeSidebarDropArea : MonoBehaviour, IDropHandler
{
    [SerializeField] private Transform cardsReturnContainer;

    private void Awake()
    {
        if (cardsReturnContainer == null)
            cardsReturnContainer = transform;
    }

    public void OnDrop(PointerEventData eventData)
    {
        EmployeeCardDraggable draggable = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<EmployeeCardDraggable>()
            : null;

        if (draggable == null)
            return;

        draggable.MoveToParent(cardsReturnContainer);
        draggable.MarkDroppedOnValidTarget();
    }
}