using UnityEngine;
using TMPro;
using System;

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI dayText;
    public GameObject dailyReportPanel;
    public TextMeshProUGUI reportDetailsText;

    [Header("Game State")]
    public int currentDay = 1;
    public int moneyEarnedToday = 0;

    // OBSERVERS
    public event Action<int> OnDayChanged;
    public event Action<int> OnDailyEarningsChanged;
    public event Action<int, int, int> OnDayEnded;
    public event Action<int> OnNextDayStarted;

    private GameManager gameManager;
    private ResourceManager resourceManager;

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

        if (dailyReportPanel != null)
            dailyReportPanel.SetActive(false);

        OnDayChanged?.Invoke(currentDay);
        OnDailyEarningsChanged?.Invoke(moneyEarnedToday);

        UpdateDayUI();
    }

    public void AddDailyEarnings(int amount)
    {
        moneyEarnedToday += amount;
        OnDailyEarningsChanged?.Invoke(moneyEarnedToday);
    }

    public void EndDay()
    {
        if (gameManager != null && gameManager.spawner != null)
        {
            gameManager.spawner.StopSpawning();
        }

        int totalSalaries = CalculateTotalWages();

        if (resourceManager != null)
        {
            resourceManager.ModifyMoney(-totalSalaries);
        }

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
            if (card.transform.parent != null)
            {
                total += card.data.GetDailyCost();
            }
        }

        return total;
    }

    private void ShowDailyReport(int wagesPaid, int profit)
    {
        string report = $"RESUMO DO DIA {currentDay}\n\n";
        report += $"Faturamento: <color=green>+${moneyEarnedToday}</color>\n";
        report += $"Salários: <color=red>-${wagesPaid}</color>\n";
        report += "----------------\n";

        if (profit >= 0)
            report += $"Lucro Líquido: <color=green>${profit}</color>";
        else
            report += $"Prejuízo: <color=red>${profit}</color>";

        if (reportDetailsText != null)
            reportDetailsText.text = report;

        if (dailyReportPanel != null)
            dailyReportPanel.SetActive(true);
    }

    public void StartNextDay()
    {
        currentDay++;
        moneyEarnedToday = 0;

        OnDayChanged?.Invoke(currentDay);
        OnDailyEarningsChanged?.Invoke(moneyEarnedToday);
        OnNextDayStarted?.Invoke(currentDay);

        UpdateDayUI();

        if (dailyReportPanel != null)
            dailyReportPanel.SetActive(false);

        if (gameManager != null && gameManager.spawner != null)
        {
            gameManager.spawner.StartSpawning();
        }
    }

    private void UpdateDayUI()
    {
        if (dayText != null)
            dayText.text = $"Dia {currentDay}";
    }
}