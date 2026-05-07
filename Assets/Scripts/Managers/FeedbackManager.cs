using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    private ResourceManager resourceManager;
    private DayCycleManager dayCycleManager;
    private TaskSpawner taskSpawner;
    private GameManager gameManager;

    private void Start()
    {
        resourceManager = ResourceManager.Instance;
        dayCycleManager = DayCycleManager.Instance;
        taskSpawner = TaskSpawner.Instance;
        gameManager = GameManager.Instance;

        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (resourceManager != null)
        {
            resourceManager.OnMoneyChanged += HandleMoneyChanged;
            resourceManager.OnReputationChanged += HandleReputationChanged;
            resourceManager.OnMoneyInsufficient += HandleMoneyInsufficient;
            resourceManager.OnReputationReachedZero += HandleReputationReachedZero;
        }

        if (dayCycleManager != null)
        {
            dayCycleManager.OnDayChanged += HandleDayChanged;
            dayCycleManager.OnDayEnded += HandleDayEnded;
        }

        if (taskSpawner != null)
        {
            taskSpawner.OnTaskSpawned += HandleTaskSpawned;
            taskSpawner.OnTaskSelected += HandleTaskSelected;
        }

        if (gameManager != null)
        {
           // gameManager.OnMissionStarted += HandleMissionStarted;
           // gameManager.OnMissionFinished += HandleMissionFinished;
        }
    }

    private void Unsubscribe()
    {
        if (resourceManager != null)
        {
            resourceManager.OnMoneyChanged -= HandleMoneyChanged;
            resourceManager.OnReputationChanged -= HandleReputationChanged;
            resourceManager.OnMoneyInsufficient -= HandleMoneyInsufficient;
            resourceManager.OnReputationReachedZero -= HandleReputationReachedZero;
        }

        if (dayCycleManager != null)
        {
            dayCycleManager.OnDayChanged -= HandleDayChanged;
            dayCycleManager.OnDayEnded -= HandleDayEnded;
        }

        if (taskSpawner != null)
        {
            taskSpawner.OnTaskSpawned -= HandleTaskSpawned;
            taskSpawner.OnTaskSelected -= HandleTaskSelected;
        }

        if (gameManager != null)
        {
          //  gameManager.OnMissionStarted -= HandleMissionStarted;
           // gameManager.OnMissionFinished -= HandleMissionFinished;
        }
    }

    private void HandleMoneyChanged(int money)
    {
        Debug.Log($"[Observer] Dinheiro atualizado: {money}");
    }

    private void HandleReputationChanged(int reputation)
    {
        Debug.Log($"[Observer] Reputação atualizada: {reputation}");
    }

    private void HandleMoneyInsufficient()
    {
        Debug.Log("[Observer] Dinheiro insuficiente.");
    }

    private void HandleReputationReachedZero()
    {
        Debug.Log("[Observer] Reputação chegou a zero. Futuro Game Over.");
    }

    private void HandleDayChanged(int day)
    {
        Debug.Log($"[Observer] Dia atual: {day}");
    }

    private void HandleDayEnded(int earnings, int wages, int profit)
    {
        Debug.Log($"[Observer] Dia encerrado. Faturamento: {earnings}, Salários: {wages}, Lucro: {profit}");
    }

    private void HandleTaskSpawned(TaskPin taskPin)
    {
        Debug.Log($"[Observer] Task spawnada: {taskPin.data.taskName}");
    }

    private void HandleTaskSelected(TaskPin taskPin)
    {
        if (taskPin == null || taskPin.data == null)
            return;

        Debug.Log($"[Observer] Task selecionada: {taskPin.data.taskName}");
    }

    private void HandleMissionStarted(TaskData task)
    {
        Debug.Log($"[Observer] Missão iniciada: {task.taskName}");
    }

    private void HandleMissionFinished(TaskData task, bool success, bool critical)
    {
        Debug.Log($"[Observer] Missão finalizada: {task.taskName}. Sucesso: {success}. Crítico: {critical}");
    }
}