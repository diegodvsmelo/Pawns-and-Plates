using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class EmployeeCardDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Settings")]
    [SerializeField] private Transform dragLayer;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector2 originalAnchoredPosition;
    private EmployeeTaskSlot originalTaskSlot;
    private bool droppedOnValidTarget;

    public EmployeeCardUI EmployeeCardUI { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        EmployeeCardUI = GetComponent<EmployeeCardUI>();

        rootCanvas = GetComponentInParent<Canvas>();

        if (dragLayer == null)
        {
            GameObject dragLayerObj = GameObject.Find("DragLayer");

            if (dragLayerObj != null)
                dragLayer = dragLayerObj.transform;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dragLayer == null)
        {
            Debug.LogWarning("EmployeeCardDraggable: DragLayer não encontrado.");
            return;
        }

        droppedOnValidTarget = false;

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalAnchoredPosition = rectTransform.anchoredPosition;

        originalTaskSlot = originalParent != null ? originalParent.GetComponent<EmployeeTaskSlot>() : null;

        if (originalTaskSlot != null)
            originalTaskSlot.ClearSlot();

        transform.SetParent(dragLayer, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rootCanvas == null)
            return;

        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (!droppedOnValidTarget)
            ReturnToOriginalParent();
    }

    public void MarkDroppedOnValidTarget()
    {
        droppedOnValidTarget = true;
    }

    public void ReturnToOriginalParent()
    {
        if (originalParent == null)
            return;

        transform.SetParent(originalParent, false);
        transform.SetSiblingIndex(originalSiblingIndex);
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }
}