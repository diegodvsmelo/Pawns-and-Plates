using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterSheetUI : MonoBehaviour
{
    [Header("Header Info")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI pointsAvailableText;

    [Header("Stat Rows")]
    public StatRowUI cookingRow;
    public StatRowUI serviceRow;
    public StatRowUI operationalRow;
    public StatRowUI agilityRow;

    private EmployeeData currentData;
    private System.Action onUpdateCallback;

    // MUDANÇA: Referência ao GameManager (o dono da pausa)
    private GameManager gameManager;

    private int tempPoints;
    private int tempCooking, tempService, tempOperational, tempAgility;

    void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void OpenSheet(EmployeeData data, System.Action onUpdate = null)
    {
        currentData = data;
        onUpdateCallback = onUpdate; 
        
        gameObject.SetActive(true);

        // PAUSA GLOBAL
        if (gameManager != null) gameManager.isGamePaused = true;

        tempPoints = data.skillPoints;
        tempCooking = data.cookingSkill;
        tempService = data.serviceSkill;
        tempOperational = data.operationalSkill;
        tempAgility = data.agility;

        nameText.text = data.employeeName;

        UpdateUI();
    }

    public void ModifyStat(string statName, int change)
    {
        if (change > 0 && tempPoints < change) return;

        if (change < 0)
        {
            if (statName == "cooking" && tempCooking <= currentData.cookingSkill) return;
            if (statName == "service" && tempService <= currentData.serviceSkill) return;
            if (statName == "operational" && tempOperational <= currentData.operationalSkill) return;
            if (statName == "agility" && tempAgility <= currentData.agility) return;
        }

        if (statName == "cooking") tempCooking += change;
        else if (statName == "service") tempService += change;
        else if (statName == "operational") tempOperational += change;
        else if (statName == "agility") tempAgility += change;

        tempPoints -= change;
        UpdateUI();
    }

    void UpdateUI()
    {
        pointsAvailableText.text = $"Pontos Disponíveis: {tempPoints}";
        cookingRow.UpdateVisuals(tempCooking);
        serviceRow.UpdateVisuals(tempService);
        operationalRow.UpdateVisuals(tempOperational);
        agilityRow.UpdateVisuals(tempAgility);
    }

    public void ConfirmChanges()
    {
        currentData.cookingSkill = tempCooking;
        currentData.serviceSkill = tempService;
        currentData.operationalSkill = tempOperational;
        currentData.agility = tempAgility;
        currentData.skillPoints = tempPoints;

        if (onUpdateCallback != null) onUpdateCallback.Invoke();

        // RETIRA PAUSA GLOBAL
        if (gameManager != null) gameManager.isGamePaused = false;

        gameObject.SetActive(false);
    }

    public void CloseWithoutSaving()
    {
        // RETIRA PAUSA GLOBAL
        if (gameManager != null) gameManager.isGamePaused = false;
        
        gameObject.SetActive(false);
    }
}

[System.Serializable]
public class StatRowUI
{
    public TextMeshProUGUI valueText;
    public void UpdateVisuals(float value)
    {
        valueText.text = value.ToString();
    }
}