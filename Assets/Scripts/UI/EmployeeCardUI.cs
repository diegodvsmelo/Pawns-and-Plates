using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmployeeCardUI : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private Image cardBackground;

    [Header("Main Info")]
    [SerializeField] private Image profileImage;
    [SerializeField] private Image levelUpIcon;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI traitText;

    [Header("Buff / Debuff Slots")]
    [SerializeField] private Image buffIconSlot;
    [SerializeField] private Image debuffIconSlot;

    [Header("Bars")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider xpSlider;

    private EmployeeData employeeData;

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

        if (cardBackground != null)
            cardBackground.color = employeeData.cardColor;

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

        UpdateIconSlot(buffIconSlot, employeeData.buffIcon);
        UpdateIconSlot(debuffIconSlot, employeeData.debuffIcon);

        if (staminaSlider != null)
            staminaSlider.value = employeeData.GetStaminaPercent();

        if (xpSlider != null)
            xpSlider.value = employeeData.GetXpPercent();
    }

    private void UpdateIconSlot(Image slot, Sprite icon)
    {
        if (slot == null)
            return;

        bool hasIcon = icon != null;

        slot.gameObject.SetActive(hasIcon);
        slot.sprite = icon;
    }
}