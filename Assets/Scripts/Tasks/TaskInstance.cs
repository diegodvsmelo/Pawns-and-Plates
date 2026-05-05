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

    [Header("Team Attributes")]
    public int teamCooking;
    public int teamService;
    public int teamOperational;
    public int teamAgility;

    public float chancePercent;
    public bool isCritical;

    private const int MaxTeamAttributeValue = 10;

    public TaskInstance(TaskData data)
    {
        this.data = data;
        state = TaskState.Available;

        remainingExpirationTime = data != null ? data.expirationTime : 0f;
        remainingExecutionTime = data != null ? data.executionTime : 0f;
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