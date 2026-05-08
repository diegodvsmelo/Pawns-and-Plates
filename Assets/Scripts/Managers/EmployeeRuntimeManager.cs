using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class EmployeeRuntimeManager : MonoBehaviour
{
    [Header("Employees To Manage")]
    [SerializeField] private List<EmployeeData> sessionEmployees = new();

    [Header("Stamina Recovery")]
    [SerializeField] private float passiveAvailableRecoveryPerSecond = 1f;
    [SerializeField] private float restingRecoveryPerSecond = 5f;

    private readonly HashSet<EmployeeData> uniqueEmployees = new();

    private void Awake()
    {
        BuildUniqueList();
        ResetEmployeesForSession();
    }

    private void Update()
    {
        if (ShouldPauseRecovery())
            return;

        TickRecovery();
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

    private bool ShouldPauseRecovery()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
            return true;

        return false;
    }

    private void TickRecovery()
    {
        foreach (EmployeeData employee in uniqueEmployees)
        {
            if (employee == null)
                continue;

            employee.TickPassiveRecovery(passiveAvailableRecoveryPerSecond, Time.deltaTime);
            employee.TickRestRecovery(restingRecoveryPerSecond, Time.deltaTime);
        }
    }
}