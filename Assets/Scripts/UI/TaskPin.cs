using UnityEngine;
using UnityEngine.UI;
using System;

public class TaskPin : MonoBehaviour
{
    public TaskData data;

    [Header("UI")]
    public Slider timerSlider;

    // OBSERVERS
    public event Action<TaskPin> OnExpired;
    public event Action<TaskPin> OnClicked;
    public event Action<TaskPin, float> OnTimerChanged;

    private Action<TaskData> onClickCallback;
    private float timeRemaining;

    private ResourceManager resourceManager;
    private GameManager gameManager;

    public void Setup(TaskData taskData, Action<TaskData> callback)
    {
        data = taskData;
        onClickCallback = callback;

        timeRemaining = taskData.timeLimit;

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
        OnClicked?.Invoke(this);

        onClickCallback?.Invoke(data);

        Destroy(gameObject);
    }
}