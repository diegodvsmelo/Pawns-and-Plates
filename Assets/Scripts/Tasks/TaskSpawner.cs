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
    private bool isBoundToTaskFlowManager;
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

    private void OnEnable()
    {
        TryBindTaskFlowManagerEvents();
    }

    private void OnDisable()
    {
        UnbindTaskFlowManagerEvents();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        TryBindTaskFlowManagerEvents();
        StartSpawning();
    }

    private void TryBindTaskFlowManagerEvents()
    {
        if (isBoundToTaskFlowManager)
            return;

        if (TaskFlowManager.Instance == null)
            return;

        TaskFlowManager.Instance.OnOrderGenerationRequested += HandleOrderGenerationRequested;
        isBoundToTaskFlowManager = true;
    }

    private void UnbindTaskFlowManagerEvents()
    {
        if (!isBoundToTaskFlowManager)
            return;

        if (TaskFlowManager.Instance != null)
            TaskFlowManager.Instance.OnOrderGenerationRequested -= HandleOrderGenerationRequested;

        isBoundToTaskFlowManager = false;
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
            return;

        if (possibleTasks == null || possibleTasks.Count == 0)
            return;

        if (taskGenerators == null || taskGenerators.Count == 0)
            return;

        List<TaskSpawnOption> validOptions = GetValidSpawnOptions();

        if (validOptions.Count == 0)
            return;

        TaskSpawnOption chosenOption = validOptions[UnityEngine.Random.Range(0, validOptions.Count)];
        SpawnTaskOnStructure(chosenOption.taskData, chosenOption.structure);
    }

    private List<TaskSpawnOption> GetValidSpawnOptions()
    {
        List<TaskSpawnOption> validOptions = new();

        foreach (TaskData task in possibleTasks)
        {
            if (task == null || !task.canSpawnRandomly)
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

    public TaskPin SpawnSpecificTaskOnStructure(TaskData taskData, TaskGeneratorStructure structure)
    {
        return SpawnTaskOnStructure(taskData, structure);
    }

    public bool TrySpawnCookingTaskForOrder(RestaurantOrder order)
    {
        if (order == null)
            return false;

        if (order.sourceData == null)
            return false;

        if (order.cookingTaskData == null)
        {
            Debug.LogWarning($"[TaskSpawner] Pedido '{order.orderName}' está sem cookingTaskData na receita.");
            return false;
        }

        if (order.cookingTaskData.taskType != TaskType.Cooking)
        {
            Debug.LogWarning($"[TaskSpawner] A task '{order.cookingTaskData.taskName}' não é do tipo Cooking.");
            return false;
        }

        TaskGeneratorStructure cookingStructure = FindFirstAvailableStructure(
            order.requiredCookingStructure,
            TaskType.Cooking
        );

        if (cookingStructure == null)
            return false;

        TaskPin cookingPin = SpawnSpecificTaskOnStructure(order.cookingTaskData, cookingStructure);

        if (cookingPin == null)
            return false;

        order.cookingStructure = cookingStructure;

        if (cookingPin.Instance != null)
            cookingPin.Instance.SetLinkedOrder(order);

        if (TaskFlowManager.Instance != null)
            TaskFlowManager.Instance.RegisterOrder(cookingPin, order);

        return true;
    }

    private TaskPin SpawnTaskOnStructure(TaskData taskData, TaskGeneratorStructure structure)
    {
        if (taskData == null || structure == null)
            return null;

        if (!structure.CanReceiveTask(taskData.taskType))
            return null;

        if (structure.PinContainer == null)
            return null;

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
            if (taskTeamSelectionUI != null)
            {
                taskTeamSelectionUI.Open(taskPin);
                OnTaskSelected?.Invoke(taskPin);
            }

            return;
        }

        if (taskPin.CurrentState == TaskState.InProgress)
            return;

        if (taskPin.CurrentState == TaskState.ReadyToCollect)
        {
            CollectFinishedTask(taskPin);
            return;
        }

        if (taskPin.CurrentState == TaskState.Expired)
            CollectExpiredTask(taskPin);
    }

    private void CollectFinishedTask(TaskPin taskPin)
    {
        if (taskPin == null || taskPin.Instance == null || taskPin.data == null)
            return;

        TaskInstance instance = taskPin.Instance;

        if (!instance.hasRolledResult && instance.chancePercent <= 0f && instance.assignedEmployees.Count > 0)
            instance.CalculateAndStoreSuccessChance();

        bool wasSuccessful = instance.RollSuccessIfNeeded();

        if (wasSuccessful)
            ApplySuccessReward(instance);
        else
            ApplyFailurePenalty(instance);

        ApplyTaskExperience(instance, wasSuccessful);

        if (TaskFlowManager.Instance != null)
            TaskFlowManager.Instance.ResolveTask(taskPin, wasSuccessful);

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

        ApplyFailurePenalty(taskPin.Instance);

        if (TaskFlowManager.Instance != null)
            TaskFlowManager.Instance.ResolveExpiredTask(taskPin);

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

            TaskGeneratorStructure structure = pendingResolvedPin.GetComponentInParent<TaskGeneratorStructure>();
            TaskGeneratorType freedType = structure != null ? structure.generatorType : TaskGeneratorType.GenericOperational;

            // Libera explicitamente o slot da estrutura ANTES de tentar puxar pedido pendente
            if (structure != null)
                structure.ClearPin(pendingResolvedPin);

            TaskPin resolvedPin = pendingResolvedPin;
            pendingResolvedPin = null;

            resolvedPin.CompleteAndDestroy();

            if (TaskFlowManager.Instance != null)
                TaskFlowManager.Instance.TryDispatchPendingOrders(freedType);
        }
    }

    private void ApplySuccessReward(TaskInstance instance)
    {
        if (instance == null || instance.data == null)
            return;

        ResourceManager resourceManager = ResourceManager.Instance;

        if (resourceManager == null)
            return;

        bool isCritical = instance.isCritical;

        int moneyReward = instance.data.GetTotalMoneyReward(isCritical);
        int reputationReward = instance.data.GetTotalReputationReward(isCritical);

        if (moneyReward != 0)
            resourceManager.ModifyMoney(moneyReward);

        if (reputationReward != 0)
            resourceManager.ModifyReputation(reputationReward);

        if (DayCycleManager.Instance != null && moneyReward > 0)
            DayCycleManager.Instance.AddDailyEarnings(moneyReward);
    }

    private void ApplyFailurePenalty(TaskInstance instance)
    {
        if (instance == null || instance.data == null)
            return;

        ResourceManager resourceManager = ResourceManager.Instance;

        if (resourceManager == null)
            return;

        if (instance.data.reputationPenalty != 0)
            resourceManager.ModifyReputation(-instance.data.reputationPenalty);
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
    }

    private void HandleOrderGenerationRequested(TaskPin serviceTaskPin, TaskGeneratorStructure originStructure)
    {
        if (serviceTaskPin == null || serviceTaskPin.data == null)
            return;

        TaskData serviceTaskData = serviceTaskPin.data;

        OrderRecipeData orderData = PickRandomGeneratedOrder(serviceTaskData);

        if (orderData == null)
            return;

        bool serviceSuccess = serviceTaskPin.Instance != null && serviceTaskPin.Instance.wasSuccessful;
        TaskGeneratorStructure cookingStructure = FindFirstAvailableStructure(orderData.requiredCookingStructure, TaskType.Cooking);

        RestaurantOrder runtimeOrder = orderData.CreateRuntimeOrder(originStructure, cookingStructure, serviceSuccess);

        if (cookingStructure == null)
        {
            if (TaskFlowManager.Instance != null)
                TaskFlowManager.Instance.EnqueuePendingOrder(runtimeOrder);

            return;
        }

        bool spawned = TrySpawnCookingTaskForOrder(runtimeOrder);

        if (!spawned && TaskFlowManager.Instance != null)
            TaskFlowManager.Instance.EnqueuePendingOrder(runtimeOrder);
    }

    private OrderRecipeData PickRandomGeneratedOrder(TaskData serviceTaskData)
    {
        if (serviceTaskData == null ||
            serviceTaskData.possibleGeneratedOrders == null ||
            serviceTaskData.possibleGeneratedOrders.Count == 0)
        {
            return null;
        }

        List<OrderRecipeData> validOrders = new();

        foreach (OrderRecipeData order in serviceTaskData.possibleGeneratedOrders)
        {
            if (order != null)
                validOrders.Add(order);
        }

        if (validOrders.Count == 0)
            return null;

        return validOrders[UnityEngine.Random.Range(0, validOrders.Count)];
    }

    private TaskGeneratorStructure FindFirstAvailableStructure(TaskGeneratorType generatorType, TaskType taskType)
    {
        foreach (TaskGeneratorStructure structure in taskGenerators)
        {
            if (structure == null)
                continue;

            if (structure.generatorType != generatorType)
                continue;

            if (!structure.CanReceiveTask(taskType))
                continue;

            return structure;
        }

        return null;
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

    public bool IsSpawningActive()
    {
        return isSpawningActive;
    }

    public bool HasActiveTasks()
    {
        TaskPin[] allPins = FindObjectsByType<TaskPin>(FindObjectsSortMode.None);

        for (int i = 0; i < allPins.Length; i++)
        {
            if (allPins[i] == null)
                continue;

            if (allPins[i].CurrentState == TaskState.Completed)
                continue;

            return true;
        }

        return false;
    }

}