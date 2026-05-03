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

    [Header("Skills (0-100)")]
    [Range(0, 100)] public int cookingSkill;
    [Range(0, 100)] public int serviceSkill;
    [Range(0, 100)] public int operationalSkill;

    [Header("Physical Stats")]
    [Range(0, 100)] public int agility;
    [Range(0, 100)] public int maxStamina = 100;
    [Range(0, 100)] public int currentStamina = 100;

    [Header("Progression")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int skillPoints = 0;

    [Header("Trait")]
    public bool hasTrait;
    public string traitName;
    public Sprite traitIcon;

    [Header("Buff / Debuff UI")]
    public Sprite buffIcon;
    public Sprite debuffIcon;

    [Header("Contract Info")]
    public int baseSalary = 1;

    public bool HasUnspentSkillPoints()
    {
        return skillPoints > 0;
    }

    public bool HasTrait()
    {
        return hasTrait && !string.IsNullOrWhiteSpace(traitName);
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