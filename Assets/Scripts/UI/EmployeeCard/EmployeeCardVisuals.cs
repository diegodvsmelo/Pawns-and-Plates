using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class EmployeeCardVisuals : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Scale States")]
    [SerializeField] private float idleScale = 1f;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float draggingScale = 0.95f;

    [Header("Pulse")]
    [SerializeField] private float pulseAmplitude = 0.035f;
    [SerializeField] private float pulseSpeed = 6f;

    [Header("Smoothing")]
    [SerializeField] private float scaleLerpSpeed = 14f;

    private RectTransform rectTransform;
    private EmployeeCardUI cardUI;
    private EmployeeCardDraggable draggable;
    private TaskTeamSelectionUI ownerUI;

    private bool isHovered;
    private bool isSelected;
    private bool isDragging;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cardUI = GetComponent<EmployeeCardUI>();
        draggable = GetComponent<EmployeeCardDraggable>();
        ResolveOwner();
    }

    private void Update()
    {
        float baseScale = idleScale;

        if (isDragging)
            baseScale = draggingScale;
        else if (isHovered)
            baseScale = hoverScale;

        float pulseOffset = 0f;

        if (isSelected && !isDragging)
            pulseOffset = Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmplitude;

        float targetScale = baseScale + pulseOffset;
        float currentScale = rectTransform.localScale.x;
        float nextScale = Mathf.Lerp(currentScale, targetScale, Time.unscaledDeltaTime * scaleLerpSpeed);

        rectTransform.localScale = Vector3.one * nextScale;
    }

    private void ResolveOwner()
    {
        if (ownerUI == null)
            ownerUI = GetComponentInParent<TaskTeamSelectionUI>(true);
    }

    public void SetSelected(bool value)
    {
        isSelected = value;
    }

    public void SetDragging(bool value)
    {
        isDragging = value;

        if (isDragging)
            isHovered = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging)
            isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cardUI == null)
            return;

        if (draggable != null && (draggable.IsDragging || draggable.WasRecentlyDragged()))
            return;

        ResolveOwner();

        if (ownerUI != null)
            ownerUI.OnEmployeeCardClicked(cardUI);
    }
}