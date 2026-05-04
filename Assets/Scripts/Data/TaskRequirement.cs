using System;
using UnityEngine;

[Serializable]
public class TaskRequirement
{
    [Header("Skill")]
    public EmployeeSkillType skillType;

    [Header("Weight")]
    [Min(0f)] public float weight = 1f;
}