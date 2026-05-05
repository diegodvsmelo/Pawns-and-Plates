using System.Collections.Generic;
using UnityEngine;

public class StaffSidebarModeController : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private GameObject compactSidebarRoot;
    [SerializeField] private GameObject expandedSidebarRoot;

    [Header("Lists")]
    [SerializeField] private EmployeeCardListUI compactListUI;
    [SerializeField] private EmployeeCardListUI expandedListUI;

    [Header("Current Employees - Temporary Source")]
    [SerializeField] private List<EmployeeData> currentEmployees = new();

    private bool isExpandedMode;

    private void Start()
    {
        ShowCompactMode();
        RebuildCompact();
    }

    public void SetEmployees(List<EmployeeData> employees)
    {
        currentEmployees = employees != null
            ? new List<EmployeeData>(employees)
            : new List<EmployeeData>();

        if (isExpandedMode)
            RebuildExpanded();
        else
            RebuildCompact();
    }

    public void ShowCompactMode()
    {
        isExpandedMode = false;

        if (compactSidebarRoot != null)
            compactSidebarRoot.SetActive(true);

        if (expandedSidebarRoot != null)
            expandedSidebarRoot.SetActive(false);

        RebuildCompact();

        if (expandedListUI != null)
            expandedListUI.Clear();
    }

    public void ShowExpandedMode()
    {
        isExpandedMode = true;

        if (compactSidebarRoot != null)
            compactSidebarRoot.SetActive(false);

        if (expandedSidebarRoot != null)
            expandedSidebarRoot.SetActive(true);

        RebuildExpanded();
    }

    private void RebuildCompact()
    {
        if (compactListUI != null)
            compactListUI.Rebuild(currentEmployees);
    }

    private void RebuildExpanded()
    {
        if (expandedListUI != null)
            expandedListUI.Rebuild(currentEmployees);
    }
}