using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TeamAttributeSummary
{
    public int cooking;
    public int service;
    public int operational;
    public int agility;

    public void Clear()
    {
        cooking = 0;
        service = 0;
        operational = 0;
        agility = 0;
    }

    public void AddEmployee(EmployeeData employee)
    {
        if (employee == null)
            return;

        cooking += employee.cookingSkill;
        service += employee.serviceSkill;
        operational += employee.operationalSkill;
        agility += employee.agility;
    }

    public void BuildFromEmployees(List<EmployeeData> employees)
    {
        Clear();

        if (employees == null)
            return;

        foreach (EmployeeData employee in employees)
            AddEmployee(employee);
    }

    public int GetValue(EmployeeSkillType skillType)
    {
        switch (skillType)
        {
            case EmployeeSkillType.Cooking:
                return cooking;

            case EmployeeSkillType.Service:
                return service;

            case EmployeeSkillType.Operational:
                return operational;

            case EmployeeSkillType.Agility:
                return agility;

            default:
                return 0;
        }
    }
}