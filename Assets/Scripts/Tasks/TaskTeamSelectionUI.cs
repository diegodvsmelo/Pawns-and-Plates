using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskTeamSelectionUI : MonoBehaviour
{
    private EmployeeCardUI clickSelectedCard;

    [Header("Root")]
    [SerializeField] private GameObject elements;

    [Header("External UI")]
    [SerializeField] private GameObject compactSidebarRoot;

    [Header("Task Info")]
    [SerializeField] private TextMeshProUGUI taskNameText;
    [SerializeField] private TextMeshProUGUI taskDescriptionText;

    [Header("Task Hints")]
    [SerializeField] private TaskHintsPanelUI taskHintsPanelUI;

    [Header("Expanded Employee Cards")]
    [SerializeField] private EmployeeCardListUI expandedCardListUI;

    [Header("Employee Roster")]
    [SerializeField] private EmployeeRosterManager employeeRosterManager;

    [Header("Selected Employees Slots")]
    [SerializeField] private Transform selectedEmployeeSlotsContainer;
    [SerializeField] private GameObject taskSlotEmployeePrefab;

    [Header("Buttons")]
    [SerializeField] private Button dispatchButton;
    [SerializeField] private Button closeButton;

    [Header("Team Stats")]
    [SerializeField] private TeamStatsUI teamStatsUI;

    private readonly List<EmployeeTaskSlot> generatedSlots = new();

    private TaskPin currentTaskPin;
    private TaskData currentTaskData;

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
        if (employeeRosterManager == null)
            employeeRosterManager = EmployeeRosterManager.Instance;

        if (employeeRosterManager == null)
        {
            Debug.LogWarning("[TaskTeamSelectionUI] EmployeeRosterManager não encontrado.");
            return;
        }

        employeeRosterManager.SetEmployees(employees);
    }

    public void Open(TaskPin taskPin)
    {
        if (taskPin == null || taskPin.data == null)
            return;

        ClearClickSelectedCard();

        currentTaskPin = taskPin;
        currentTaskData = taskPin.data;

        if (GameManager.Instance != null)
            GameManager.Instance.PauseGame();
        else
            Debug.LogError("[TaskTeamSelectionUI] GameManager.Instance está NULL ao abrir a task.");

        currentTaskPin.PauseTimer();

        SetTaskSelectionVisible(true);

        UpdateTaskInfo();
        RefreshTaskHints();
        GenerateEmployeeSlots(currentTaskData.maxSlots);
        RebuildExpandedEmployeeCards();
        RefreshTeamStats();
    }

    public void OnEmployeeCardClicked(EmployeeCardUI card)
    {
        if (card == null || card.Data == null)
            return;

        if (!card.Data.CanBeAssignedToTask())
            return;

        if (clickSelectedCard == card)
        {
            SetClickSelectedCard(null);
            return;
        }

        SetClickSelectedCard(card);
    }

    public void OnTaskSlotClicked(EmployeeTaskSlot slot)
    {
        if (slot == null || clickSelectedCard == null || clickSelectedCard.Data == null)
            return;

        if (!clickSelectedCard.Data.CanBeAssignedToTask())
        {
            SetClickSelectedCard(null);
            return;
        }

        bool placed = slot.TryReceiveCardByClick(clickSelectedCard);

        if (placed)
            SetClickSelectedCard(null);
    }

    private void SetClickSelectedCard(EmployeeCardUI newSelectedCard)
    {
        if (clickSelectedCard == newSelectedCard)
            return;

        if (clickSelectedCard != null)
        {
            EmployeeCardVisuals previousVisuals = clickSelectedCard.GetComponent<EmployeeCardVisuals>();

            if (previousVisuals != null)
                previousVisuals.SetSelected(false);
        }

        clickSelectedCard = newSelectedCard;

        if (clickSelectedCard != null)
        {
            EmployeeCardVisuals newVisuals = clickSelectedCard.GetComponent<EmployeeCardVisuals>();

            if (newVisuals != null)
                newVisuals.SetSelected(true);
        }
    }

    private void ClearClickSelectedCard()
    {
        SetClickSelectedCard(null);
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

        ClearClickSelectedCard();
        ClearTaskHints();

        currentTaskPin = null;
        currentTaskData = null;

        SetTaskSelectionVisible(false);

        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    private void HideInstant()
    {
        SetTaskSelectionVisible(false);
    }

    private void SetTaskSelectionVisible(bool visible)
    {
        if (elements != null)
            elements.SetActive(visible);

        if (compactSidebarRoot != null)
            compactSidebarRoot.SetActive(!visible);

        if (!visible && employeeRosterManager != null)
            employeeRosterManager.RefreshAllViews();
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

    private void RefreshTaskHints()
    {
        if (taskHintsPanelUI == null)
            return;

        if (currentTaskData == null)
        {
            taskHintsPanelUI.ClearHints();
            return;
        }

        taskHintsPanelUI.ShowHints(currentTaskData);
    }

    private void ClearTaskHints()
    {
        if (taskHintsPanelUI != null)
            taskHintsPanelUI.ClearHints();
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
        if (expandedCardListUI == null)
            return;

        if (employeeRosterManager == null)
            employeeRosterManager = EmployeeRosterManager.Instance;

        if (employeeRosterManager == null)
        {
            Debug.LogWarning("[TaskTeamSelectionUI] EmployeeRosterManager não encontrado ao rebuildar cards expandidos.");
            expandedCardListUI.Clear();
            return;
        }

        expandedCardListUI.Rebuild(employeeRosterManager.GetCurrentEmployeesList());
    }

    private void OnDispatchButtonClicked()
    {
        if (currentTaskPin == null || currentTaskPin.Instance == null)
            return;

        ClearClickSelectedCard();

        List<EmployeeData> selectedEmployees = GetSelectedEmployees();

        if (selectedEmployees.Count == 0)
        {
            Debug.LogWarning("Nenhum funcionário selecionado para a task.");
            return;
        }

        for (int i = 0; i < selectedEmployees.Count; i++)
        {
            EmployeeData employee = selectedEmployees[i];

            if (employee == null || !employee.CanBeAssignedToTask())
            {
                Debug.LogWarning("Há funcionário indisponível, ocupado ou sem stamina na equipe selecionada.");
                return;
            }
        }

        currentTaskPin.Instance.AssignEmployees(selectedEmployees);
        currentTaskPin.Instance.CalculateAndStoreSuccessChance();

        ConsumeSelectedEmployeesStamina(selectedEmployees, currentTaskData.staminaCost);

        foreach (EmployeeData employee in selectedEmployees)
        {
            if (employee == null)
                continue;

            employee.SetOccupied();
        }

        RefreshSelectedCards();

        Debug.Log(
            $"Task '{currentTaskData.taskName}' iniciada com {selectedEmployees.Count} funcionário(s). " +
            $"Cooking: {currentTaskPin.Instance.teamCooking}, " +
            $"Service: {currentTaskPin.Instance.teamService}, " +
            $"Operational: {currentTaskPin.Instance.teamOperational}, " +
            $"Agility: {currentTaskPin.Instance.teamAgility}, " +
            $"Chance: {currentTaskPin.Instance.chancePercent:F1}%, " +
            $"Critical: {currentTaskPin.Instance.isCritical}"
        );

        currentTaskPin.ResumeTimer();
        currentTaskPin.StartExecution();

        ClearEmployeeSlots();

        if (expandedCardListUI != null)
            expandedCardListUI.Clear();

        ClearTaskHints();

        currentTaskPin = null;
        currentTaskData = null;

        SetTaskSelectionVisible(false);

        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    private void ConsumeSelectedEmployeesStamina(List<EmployeeData> selectedEmployees, int staminaCost)
    {
        if (selectedEmployees == null || staminaCost <= 0)
            return;

        foreach (EmployeeData employee in selectedEmployees)
        {
            if (employee == null)
                continue;

            employee.ConsumeStamina(staminaCost);
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

        if (employeeRosterManager != null)
            employeeRosterManager.RefreshAllViews();
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