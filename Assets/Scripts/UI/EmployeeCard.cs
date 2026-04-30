using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class EmployeeCard : MonoBehaviour, IPointerClickHandler
{
    public EmployeeData data;
    public Image backgroundImage;

    [Header("Stamina System")]
    public Slider staminaSlider;
    public float currentStamina;
    public float staminaRegen = 2f;

    [Header("Level & Upgrade")]
    public TextMeshProUGUI levelText;
    public Slider xpSlider;
    public GameObject upgradeIcon;

    // OBSERVERS
    public event Action<EmployeeCard, float> OnStaminaChanged;
    public event Action<EmployeeCard, int> OnExperienceChanged;
    public event Action<EmployeeCard, int> OnLevelChanged;
    public event Action<EmployeeCard> OnLevelUp;
    public event Action<EmployeeCard> OnSkillPointsChanged;

    private CharacterSheetUI characterSheet;

    private void Start()
    {
        characterSheet = FindFirstObjectByType<CharacterSheetUI>(FindObjectsInactive.Include);
    }

    private void Update()
    {
        if (transform.parent != null)
        {
            Slot mySlot = transform.parent.GetComponent<Slot>();

            if (mySlot != null && mySlot.isRoster)
            {
                RecoverStamina(Time.deltaTime * staminaRegen);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (characterSheet != null)
            {
                characterSheet.OpenSheet(data, UpdateLevelUI);
            }
            else
            {
                Debug.LogWarning("Painel CharacterSheetUI não encontrado!");
            }
        }
    }

    public void Setup(EmployeeData newData)
    {
        data = newData;

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (backgroundImage != null)
            backgroundImage.color = newData.cardColor;

        currentStamina = newData.maxStamina;

        UpdateStaminaUI();
        UpdateLevelUI();

        OnStaminaChanged?.Invoke(this, currentStamina);
        OnExperienceChanged?.Invoke(this, data.currentXP);
        OnLevelChanged?.Invoke(this, data.currentLevel);
    }

    public void ConsumeStamina(int amount)
    {
        currentStamina -= amount;

        if (currentStamina < 0)
            currentStamina = 0;

        UpdateStaminaUI();
        OnStaminaChanged?.Invoke(this, currentStamina);
    }

    public void RecoverStamina(float amount)
    {
        if (data == null)
            return;

        float previousStamina = currentStamina;

        currentStamina += amount;

        if (currentStamina > data.maxStamina)
            currentStamina = data.maxStamina;

        if (Mathf.Approximately(previousStamina, currentStamina))
            return;

        UpdateStaminaUI();
        OnStaminaChanged?.Invoke(this, currentStamina);
    }

    private void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = data.maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    public void AddExperience(int amount)
    {
        data.currentXP += amount;

        CheckLevelUp();

        UpdateLevelUI();

        OnExperienceChanged?.Invoke(this, data.currentXP);
        OnLevelChanged?.Invoke(this, data.currentLevel);
    }

    private void CheckLevelUp()
    {
        bool leveledUp = false;

        while (data.currentXP >= data.GetXpToNextLevel())
        {
            data.currentXP -= data.GetXpToNextLevel();
            data.currentLevel++;
            data.skillPoints += 5;

            leveledUp = true;

            Debug.Log($"LEVEL UP! {data.employeeName} nv. {data.currentLevel}. Pontos: {data.skillPoints}");

            OnLevelUp?.Invoke(this);
            OnSkillPointsChanged?.Invoke(this);
        }

        if (leveledUp)
        {
            OnLevelChanged?.Invoke(this, data.currentLevel);
        }
    }

    public void UpdateLevelUI()
    {
        if (levelText != null)
            levelText.text = $"Nv. {data.currentLevel}";

        if (xpSlider != null)
        {
            xpSlider.maxValue = data.GetXpToNextLevel();
            xpSlider.value = data.currentXP;
        }

        if (upgradeIcon != null)
        {
            bool hasPoints = data.skillPoints > 0;

            if (upgradeIcon.activeSelf != hasPoints)
            {
                upgradeIcon.SetActive(hasPoints);
            }
        }

        OnSkillPointsChanged?.Invoke(this);
    }
}