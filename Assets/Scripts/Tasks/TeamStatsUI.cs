using System.Collections.Generic;
using UnityEngine;

public class TeamStatsUI : MonoBehaviour
{
    [Header("Attribute Squares")]
    [SerializeField] private AttributeSquaresUI cookingSquares;
    [SerializeField] private AttributeSquaresUI serviceSquares;
    [SerializeField] private AttributeSquaresUI operationalSquares;
    [SerializeField] private AttributeSquaresUI agilitySquares;

    [Header("Rules")]
    [SerializeField] private int maxTeamAttributeValue = 10;

    public void Clear()
    {
        UpdateVisuals(0, 0, 0, 0);
    }

    public void UpdateFromEmployees(List<EmployeeData> employees)
    {
        int cooking = 0;
        int service = 0;
        int operational = 0;
        int agility = 0;

        if (employees != null)
        {
            foreach (EmployeeData employee in employees)
            {
                if (employee == null)
                    continue;

                cooking += employee.cookingSkill;
                service += employee.serviceSkill;
                operational += employee.operationalSkill;
                agility += employee.agility;
            }
        }

        cooking = Mathf.Clamp(cooking, 0, maxTeamAttributeValue);
        service = Mathf.Clamp(service, 0, maxTeamAttributeValue);
        operational = Mathf.Clamp(operational, 0, maxTeamAttributeValue);
        agility = Mathf.Clamp(agility, 0, maxTeamAttributeValue);

        UpdateVisuals(cooking, service, operational, agility);
    }

    private void UpdateVisuals(int cooking, int service, int operational, int agility)
    {
        if (cookingSquares != null)
            cookingSquares.UpdateValue(cooking);

        if (serviceSquares != null)
            serviceSquares.UpdateValue(service);

        if (operationalSquares != null)
            operationalSquares.UpdateValue(operational);

        if (agilitySquares != null)
            agilitySquares.UpdateValue(agility);
    }
}