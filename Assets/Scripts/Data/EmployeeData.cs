using UnityEngine;

[CreateAssetMenu(fileName = "New Employee", menuName = "Restaurant/Employee")]
public class EmployeeData : ScriptableObject
{
    [Header("Identity")]
    public string employeeName;
    [TextArea] public string description;
    public Sprite profilePicture;

    [Header("Visual Settings")]
    public Color cardColor = Color.white;

    [Header("Skills (0-10)")]
    [Range(0, 10)] public int cookingSkill;
    [Range(0, 10)] public int serviceSkill;
    [Range(0, 10)] public int operationalSkill;

    [Header("Physical Stats")]
    [Range(0, 10)] public int agility;
    [Min(1)] public int maxStamina = 100;
    [Min(0)] public int currentStamina = 100;

    [Header("Progression")]
    [Min(1)] public int currentLevel = 1;
    [Min(0)] public int currentXP = 0;
    [Min(0)] public int skillPoints = 0;

    [Header("Trait")]
    public bool hasTrait;
    public string traitName;

    [Header("Status Icons")]
    [Tooltip("Pode ser buff, debuff ou qualquer outro status visual.")]
    public Sprite statusIconA;

    [Tooltip("Pode ser buff, debuff ou qualquer outro status visual.")]
    public Sprite statusIconB;

    [Header("Contract Info")]
    [Min(0)] public int baseSalary = 1;

    private void OnValidate()
    {
        if (maxStamina < 1)
            maxStamina = 1;

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (currentLevel < 1)
            currentLevel = 1;

        if (currentXP < 0)
            currentXP = 0;

        if (skillPoints < 0)
            skillPoints = 0;

        if (baseSalary < 0)
            baseSalary = 0;

        if (!hasTrait)
            traitName = "";
    }

    public bool HasUnspentSkillPoints()
    {
        return skillPoints > 0;
    }

    public bool HasTrait()
    {
        return hasTrait && !string.IsNullOrWhiteSpace(traitName);
    }

    public bool HasStatusA()
    {
        return statusIconA != null;
    }

    public bool HasStatusB()
    {
        return statusIconB != null;
    }

    public int GetDailyCost()
    {
        return baseSalary + currentLevel;
    }

    public int GetXpToNextLevel()
    {
        return currentLevel * 100;
    }

    public float GetStaminaPercent()
    {
        if (maxStamina <= 0)
            return 0f;

        return Mathf.Clamp01((float)currentStamina / maxStamina);
    }

    public float GetXpPercent()
    {
        int xpToNextLevel = GetXpToNextLevel();

        if (xpToNextLevel <= 0)
            return 0f;

        return Mathf.Clamp01((float)currentXP / xpToNextLevel);
    }
}