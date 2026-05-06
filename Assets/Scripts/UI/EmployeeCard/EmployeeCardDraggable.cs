using System.Collections;
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
    private EmployeeCardVisuals visuals;

    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector2 originalAnchoredPosition;

    private bool droppedOnValidTarget;
    private float draggedDistance;
    private float lastDragEndTime;
    private Coroutine moveRoutine;

    public EmployeeCardUI EmployeeCardUI { get; private set; }

    public Transform OriginalParent => originalParent;
    public int OriginalSiblingIndex => originalSiblingIndex;
    public bool IsDragging { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        EmployeeCardUI = GetComponent<EmployeeCardUI>();
        visuals = GetComponent<EmployeeCardVisuals>();

        rootCanvas = GetComponentInParent<Canvas>();

        if (dragLayer == null)
        {
            GameObject dragLayerObj = GameObject.Find("DragLayer");

            if (dragLayerObj != null)
                dragLayer = dragLayerObj.transform;
        }

        if (dragLayer == null && rootCanvas != null)
            dragLayer = rootCanvas.transform;
    }

    public bool WasRecentlyDragged(float graceTime = 0.12f)
    {
        return Time.unscaledTime - lastDragEndTime <= graceTime;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dragLayer == null)
        {
            Debug.LogWarning("EmployeeCardDraggable: DragLayer não encontrado.");
            return;
        }

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        IsDragging = true;
        draggedDistance = 0f;
        droppedOnValidTarget = false;

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalAnchoredPosition = rectTransform.anchoredPosition;

        EmployeeTaskSlot originalTaskSlot = originalParent != null
            ? originalParent.GetComponent<EmployeeTaskSlot>()
            : null;

        if (originalTaskSlot != null)
            originalTaskSlot.ClearSlot(EmployeeCardUI);

        transform.SetParent(dragLayer, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;

        visuals?.SetDragging(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDragging || rootCanvas == null)
            return;

        draggedDistance += eventData.delta.magnitude;
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        visuals?.SetDragging(false);

        if (draggedDistance > 6f)
            lastDragEndTime = Time.unscaledTime;

        if (!droppedOnValidTarget)
            ReturnToOriginalParent();
    }

    public void MarkDroppedOnValidTarget()
    {
        droppedOnValidTarget = true;
    }

    public void ReturnToOriginalParent()
    {
        MoveToParent(originalParent, originalSiblingIndex);
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }

    public void MoveToParent(Transform targetParent, int siblingIndex = -1)
    {
        if (targetParent == null)
            return;

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        transform.SetParent(targetParent, false);

        if (siblingIndex >= 0 && siblingIndex < targetParent.childCount)
            transform.SetSiblingIndex(siblingIndex);

        ResetRectTransformToCenter();
    }

    public void MoveToParentAnimated(Transform targetParent, float duration = 0.14f, int siblingIndex = -1)
    {
        if (targetParent == null)
            return;

        if (duration <= 0f)
        {
            MoveToParent(targetParent, siblingIndex);
            return;
        }

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveToParentAnimatedRoutine(targetParent, siblingIndex, duration));
    }

    private IEnumerator MoveToParentAnimatedRoutine(Transform targetParent, int siblingIndex, float duration)
    {
        Transform animationParent = dragLayer != null
            ? dragLayer
            : (rootCanvas != null ? rootCanvas.transform : targetParent);

        Vector3 startWorldPosition = rectTransform.position;
        Vector3 targetWorldPosition = GetTargetWorldPosition(targetParent);

        transform.SetParent(animationParent, true);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);

            rectTransform.position = Vector3.Lerp(startWorldPosition, targetWorldPosition, t);

            yield return null;
        }

        MoveToParent(targetParent, siblingIndex);
        moveRoutine = null;
    }

    private Vector3 GetTargetWorldPosition(Transform targetParent)
    {
        RectTransform targetRect = targetParent as RectTransform;

        if (targetRect == null)
            return targetParent.position;

        return targetRect.TransformPoint(targetRect.rect.center);
    }

    private void ResetRectTransformToCenter()
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}