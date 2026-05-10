using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TaskHintLine
{
    [TextArea(2, 3)] public string prefixText;
    public string highlightedText;
    [TextArea(2, 3)] public string suffixText;

    public EmployeeSkillType relatedSkill;
}

[CreateAssetMenu(fileName = "New Task", menuName = "Restaurant/Task")]
public class TaskData : ScriptableObject
{
    [Header("Identity")]
    public string taskName;

    [TextArea(3, 6)]
    public string description;

    [Header("Classification")]
    public TaskType taskType;
    public TaskOutcomeFlow outcomeFlow = TaskOutcomeFlow.None;

    [Header("Spawn Rules")]
    public bool canSpawnRandomly = true;

    [Header("Execution")]
    [Min(1)] public int maxSlots = 1;
    [Min(0f)] public float expirationTime = 20f;
    [Min(0f)] public float executionTime = 5f;

    [Header("Difficulty")]
    [Min(1f)] public float difficultyPoints = 5f;
    public List<TaskRequirement> requirements = new();

    [Header("Hints")]
    public List<TaskHintLine> taskHints = new();

    [Header("Order Flow")]
    public TaskData followUpCookingTask;
    public List<OrderRecipeData> possibleGeneratedOrders = new();

    [Header("Costs")]
    [Min(0)] public int staminaCost = 10;

    [Header("Rewards - Success")]
    public int moneyReward = 0;
    public int reputationReward = 0;
    public int xpOnSuccess = 10;

    [Header("Rewards - Failure")]
    public int reputationPenalty = 0;
    public int xpOnFailure = 3;

    [Header("Rewards - Critical / 100%")]
    public int xpOnCritical = 15;
    public int bonusMoneyOnCritical = 0;
    public int bonusReputationOnCritical = 0;

    [Header("UI")]
    public Sprite taskIcon;
    public Color taskColor = Color.white;

    public int GetSuccessXP(bool isCritical)
    {
        return isCritical ? xpOnCritical : xpOnSuccess;
    }

    public int GetTotalMoneyReward(bool isCritical)
    {
        return isCritical ? moneyReward + bonusMoneyOnCritical : moneyReward;
    }

    public int GetTotalReputationReward(bool isCritical)
    {
        return isCritical ? reputationReward + bonusReputationOnCritical : reputationReward;
    }
}