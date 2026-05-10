using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskInstance
{
    public TaskData data;
    public TaskState state;

    public float remainingExpirationTime;
    public float remainingExecutionTime;

    public List<EmployeeData> assignedEmployees = new();

    [Header("Order Flow")]
    public RestaurantOrder linkedOrder;

    [Header("Team Attributes")]
    public int teamCooking;
    public int teamService;
    public int teamOperational;
    public int teamAgility;

    public float chancePercent;
    public bool isCritical;
    public bool hasRolledResult;
    public bool wasSuccessful;
    public float rolledValue = -1f;

    private const int MaxTeamAttributeValue = 10;

    public void CalculateAndStoreSuccessChance()
    {
        chancePercent = TaskSuccessCalculator.CalculateChancePercent(this);
        isCritical = TaskSuccessCalculator.IsCritical(chancePercent);
    }

    public void ResetStoredResult()
    {
        chancePercent = 0f;
        isCritical = false;
        hasRolledResult = false;
        wasSuccessful = false;
        rolledValue = -1f;
    }

    public bool RollSuccessIfNeeded()
    {
        if (hasRolledResult)
            return wasSuccessful;

        rolledValue = Random.Range(0f, 100f);
        wasSuccessful = isCritical || rolledValue <= chancePercent;
        hasRolledResult = true;

        return wasSuccessful;
    }

    public TaskInstance(TaskData data)
    {
        this.data = data;
        state = TaskState.Available;

        remainingExpirationTime = data != null ? data.expirationTime : 0f;
        remainingExecutionTime = data != null ? data.executionTime : 0f;

        ResetStoredResult();
    }

    public bool CanExpire()
    {
        return data != null && data.expirationTime > 0f;
    }

    public bool IsAvailable()
    {
        return state == TaskState.Available;
    }

    public bool IsInProgress()
    {
        return state == TaskState.InProgress;
    }

    public bool IsReadyToCollect()
    {
        return state == TaskState.ReadyToCollect;
    }

    public void AssignEmployees(List<EmployeeData> employees)
    {
        assignedEmployees.Clear();

        if (employees != null)
            assignedEmployees.AddRange(employees);

        RecalculateTeamAttributes();
        ResetStoredResult();
    }

    public void SetLinkedOrder(RestaurantOrder order)
    {
        linkedOrder = order;
    }

    public void ClearLinkedOrder()
    {
        linkedOrder = null;
    }

    public void RecalculateTeamAttributes()
    {
        ClearTeamAttributes();

        foreach (EmployeeData employee in assignedEmployees)
        {
            if (employee == null)
                continue;

            teamCooking += employee.cookingSkill;
            teamService += employee.serviceSkill;
            teamOperational += employee.operationalSkill;
            teamAgility += employee.agility;
        }

        ClampTeamAttributes();
    }

    private void ClampTeamAttributes()
    {
        teamCooking = Mathf.Clamp(teamCooking, 0, MaxTeamAttributeValue);
        teamService = Mathf.Clamp(teamService, 0, MaxTeamAttributeValue);
        teamOperational = Mathf.Clamp(teamOperational, 0, MaxTeamAttributeValue);
        teamAgility = Mathf.Clamp(teamAgility, 0, MaxTeamAttributeValue);
    }

    public void ClearTeamAttributes()
    {
        teamCooking = 0;
        teamService = 0;
        teamOperational = 0;
        teamAgility = 0;
    }

    public int GetTeamAttribute(EmployeeSkillType skillType)
    {
        switch (skillType)
        {
            case EmployeeSkillType.Cooking:
                return teamCooking;

            case EmployeeSkillType.Service:
                return teamService;

            case EmployeeSkillType.Operational:
                return teamOperational;

            case EmployeeSkillType.Agility:
                return teamAgility;

            default:
                return 0;
        }
    }
}