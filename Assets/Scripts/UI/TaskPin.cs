using UnityEngine;
using UnityEngine.UI;
using System;

public class TaskPin : MonoBehaviour
{
    public TaskData data;

    [Header("UI")]
    public Slider timerSlider;

    public event Action<TaskPin> OnExpired;
    public event Action<TaskPin> OnClicked;
    public event Action<TaskPin, float> OnTimerChanged;

    private Action<TaskPin> onClickCallback;

    private float timeRemaining;
    private bool isSelected = false;

    private ResourceManager resourceManager;
    private GameManager gameManager;

    public void Setup(TaskData taskData, Action<TaskPin> callback)
    {
        data = taskData;
        onClickCallback = callback;

        timeRemaining = taskData.timeLimit;
        isSelected = false;

        if (timerSlider != null)
        {
            timerSlider.maxValue = taskData.timeLimit;
            timerSlider.value = timeRemaining;
        }

        resourceManager = ResourceManager.Instance;
        gameManager = GameManager.Instance;
    }

    private void Update()
    {
        if (gameManager != null && gameManager.isGamePaused)
            return;

        if (isSelected)
            return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining < 0)
                timeRemaining = 0;

            if (timerSlider != null)
                timerSlider.value = timeRemaining;

            OnTimerChanged?.Invoke(this, timeRemaining);

            if (timeRemaining <= 0)
            {
                HandleExpiration();
            }
        }
    }

    private void HandleExpiration()
    {
        Debug.Log($"Tarefa {data.taskName} expirou! Cliente foi embora.");

        OnExpired?.Invoke(this);

        if (resourceManager != null)
        {
            resourceManager.ModifyReputation(-data.reputationPenalty);
        }

        Destroy(gameObject);
    }

    public void OnClick()
    {
        if (isSelected)
            return;

        isSelected = true;

        OnClicked?.Invoke(this);

        onClickCallback?.Invoke(this);
    }

    public void ResumeTimer()
    {
        isSelected = false;
    }

    public void CompleteAndDestroy()
    {
        Destroy(gameObject);
    }

    public float GetTimeRemaining()
    {
        return timeRemaining;
    }
}