using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class EmployeeRuntimeManager : MonoBehaviour
{
    [Header("Employees To Manage")]
    [SerializeField] private List<EmployeeData> sessionEmployees = new();

    [Header("Rest Recovery")]
    [SerializeField] private float restingStaminaRecoveryPerSecond = 5f;

    private readonly HashSet<EmployeeData> uniqueEmployees = new();

    private void Awake()
    {
        BuildUniqueList();
        ResetEmployeesForSession();
    }

    private void Update()
    {
        TickRestRecovery();
    }

    private void BuildUniqueList()
    {
        uniqueEmployees.Clear();

        foreach (EmployeeData employee in sessionEmployees)
        {
            if (employee == null)
                continue;

            uniqueEmployees.Add(employee);
        }
    }

    public void ResetEmployeesForSession()
    {
        foreach (EmployeeData employee in uniqueEmployees)
        {
            if (employee == null)
                continue;

            if (employee.ShouldResetRuntimeStateOnSessionStart())
                employee.ResetRuntimeStateForSession();
        }
    }

    private void TickRestRecovery()
    {
        if (restingStaminaRecoveryPerSecond <= 0f)
            return;

        foreach (EmployeeData employee in uniqueEmployees)
        {
            if (employee == null)
                continue;

            employee.TickRestRecovery(restingStaminaRecoveryPerSecond, Time.deltaTime);
        }
    }
}