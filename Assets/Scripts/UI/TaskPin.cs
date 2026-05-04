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
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI stateText;

    [Header("Visual States")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color inProgressColor = Color.yellow;
    [SerializeField] private Color readyToCollectColor = Color.green;
    [SerializeField] private Color expiredColor = Color.gray;

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

        ApplyTaskVisualData();
        RefreshVisualState();
    }

    private void Update()
    {
        if (Instance == null || data == null)
            return;

        if (isPaused)
            return;

        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
            return;

        if (Instance.state == TaskState.Available)
        {
            UpdateAvailableTimer();
        }
        else if (Instance.state == TaskState.InProgress)
        {
            UpdateExecutionTimer();
        }

        UpdateTimerText();
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

        if (Instance.state == TaskState.Expired)
            return;

        onClick?.Invoke(this);
    }

    public void SetState(TaskState newState)
    {
        if (Instance == null)
            return;

        Instance.state = newState;
        RefreshVisualState();
    }

    public void StartExecution()
    {
        if (Instance == null || data == null)
            return;

        Instance.remainingExecutionTime = data.executionTime;
        SetState(TaskState.InProgress);
    }

    public void MarkReadyToCollect()
    {
        SetState(TaskState.ReadyToCollect);
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

        if (backgroundImage != null)
            backgroundImage.color = data.taskColor;
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
    }

    private void UpdateTimerText()
    {
        if (timerText == null || Instance == null)
            return;

        if (Instance.state == TaskState.Available)
        {
            if (Instance.CanExpire())
                timerText.text = Mathf.CeilToInt(Instance.remainingExpirationTime).ToString();
            else
                timerText.text = "";
        }
        else if (Instance.state == TaskState.InProgress)
        {
            timerText.text = Mathf.CeilToInt(Instance.remainingExecutionTime).ToString();
        }
        else if (Instance.state == TaskState.ReadyToCollect)
        {
            timerText.text = "Pronto";
        }
        else if (Instance.state == TaskState.Expired)
        {
            timerText.text = "Expirou";
        }
        else
        {
            timerText.text = "";
        }
    }
}