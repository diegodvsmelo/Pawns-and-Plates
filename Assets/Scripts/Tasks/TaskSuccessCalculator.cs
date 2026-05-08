using UnityEngine;

public static class TaskSuccessCalculator
{
    public static float CalculateChancePercent(TaskInstance instance)
    {
        if (instance == null || instance.data == null)
            return 0f;

        return CalculateChancePercent(
            instance.data,
            instance.teamCooking,
            instance.teamService,
            instance.teamOperational,
            instance.teamAgility
        );
    }

    public static float CalculateChancePercent(
        TaskData taskData,
        int teamCooking,
        int teamService,
        int teamOperational,
        int teamAgility)
    {
        if (taskData == null)
            return 0f;

        if (taskData.requirements == null || taskData.requirements.Count == 0)
            return 100f;

        float product = 1f;
        int validRequirements = 0;

        foreach (TaskRequirement requirement in taskData.requirements)
        {
            if (requirement == null || requirement.requiredValue <= 0)
                continue;

            float teamValue = GetTeamValue(
                requirement.skillType,
                teamCooking,
                teamService,
                teamOperational,
                teamAgility
            );

            float ratio = Mathf.Clamp01(teamValue / (float)requirement.requiredValue);

            product *= ratio;
            validRequirements++;

            if (Mathf.Approximately(product, 0f))
                return 0f;
        }

        if (validRequirements == 0)
            return 100f;

        float geometricMean = Mathf.Pow(product, 1f / validRequirements);
        return Mathf.Clamp(geometricMean * 100f, 0f, 100f);
    }

    public static bool IsCritical(float chancePercent)
    {
        return chancePercent >= 100f;
    }

    private static float GetTeamValue(
        EmployeeSkillType skillType,
        int teamCooking,
        int teamService,
        int teamOperational,
        int teamAgility)
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
                return 0f;
        }
    }
}