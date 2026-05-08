using System;
using UnityEngine;

[Serializable]
public class TaskRequirement
{
    [Header("Skill")]
    public EmployeeSkillType skillType;

    [Header("Required Value")]
    [Range(1, 10)] public int requiredValue = 5;
}