using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskTeamSelectionUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject elements;

    [Header("Task Info")]
    [SerializeField] private TextMeshProUGUI taskNameText;
    [SerializeField] private TextMeshProUGUI taskDescriptionText;

    [Header("Expanded Employee Cards")]
    [SerializeField] private EmployeeCardListUI expandedCardListUI;

    [Header("Selected Employees Slots")]
    [SerializeField] private Transform selectedEmployeeSlotsContainer;
    [SerializeField] private GameObject taskSlotEmployeePrefab;

    [Header("Buttons")]
    [SerializeField] private Button dispatchButton;
    [SerializeField] private Button closeButton;

    [Header("Temporary Employee Source")]
    [SerializeField] private List<EmployeeData> currentEmployees = new();

    private readonly List<EmployeeTaskSlot> generatedSlots = new();

    private TaskPin currentTaskPin;
    private TaskData currentTaskData;

    [Header("Team Stats")]
    [SerializeField] private TeamStatsUI teamStatsUI;

    private void Awake()
    {
        if (dispatchButton != null)
        {
            dispatchButton.onClick.RemoveListener(OnDispatchButtonClicked);
            dispatchButton.onClick.AddListener(OnDispatchButtonClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }

        HideInstant();
    }

    public void SetEmployees(List<EmployeeData> employees)
    {
        currentEmployees = employees != null
            ? new List<EmployeeData>(employees)
            : new List<EmployeeData>();
    }

    public void Open(TaskPin taskPin)
    {
        if (taskPin == null || taskPin.data == null)
            return;

        currentTaskPin = taskPin;
        currentTaskData = taskPin.data;

        if (GameManager.Instance != null)
            GameManager.Instance.isGamePaused = true;

        currentTaskPin.PauseTimer();

        if (elements != null)
            elements.SetActive(true);

        UpdateTaskInfo();
        GenerateEmployeeSlots(currentTaskData.maxSlots);
        RebuildExpandedEmployeeCards();

        RefreshTeamStats();
    }

    private void RefreshTeamStats()
    {
        if (teamStatsUI == null)
            return;

        List<EmployeeData> selectedEmployees = GetSelectedEmployees();
        teamStatsUI.UpdateFromEmployees(selectedEmployees);
    }
    public void Close()
    {
        if (currentTaskPin != null && currentTaskPin.CurrentState == TaskState.Available)
            currentTaskPin.ResumeTimer();

        ClearEmployeeSlots();

        if (expandedCardListUI != null)
            expandedCardListUI.Clear();

        currentTaskPin = null;
        currentTaskData = null;

        if (elements != null)
            elements.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.isGamePaused = false;
    }

    private void HideInstant()
    {
        if (elements != null)
            elements.SetActive(false);
    }

    private void UpdateTaskInfo()
    {
        if (currentTaskData == null)
            return;

        if (taskNameText != null)
            taskNameText.text = currentTaskData.taskName;

        if (taskDescriptionText != null)
            taskDescriptionText.text = currentTaskData.description;
    }

    private void GenerateEmployeeSlots(int amount)
    {
        ClearEmployeeSlots();

        if (selectedEmployeeSlotsContainer == null || taskSlotEmployeePrefab == null)
            return;

        amount = Mathf.Max(0, amount);

        for (int i = 0; i < amount; i++)
        {
            GameObject slotObj = Instantiate(taskSlotEmployeePrefab, selectedEmployeeSlotsContainer);

            EmployeeTaskSlot slot = slotObj.GetComponent<EmployeeTaskSlot>();

            if (slot != null)
            {
                slot.OnSlotChanged += RefreshTeamStats;
                generatedSlots.Add(slot);
            }
        }

        RefreshTeamStats();
    }

    private void ClearEmployeeSlots()
    {
        foreach (EmployeeTaskSlot slot in generatedSlots)
        {
            if (slot != null)
                slot.OnSlotChanged -= RefreshTeamStats;
        }

        generatedSlots.Clear();

        if (selectedEmployeeSlotsContainer == null)
            return;

        foreach (Transform child in selectedEmployeeSlotsContainer)
            Destroy(child.gameObject);

        if (teamStatsUI != null)
            teamStatsUI.Clear();
    }

    private void RebuildExpandedEmployeeCards()
    {
        if (expandedCardListUI != null)
            expandedCardListUI.Rebuild(currentEmployees);
    }

    private void OnDispatchButtonClicked()
    {
        if (currentTaskPin == null || currentTaskPin.Instance == null)
            return;

        List<EmployeeData> selectedEmployees = GetSelectedEmployees();

        if (selectedEmployees.Count == 0)
        {
            Debug.LogWarning("Nenhum funcionário selecionado para a task.");
            return;
        }

        currentTaskPin.Instance.AssignEmployees(selectedEmployees);

        ConsumeSelectedEmployeesStamina(selectedEmployees, currentTaskData.staminaCost);
        RefreshSelectedCards();

        Debug.Log(
            $"Task '{currentTaskData.taskName}' iniciada com {selectedEmployees.Count} funcionário(s). " +
            $"Cooking: {currentTaskPin.Instance.teamCooking}, " +
            $"Service: {currentTaskPin.Instance.teamService}, " +
            $"Operational: {currentTaskPin.Instance.teamOperational}, " +
            $"Agility: {currentTaskPin.Instance.teamAgility}"
        );

        currentTaskPin.ResumeTimer();
        currentTaskPin.StartExecution();

        ClearEmployeeSlots();

        if (expandedCardListUI != null)
            expandedCardListUI.Clear();

        currentTaskPin = null;
        currentTaskData = null;

        if (elements != null)
            elements.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.isGamePaused = false;
    }

    private void ConsumeSelectedEmployeesStamina(List<EmployeeData> selectedEmployees, int staminaCost)
    {
        if (selectedEmployees == null || staminaCost <= 0)
            return;

        foreach (EmployeeData employee in selectedEmployees)
        {
            if (employee == null)
                continue;

            employee.currentStamina -= staminaCost;
            employee.currentStamina = Mathf.Clamp(employee.currentStamina, 0, employee.maxStamina);
        }
    }

    private void RefreshSelectedCards()
    {
        foreach (EmployeeTaskSlot slot in generatedSlots)
        {
            if (slot == null || slot.CurrentCard == null)
                continue;

            slot.CurrentCard.Refresh();
        }

        if (expandedCardListUI != null)
            expandedCardListUI.Refresh();
    }
    
    private List<EmployeeData> GetSelectedEmployees()
    {
        List<EmployeeData> selectedEmployees = new();

        foreach (EmployeeTaskSlot slot in generatedSlots)
        {
            if (slot == null)
                continue;

            if (slot.CurrentCard == null)
                continue;

            EmployeeData employeeData = slot.CurrentCard.Data;

            if (employeeData == null)
                continue;

            if (!selectedEmployees.Contains(employeeData))
                selectedEmployees.Add(employeeData);
        }

        return selectedEmployees;
    }
}