using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmployeeCardUI : MonoBehaviour
{
    [Header("Main Info")]
    [SerializeField] private Image profileImage;
    [SerializeField] private Image levelUpIcon;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI traitText;

    [Header("Status Slots")]
    [SerializeField] private Image statusIconSlotA;
    [SerializeField] private Image statusIconSlotB;

    [Header("Bars")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider xpSlider;

    [Header("Availability Overlay")]
    [SerializeField] private CanvasGroup availabilityOverlayGroup;
    [SerializeField] private TextMeshProUGUI availabilityText;
    [SerializeField] private float overlayMaxAlpha = 0.5f;

    [Header("Expanded Stats - Optional")]
    [SerializeField] private AttributeSquaresUI cookingSquares;
    [SerializeField] private AttributeSquaresUI serviceSquares;
    [SerializeField] private AttributeSquaresUI operationalSquares;
    [SerializeField] private AttributeSquaresUI agilitySquares;

    private EmployeeData employeeData;

    public EmployeeData Data => employeeData;

    public void Setup(EmployeeData data)
    {
        UnsubscribeFromCurrentData();

        employeeData = data;

        SubscribeToCurrentData();
        Refresh();
    }

    private void OnDestroy()
    {
        UnsubscribeFromCurrentData();
    }

    private void SubscribeToCurrentData()
    {
        if (employeeData != null)
            employeeData.OnDataChanged += Refresh;
    }

    private void UnsubscribeFromCurrentData()
    {
        if (employeeData != null)
            employeeData.OnDataChanged -= Refresh;
    }

    public void Refresh()
    {
        if (employeeData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (profileImage != null)
            profileImage.sprite = employeeData.profilePicture;

        if (nameText != null)
            nameText.text = employeeData.employeeName;

        if (levelText != null)
            levelText.text = $"Lvl. {employeeData.currentLevel}";

        if (traitText != null)
        {
            bool hasTrait = employeeData.HasTrait();
            traitText.gameObject.SetActive(hasTrait);
            traitText.text = hasTrait ? employeeData.traitName : "";
        }

        if (levelUpIcon != null)
            levelUpIcon.gameObject.SetActive(employeeData.HasUnspentSkillPoints());

        UpdateStatusIcon(statusIconSlotA, employeeData.statusIconA);
        UpdateStatusIcon(statusIconSlotB, employeeData.statusIconB);

        if (staminaSlider != null)
            staminaSlider.value = employeeData.GetStaminaPercent();

        if (xpSlider != null)
            xpSlider.value = employeeData.GetXpPercent();

        UpdateAttributeSquares();
        UpdateAvailabilityOverlay();
    }

    private void UpdateAttributeSquares()
    {
        if (employeeData == null)
            return;

        if (cookingSquares != null)
            cookingSquares.UpdateValue(employeeData.cookingSkill);

        if (serviceSquares != null)
            serviceSquares.UpdateValue(employeeData.serviceSkill);

        if (operationalSquares != null)
            operationalSquares.UpdateValue(employeeData.operationalSkill);

        if (agilitySquares != null)
            agilitySquares.UpdateValue(employeeData.agility);
    }

    private void UpdateStatusIcon(Image slot, Sprite icon)
    {
        if (slot == null)
            return;

        bool hasIcon = icon != null;

        slot.gameObject.SetActive(hasIcon);
        slot.sprite = icon;
    }

    private void UpdateAvailabilityOverlay()
    {
        if (availabilityOverlayGroup == null)
            return;

        string overlayLabel = "";
        float targetAlpha = 0f;
        bool shouldShowOverlay = false;

        if (employeeData != null)
        {
            if (employeeData.IsOccupied())
            {
                overlayLabel = "OCCUPIED";
                targetAlpha = overlayMaxAlpha;
                shouldShowOverlay = true;
            }
            else if (employeeData.IsResting())
            {
                overlayLabel = "RESTING";
                targetAlpha = overlayMaxAlpha;
                shouldShowOverlay = true;
            }
        }

        if (!shouldShowOverlay)
        {
            HideOverlayInstant();
            return;
        }

        if (availabilityText != null)
        {
            availabilityText.text = overlayLabel;
            availabilityText.gameObject.SetActive(true);
        }

        if (!availabilityOverlayGroup.gameObject.activeSelf)
            availabilityOverlayGroup.gameObject.SetActive(true);

        availabilityOverlayGroup.alpha = targetAlpha;
        availabilityOverlayGroup.blocksRaycasts = true;
        availabilityOverlayGroup.interactable = false;
    }

    private void HideOverlayInstant()
    {
        if (availabilityOverlayGroup == null)
            return;

        availabilityOverlayGroup.alpha = 0f;
        availabilityOverlayGroup.blocksRaycasts = false;
        availabilityOverlayGroup.interactable = false;

        if (availabilityText != null)
        {
            availabilityText.text = "";
            availabilityText.gameObject.SetActive(false);
        }

        availabilityOverlayGroup.gameObject.SetActive(false);
    }
}