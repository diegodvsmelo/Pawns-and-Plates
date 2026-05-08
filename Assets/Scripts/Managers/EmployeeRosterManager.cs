using System.Collections.Generic;
using UnityEngine;

public class EmployeeRosterManager : MonoBehaviour
{
    public static EmployeeRosterManager Instance { get; private set; }

    [Header("Current Employee Roster")]
    [SerializeField] private List<EmployeeData> currentEmployees = new();

    [Header("UI Views")]
    [SerializeField] private StaffSidebarUI compactSidebarUI;
    [SerializeField] private EmployeeCardListUI expandedSidebarUI;

    public IReadOnlyList<EmployeeData> CurrentEmployees => currentEmployees;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RebuildAllViews();
    }

    public List<EmployeeData> GetCurrentEmployeesList()
    {
        return currentEmployees;
    }

    public void SetEmployees(List<EmployeeData> employees)
    {
        currentEmployees = employees != null
            ? new List<EmployeeData>(employees)
            : new List<EmployeeData>();

        RebuildAllViews();
    }

    public void AddEmployee(EmployeeData employee)
    {
        if (employee == null)
            return;

        if (currentEmployees.Contains(employee))
            return;

        currentEmployees.Add(employee);
        RebuildAllViews();
    }

    public void RemoveEmployee(EmployeeData employee)
    {
        if (employee == null)
            return;

        if (currentEmployees.Remove(employee))
            RebuildAllViews();
    }

    public void RebuildAllViews()
    {
        if (compactSidebarUI != null)
            compactSidebarUI.Rebuild(currentEmployees);

        if (expandedSidebarUI != null)
            expandedSidebarUI.Rebuild(currentEmployees);
    }

    public void RefreshAllViews()
    {
        if (compactSidebarUI != null)
            compactSidebarUI.Refresh();

        if (expandedSidebarUI != null)
            expandedSidebarUI.Refresh();
    }

    public void RebuildCompactView()
    {
        if (compactSidebarUI != null)
            compactSidebarUI.Rebuild(currentEmployees);
    }

    public void RebuildExpandedView()
    {
        if (expandedSidebarUI != null)
            expandedSidebarUI.Rebuild(currentEmployees);
    }
}