using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TaskPin : MonoBehaviour
{
    [Header("Data")]
    public TaskData data;
    public TaskInstance Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Image timerFillImage;
    [SerializeField] private Image timerBackgroundImage;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI stateText;

    [Header("Visual States")]
    [SerializeField] private Color availableColor;
    [SerializeField] private Color inProgressColor;
    [SerializeField] private Color readyToCollectColor;
    [SerializeField] private Color expiredColor;

    [Header("Timer Colors")]
    [SerializeField] private Color availableTimerColor;
    [SerializeField] private Color executionTimerColor;
    [SerializeField] private Color readyTimerColor;
    [SerializeField] private Color expiredTimerColor;

    private Action<TaskPin> onClick;
    private bool isPaused;

    public TaskState CurrentState
    {
        get
        {
            if (Instance == null)
                return TaskState.Available;

            return Instance.state;
        }
    }

    public void Setup(TaskData taskData, Action<TaskPin> clickCallback)
    {
        data = taskData;
        Instance = new TaskInstance(taskData);
        onClick = clickCallback;

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }

        SetupSlider();
        ApplyTaskVisualData();
        RefreshVisualState();
        UpdateTimerVisual();
    }

    private void Update()
    {
        if (Instance == null || data == null)
            return;

        if (ShouldPauseTimer())
            return;

        if (Instance.state == TaskState.Available)
        {
            UpdateAvailableTimer();
        }
        else if (Instance.state == TaskState.InProgress)
        {
            UpdateExecutionTimer();
        }

        UpdateTimerVisual();
    }

    private bool ShouldPauseTimer()
    {
        if (isPaused)
            return true;

        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
            return true;

        if (TaskSpawner.IsResultPopupOpenGlobally)
            return true;

        return false;
    }

    private void SetupSlider()
    {
        if (timerSlider == null)
            return;

        timerSlider.minValue = 0f;
        timerSlider.maxValue = 1f;
        timerSlider.value = 1f;
        timerSlider.interactable = false;
    }

    private void UpdateAvailableTimer()
    {
        if (!Instance.CanExpire())
            return;

        Instance.remainingExpirationTime -= Time.deltaTime;

        if (Instance.remainingExpirationTime <= 0f)
        {
            Instance.remainingExpirationTime = 0f;
            SetState(TaskState.Expired);
        }
    }

    private void UpdateExecutionTimer()
    {
        Instance.remainingExecutionTime -= Time.deltaTime;

        if (Instance.remainingExecutionTime <= 0f)
        {
            Instance.remainingExecutionTime = 0f;
            SetState(TaskState.ReadyToCollect);
        }
    }

    private void HandleClick()
    {
        if (Instance == null)
            return;

        onClick?.Invoke(this);
    }

    public void StartExecution()
    {
        if (Instance == null || data == null)
        {
            Debug.LogWarning("Não foi possível iniciar execução: Instance ou Data null.");
            return;
        }

        ResumeTimer();

        Instance.remainingExecutionTime = data.executionTime;
        SetState(TaskState.InProgress);

        Debug.Log($"Task '{data.taskName}' entrou em execução por {data.executionTime} segundos.");
    }

    public void SetState(TaskState newState)
    {
        if (Instance == null)
            return;

        Instance.state = newState;

        RefreshVisualState();
        UpdateTimerVisual();
    }

    public void CompleteAndDestroy()
    {
        if (Instance != null)
            Instance.state = TaskState.Completed;

        Destroy(gameObject);
    }

    public void PauseTimer()
    {
        isPaused = true;
    }

    public void ResumeTimer()
    {
        isPaused = false;
    }

    private void ApplyTaskVisualData()
    {
        if (data == null)
            return;

        if (iconImage != null)
        {
            iconImage.sprite = data.taskIcon;
            iconImage.gameObject.SetActive(data.taskIcon != null);
        }
    }

    private void RefreshVisualState()
    {
        if (Instance == null)
            return;

        if (backgroundImage != null)
        {
            if (Instance.state == TaskState.Available)
                backgroundImage.color = availableColor;
            else if (Instance.state == TaskState.InProgress)
                backgroundImage.color = inProgressColor;
            else if (Instance.state == TaskState.ReadyToCollect)
                backgroundImage.color = readyToCollectColor;
            else if (Instance.state == TaskState.Expired)
                backgroundImage.color = expiredColor;
        }

        if (timerBackgroundImage != null)
        {
            if (Instance.state == TaskState.Available)
                timerBackgroundImage.color = availableTimerColor;
            else if (Instance.state == TaskState.InProgress)
                timerBackgroundImage.color = executionTimerColor;
            else if (Instance.state == TaskState.ReadyToCollect)
                timerBackgroundImage.color = readyTimerColor;
            else if (Instance.state == TaskState.Expired)
                timerBackgroundImage.color = expiredTimerColor;
        }

        if (stateText != null)
        {
            if (Instance.state == TaskState.Available)
                stateText.text = "!";
            else if (Instance.state == TaskState.InProgress)
                stateText.text = "...";
            else if (Instance.state == TaskState.ReadyToCollect)
                stateText.text = "✓";
            else if (Instance.state == TaskState.Expired)
                stateText.text = "X";
            else
                stateText.text = "";
        }

        UpdateFillVisibility();
    }

    private void UpdateFillVisibility()
    {
        if (timerFillImage == null || Instance == null)
            return;

        Color color = timerFillImage.color;
        color.a = Instance.state == TaskState.ReadyToCollect ? 0f : 1f;
        timerFillImage.color = color;
    }

    private void UpdateTimerVisual()
    {
        if (Instance == null || data == null)
            return;

        if (timerSlider != null)
            timerSlider.value = GetTimerNormalizedValue();

        if (timerText != null)
            timerText.text = GetTimerText();
    }

    private float GetTimerNormalizedValue()
    {
        if (Instance.state == TaskState.Available)
        {
            if (!Instance.CanExpire())
                return 1f;

            if (data.expirationTime <= 0f)
                return 1f;

            return Mathf.Clamp01(Instance.remainingExpirationTime / data.expirationTime);
        }

        if (Instance.state == TaskState.InProgress)
        {
            if (data.executionTime <= 0f)
                return 1f;

            return Mathf.Clamp01(Instance.remainingExecutionTime / data.executionTime);
        }

        if (Instance.state == TaskState.ReadyToCollect)
            return 1f;

        if (Instance.state == TaskState.Expired)
            return 0f;

        return 0f;
    }

    private string GetTimerText()
    {
        if (Instance.state == TaskState.Available)
        {
            if (!Instance.CanExpire())
                return "";

            return Mathf.CeilToInt(Instance.remainingExpirationTime).ToString();
        }

        if (Instance.state == TaskState.InProgress)
            return Mathf.CeilToInt(Instance.remainingExecutionTime).ToString();

        if (Instance.state == TaskState.ReadyToCollect)
            return "Pronto";

        if (Instance.state == TaskState.Expired)
            return "Expirou";

        return "";
    }
}