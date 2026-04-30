using UnityEngine;
using UnityEngine.UI;

public class TaskPin : MonoBehaviour
{
    public TaskData data;
    
    [Header("UI")]
    public Slider timerSlider; 

    private System.Action<TaskData> onClickCallback;
    private float timeRemaining;
    private ResourceManager resourceManager; 
    
    // MUDANÇA: Referência ao GameManager
    private GameManager gameManager;

    public void Setup(TaskData taskData, System.Action<TaskData> callback)
    {
        this.data = taskData;
        this.onClickCallback = callback;
        
        timeRemaining = taskData.timeLimit;
        if (timerSlider != null)
        {
            timerSlider.maxValue = taskData.timeLimit;
            timerSlider.value = timeRemaining;
        }

        resourceManager = FindFirstObjectByType<ResourceManager>();
        
        // Busca o chefe para saber se pode contar tempo
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        // Se o GameManager existir e estiver pausado, NÃO FAZ NADA.
        if (gameManager != null && gameManager.isGamePaused) return;

        // Se chegou aqui, o jogo está rodando
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timerSlider != null)
                timerSlider.value = timeRemaining;

            if (timeRemaining <= 0)
            {
                HandleExpiration();
            }
        }
    }

    void HandleExpiration()
    {
        Debug.Log($"Tarefa {data.taskName} expirou! Cliente foi embora.");

        if (resourceManager != null)
        {
            resourceManager.ModifyReputation(-data.reputationPenalty);
        }
        Destroy(gameObject);
    }

    public void OnClick()
    {
        if (onClickCallback != null)
        {
            onClickCallback.Invoke(data);
        }
        Destroy(gameObject);
    }
}