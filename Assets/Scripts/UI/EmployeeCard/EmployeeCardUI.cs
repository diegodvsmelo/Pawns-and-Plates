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

    [Header("Expanded Stats - Optional")]
    [SerializeField] private AttributeSquaresUI cookingSquares;
    [SerializeField] private AttributeSquaresUI serviceSquares;
    [SerializeField] private AttributeSquaresUI operationalSquares;
    [SerializeField] private AttributeSquaresUI agilitySquares;

    private EmployeeData employeeData;

    public EmployeeData Data => employeeData;

    public void Setup(EmployeeData data)
    {
        employeeData = data;
        Refresh();
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
    }

    private void UpdateAttributeSquares()
    {
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
}