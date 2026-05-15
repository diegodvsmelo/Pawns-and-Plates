using UnityEngine;
using TMPro;
using System;

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private RectTransform clockRoot;
    [SerializeField] private GameObject dailyReportPanel;
    [SerializeField] private TextMeshProUGUI reportDetailsText;

    [Header("Day Timing")]
    [SerializeField] private int startHour = 8;
    [SerializeField] private int endHour = 18;
    [SerializeField] private float realSecondsPerInGameMinute = 1f;

    [Header("Clock Alert")]
    [SerializeField] private float warningShakeDuration = 1f;
    [SerializeField] private float warningShakeStrength = 10f;

    [Header("Game State")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int moneyEarnedToday = 0;

    public event Action<int> OnDayChanged;
    public event Action<int> OnDailyEarningsChanged;
    public event Action<int, int, int> OnDayEnded;
    public event Action<int> OnNextDayStarted;
    public event Action<int, int> OnClockChanged;
    public event Action OnClosingTimeStarted;

    public int CurrentDay => currentDay;
    public int MoneyEarnedToday => moneyEarnedToday;
    public bool IsDayRunning => dayState == DayState.Running;
    public bool IsClosingTime => dayState == DayState.ClosingTime || dayState == DayState.WaitingForTasksToFinish;
    public bool IsReportOpen => dayState == DayState.ReportOpen;

    private GameManager gameManager;
    private ResourceManager resourceManager;
    private TaskSpawner taskSpawner;

    private DayState dayState = DayState.Running;
    private float minuteTimer;
    private int currentHour;
    private int currentMinute;
    private bool oneHourWarningTriggered;
    private Vector2 originalClockAnchoredPosition;

    private enum DayState
    {
        Running,
        ClosingTime,
        WaitingForTasksToFinish,
        ReportOpen
    }

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
        gameManager = GameManager.Instance;
        resourceManager = ResourceManager.Instance;
        taskSpawner = TaskSpawner.Instance;

        if (dailyReportPanel != null)
            dailyReportPanel.SetActive(false);

        if (clockRoot != null)
            originalClockAnchoredPosition = clockRoot.anchoredPosition;

        InitializeDayClock();
        NotifyDayStateChanged();
        UpdateUI();
    }

    private void Update()
    {
        if (!ShouldPauseDayClock())
            TickClock();

        if (dayState == DayState.ClosingTime || dayState == DayState.WaitingForTasksToFinish)
            TryFinishDayAfterTasks();
    }

    private bool ShouldPauseDayClock()
    {
        if (dayState != DayState.Running)
            return true;

        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
            return true;

        if (TaskSpawner.IsResultPopupOpenGlobally)
            return true;

        return false;
    }

    private void InitializeDayClock()
    {
        currentHour = startHour;
        currentMinute = 0;
        minuteTimer = 0f;
        oneHourWarningTriggered = false;
        dayState = DayState.Running;
    }

    public void AddDailyEarnings(int amount)
    {
        moneyEarnedToday += amount;
        OnDailyEarningsChanged?.Invoke(moneyEarnedToday);
    }

    private void TickClock()
    {
        if (realSecondsPerInGameMinute <= 0f)
            return;

        minuteTimer += Time.deltaTime;

        while (minuteTimer >= realSecondsPerInGameMinute)
        {
            minuteTimer -= realSecondsPerInGameMinute;
            AdvanceOneMinute();
        }
    }

    private void AdvanceOneMinute()
    {
        currentMinute++;

        if (currentMinute >= 60)
        {
            currentMinute = 0;
            currentHour++;
        }

        OnClockChanged?.Invoke(currentHour, currentMinute);
        UpdateClockUI();

        if (!oneHourWarningTriggered && currentHour == endHour - 1 && currentMinute == 0)
        {
            oneHourWarningTriggered = true;

            if (clockRoot != null)
                StartCoroutine(ClockWarningShakeRoutine());
        }

        if (currentHour >= endHour)
            BeginClosingTime();
    }

    private void BeginClosingTime()
    {
        if (dayState != DayState.Running)
            return;

        dayState = DayState.ClosingTime;

        if (taskSpawner != null)
            taskSpawner.StopSpawning();

        OnClosingTimeStarted?.Invoke();
        TryFinishDayAfterTasks();
    }

    private void TryFinishDayAfterTasks()
    {
        if (taskSpawner == null)
            taskSpawner = TaskSpawner.Instance;

        bool hasActiveTasks = taskSpawner != null && taskSpawner.HasActiveTasks();
        bool hasUnfinishedFlow = TaskFlowManager.Instance != null && TaskFlowManager.Instance.HasUnfinishedRestaurantFlow();

        if (hasActiveTasks || hasUnfinishedFlow)
        {
            dayState = DayState.WaitingForTasksToFinish;
            return;
        }

        EndDay();
    }

    public void EndDay()
    {
        if (dayState == DayState.ReportOpen)
            return;

        dayState = DayState.ReportOpen;

        int totalSalaries = CalculateTotalWages();

        if (resourceManager != null)
            resourceManager.ModifyMoney(-totalSalaries);

        int profit = moneyEarnedToday - totalSalaries;

        OnDayEnded?.Invoke(moneyEarnedToday, totalSalaries, profit);
        ShowDailyReport(totalSalaries, profit);
    }

    private int CalculateTotalWages()
    {
        int total = 0;

        EmployeeCard[] allCards = FindObjectsByType<EmployeeCard>(FindObjectsSortMode.None);

        foreach (EmployeeCard card in allCards)
        {
            if (card == null || card.data == null)
                continue;

            if (card.transform.parent != null)
                total += card.data.GetDailyCost();
        }

        return total;
    }

    private void ShowDailyReport(int wagesPaid, int profit)
{
    string report = $"DAY {currentDay} SUMMARY\n\n";
    report += $"Revenue: <color=green>+${moneyEarnedToday}</color>\n";
    report += $"Wages: <color=red>-${wagesPaid}</color>\n";
    report += "----------------\n";

    if (profit >= 0)
        report += $"Net Profit: <color=green>${profit}</color>";
    else
        report += $"Loss: <color=red>${profit}</color>";

    if (reportDetailsText != null)
        reportDetailsText.text = report;

    if (dailyReportPanel != null)
        dailyReportPanel.SetActive(true);
}

    public void StartNextDay()
    {
        currentDay++;
        moneyEarnedToday = 0;

        InitializeDayClock();

        if (dailyReportPanel != null)
            dailyReportPanel.SetActive(false);

        if (taskSpawner == null)
            taskSpawner = TaskSpawner.Instance;

        if (taskSpawner != null)
            taskSpawner.StartSpawning();

        NotifyDayStateChanged();
        UpdateUI();
    }

    private void NotifyDayStateChanged()
    {
        OnDayChanged?.Invoke(currentDay);
        OnDailyEarningsChanged?.Invoke(moneyEarnedToday);
        OnNextDayStarted?.Invoke(currentDay);
        OnClockChanged?.Invoke(currentHour, currentMinute);
    }

    private void UpdateUI()
    {
        UpdateDayUI();
        UpdateClockUI();
    }

    private void UpdateDayUI()
    {
        if (dayText != null)
            dayText.text = $"CURRENT DAY: {currentDay}";
    }

    private void UpdateClockUI()
    {
        if (clockText != null)
            clockText.text = $"{currentHour:00}:{currentMinute:00}";
    }

    private System.Collections.IEnumerator ClockWarningShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < warningShakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float offsetX = UnityEngine.Random.Range(-warningShakeStrength, warningShakeStrength);
            float offsetY = UnityEngine.Random.Range(-warningShakeStrength, warningShakeStrength);

            if (clockRoot != null)
                clockRoot.anchoredPosition = originalClockAnchoredPosition + new Vector2(offsetX, offsetY);

            yield return null;
        }

        if (clockRoot != null)
            clockRoot.anchoredPosition = originalClockAnchoredPosition;
    }
}