using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TaskSpawner : MonoBehaviour
{
    public static TaskSpawner Instance { get; private set; }

    [Header("Prefab")]
    [SerializeField] private GameObject pinPrefab;

    [Header("Task Generator Structures")]
    [SerializeField] private List<TaskGeneratorStructure> taskGenerators = new();

    [Header("Tasks")]
    [SerializeField] private List<TaskData> possibleTasks = new();

    [Header("Timers")]
    [SerializeField] private float minSpawnTime = 2f;
    [SerializeField] private float maxSpawnTime = 5f;

    [Header("Task Team Selection")]
    [SerializeField] private TaskTeamSelectionUI taskTeamSelectionUI;

    [Header("Task Result Popup")]
    [SerializeField] private TaskResultPopupUI taskResultPopupUI;

    public event Action OnSpawningStarted;
    public event Action OnSpawningStopped;
    public event Action<TaskPin> OnTaskSpawned;
    public event Action<TaskPin> OnTaskSelected;
    private bool isSpawningActive;
    private bool isResultPopupOpen;
    private GameManager gameManager;
    private Coroutine spawnCoroutine;
    private TaskPin pendingResolvedPin;

    public static bool IsResultPopupOpenGlobally
    {
        get
        {
            return Instance != null && Instance.isResultPopupOpen;
        }
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

    private void ReleaseAssignedEmployees(TaskInstance instance)
    {
        if (instance == null || instance.assignedEmployees == null)
            return;

        foreach (EmployeeData employee in instance.assignedEmployees)
        {
            if (employee == null)
                continue;

            employee.SetAvailable();
        }
    }
    private void Start()
    {
        gameManager = GameManager.Instance;
        StartSpawning();
    }

    public void StartSpawning()
    {
        if (isSpawningActive)
            return;

        isSpawningActive = true;
        spawnCoroutine = StartCoroutine(SpawnRoutine());

        OnSpawningStarted?.Invoke();
    }

    public void StopSpawning()
    {
        if (!isSpawningActive)
            return;

        isSpawningActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        OnSpawningStopped?.Invoke();
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawningActive)
        {
            float waitTime = UnityEngine.Random.Range(minSpawnTime, maxSpawnTime);
            float counter = 0f;

            while (counter < waitTime)
            {
                if ((gameManager == null || !gameManager.isGamePaused) && !isResultPopupOpen)
                    counter += Time.deltaTime;

                yield return null;
            }

            if (isSpawningActive &&
                (gameManager == null || !gameManager.isGamePaused) &&
                !isResultPopupOpen)
            {
                SpawnRandomTaskOnStructure();
            }
        }
    }

    private void SpawnRandomTaskOnStructure()
    {
        if (pinPrefab == null)
        {
            Debug.LogWarning("TaskSpawner está sem pinPrefab.");
            return;
        }

        if (possibleTasks == null || possibleTasks.Count == 0)
        {
            Debug.LogWarning("TaskSpawner não possui possibleTasks.");
            return;
        }

        if (taskGenerators == null || taskGenerators.Count == 0)
        {
            Debug.LogWarning("TaskSpawner não possui estruturas geradoras.");
            return;
        }

        List<TaskSpawnOption> validOptions = GetValidSpawnOptions();

        if (validOptions.Count == 0)
        {
            Debug.Log("Nenhuma estrutura disponível para spawnar task no momento.");
            return;
        }

        TaskSpawnOption chosenOption = validOptions[UnityEngine.Random.Range(0, validOptions.Count)];

        SpawnTaskOnStructure(chosenOption.taskData, chosenOption.structure);
    }

    private List<TaskSpawnOption> GetValidSpawnOptions()
    {
        List<TaskSpawnOption> validOptions = new();

        foreach (TaskData task in possibleTasks)
        {
            if (task == null)
                continue;

            foreach (TaskGeneratorStructure structure in taskGenerators)
            {
                if (structure == null)
                    continue;

                if (structure.CanReceiveTask(task.taskType))
                    validOptions.Add(new TaskSpawnOption(task, structure));
            }
        }

        return validOptions;
    }

    private TaskPin SpawnTaskOnStructure(TaskData taskData, TaskGeneratorStructure structure)
    {
        if (taskData == null || structure == null)
            return null;

        if (!structure.CanReceiveTask(taskData.taskType))
            return null;

        if (structure.PinContainer == null)
        {
            Debug.LogWarning($"Estrutura {structure.structureName} está sem PinContainer.");
            return null;
        }

        GameObject newPin = Instantiate(pinPrefab, structure.PinContainer);

        RectTransform pinRect = newPin.GetComponent<RectTransform>();

        if (pinRect != null)
        {
            pinRect.anchoredPosition = Vector2.zero;
            pinRect.localRotation = Quaternion.identity;
            pinRect.localScale = Vector3.one;
        }
        else
        {
            newPin.transform.localPosition = Vector3.zero;
            newPin.transform.localRotation = Quaternion.identity;
            newPin.transform.localScale = Vector3.one;
        }

        TaskPin pinScript = newPin.GetComponent<TaskPin>();

        if (pinScript == null)
        {
            Debug.LogWarning("Prefab de TaskPin não possui componente TaskPin.");
            Destroy(newPin);
            return null;
        }

        pinScript.Setup(taskData, OnTaskPinClicked);

        structure.RegisterPin(pinScript);

        TaskPinWorldBinding binding = newPin.GetComponent<TaskPinWorldBinding>();

        if (binding == null)
            binding = newPin.AddComponent<TaskPinWorldBinding>();

        binding.Setup(structure, pinScript);

        OnTaskSpawned?.Invoke(pinScript);

        Debug.Log($"Spawnou task '{taskData.taskName}' em '{structure.structureName}'.");

        return pinScript;
    }

    private void OnTaskPinClicked(TaskPin taskPin)
    {
        if (isResultPopupOpen)
            return;

        if (taskPin == null || taskPin.data == null || taskPin.Instance == null)
            return;

        if (taskPin.CurrentState == TaskState.Available)
        {
            Debug.Log($"Abrindo seleção de equipe para task: {taskPin.data.taskName}");

            if (taskTeamSelectionUI != null)
            {
                taskTeamSelectionUI.Open(taskPin);
                OnTaskSelected?.Invoke(taskPin);
            }
            else
            {
                Debug.LogWarning("TaskSpawner está sem referência para TaskTeamSelectionUI.");
            }

            return;
        }

        if (taskPin.CurrentState == TaskState.InProgress)
        {
            Debug.Log($"Task em andamento: {taskPin.data.taskName}. Aguarde terminar.");
            return;
        }

        if (taskPin.CurrentState == TaskState.ReadyToCollect)
        {
            CollectFinishedTask(taskPin);
            return;
        }

        if (taskPin.CurrentState == TaskState.Expired)
        {
            CollectExpiredTask(taskPin);
            return;
        }
    }

    private void CollectFinishedTask(TaskPin taskPin)
    {
        if (taskPin == null || taskPin.Instance == null || taskPin.data == null)
            return;

        TaskInstance instance = taskPin.Instance;

        if (!instance.hasRolledResult && instance.chancePercent <= 0f && instance.assignedEmployees.Count > 0)
            instance.CalculateAndStoreSuccessChance();

        bool wasSuccessful = instance.RollSuccessIfNeeded();

        Debug.Log(
            $"Task '{taskPin.data.taskName}' coletada. " +
            $"Chance: {instance.chancePercent:F1}% | " +
            $"Roll: {instance.rolledValue:F1} | " +
            $"Success: {wasSuccessful} | " +
            $"Critical: {instance.isCritical}"
        );

        if (wasSuccessful)
            ApplySuccessReward(instance);
        else
            ApplyFailurePenalty(instance);

        ApplyTaskExperience(instance, wasSuccessful);

        if (taskResultPopupUI != null)
        {
            isResultPopupOpen = true;
            pendingResolvedPin = taskPin;
            taskResultPopupUI.ShowTaskResult(instance, wasSuccessful, HandleResultPopupClosed);
        }
        else
        {
            ReleaseAssignedEmployees(taskPin.Instance);
            taskPin.CompleteAndDestroy();
        }
    }

    private void CollectExpiredTask(TaskPin taskPin)
    {
        if (taskPin == null || taskPin.Instance == null || taskPin.data == null)
            return;

        Debug.Log($"Task expirada coletada: {taskPin.data.taskName}. Aplicando penalidade.");

        ApplyFailurePenalty(taskPin.Instance);

        if (taskResultPopupUI != null)
        {
            isResultPopupOpen = true;
            pendingResolvedPin = taskPin;
            taskResultPopupUI.ShowExpiredResult(taskPin.data, HandleResultPopupClosed);
        }
        else
        {
            ReleaseAssignedEmployees(taskPin.Instance);
            taskPin.CompleteAndDestroy();
        }
    }

    private void HandleResultPopupClosed()
    {
        isResultPopupOpen = false;

        if (pendingResolvedPin != null)
        {
            ReleaseAssignedEmployees(pendingResolvedPin.Instance);
            pendingResolvedPin.CompleteAndDestroy();
            pendingResolvedPin = null;
        }
    }

    private void ApplySuccessReward(TaskInstance instance)
    {
        if (instance == null || instance.data == null)
            return;

        ResourceManager resourceManager = ResourceManager.Instance;

        if (resourceManager == null)
        {
            Debug.LogWarning("Não foi possível aplicar recompensa: ResourceManager não encontrado.");
            return;
        }

        bool isCritical = instance.isCritical;

        int moneyReward = instance.data.GetTotalMoneyReward(isCritical);
        int reputationReward = instance.data.GetTotalReputationReward(isCritical);

        if (moneyReward != 0)
            resourceManager.ModifyMoney(moneyReward);

        if (reputationReward != 0)
            resourceManager.ModifyReputation(reputationReward);

        if (DayCycleManager.Instance != null && moneyReward > 0)
            DayCycleManager.Instance.AddDailyEarnings(moneyReward);

        Debug.Log(
            $"Recompensa aplicada: +${moneyReward}, +{reputationReward} reputação. " +
            $"Critical: {isCritical}"
        );
    }

    private void ApplyFailurePenalty(TaskInstance instance)
    {
        if (instance == null || instance.data == null)
            return;

        ResourceManager resourceManager = ResourceManager.Instance;

        if (resourceManager == null)
        {
            Debug.LogWarning("Não foi possível aplicar penalidade: ResourceManager não encontrado.");
            return;
        }

        if (instance.data.reputationPenalty != 0)
            resourceManager.ModifyReputation(-instance.data.reputationPenalty);

        Debug.Log($"Penalidade aplicada: -{instance.data.reputationPenalty} reputação.");
    }

    private void ApplyTaskExperience(TaskInstance instance, bool wasSuccessful)
    {
        if (instance == null || instance.data == null || instance.assignedEmployees == null || instance.assignedEmployees.Count == 0)
            return;

        int xpToGive = wasSuccessful
            ? instance.data.GetSuccessXP(instance.isCritical)
            : instance.data.xpOnFailure;

        if (xpToGive <= 0)
            return;

        foreach (EmployeeData employee in instance.assignedEmployees)
        {
            if (employee == null)
                continue;

            employee.AddExperience(xpToGive);
        }

        Debug.Log(
            $"XP aplicado aos funcionários da task '{instance.data.taskName}': " +
            $"{xpToGive} XP para cada membro."
        );
    }

    private struct TaskSpawnOption
    {
        public TaskData taskData;
        public TaskGeneratorStructure structure;

        public TaskSpawnOption(TaskData taskData, TaskGeneratorStructure structure)
        {
            this.taskData = taskData;
            this.structure = structure;
        }
    }
}