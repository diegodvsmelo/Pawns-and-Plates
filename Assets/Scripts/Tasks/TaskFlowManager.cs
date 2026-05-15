using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskFlowManager : MonoBehaviour
{
    public static TaskFlowManager Instance { get; private set; }

    [Header("Pending Orders UI")]
    [SerializeField] private PendingOrdersPanelUI pendingOrdersPanelUI;

    public event Action<TaskPin, TaskGeneratorStructure> OnOrderGenerationRequested;
    public event Action<TaskPin, TaskGeneratorStructure> OnSinkCleaningRequested;
    public event Action<TaskPin, RestaurantOrder> OnOrderDeliveryResolved;

    private readonly Dictionary<TaskPin, RestaurantOrder> ordersByTaskPin = new();
    private readonly List<PendingOrderTicket> pendingOrders = new();
    private readonly Dictionary<TaskGeneratorStructure, Coroutine> activeEatingCoroutines = new();

    public IReadOnlyList<PendingOrderTicket> PendingOrders => pendingOrders;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (ShouldPausePendingOrderTimers())
            return;

        TickPendingOrders();
    }

    private bool ShouldPausePendingOrderTimers()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
            return true;

        if (TaskSpawner.IsResultPopupOpenGlobally)
            return true;

        return false;
    }

    public void ResolveTask(TaskPin taskPin, bool success)
    {
        if (taskPin == null || taskPin.Instance == null || taskPin.data == null)
            return;

        TaskGeneratorStructure originStructure = GetOriginStructure(taskPin);

        if (success)
            ResolveSuccessFlow(taskPin, originStructure);
        else
            ResolveFailureFlow(taskPin, originStructure);
    }

    public void ResolveExpiredTask(TaskPin taskPin)
    {
        if (taskPin == null || taskPin.Instance == null || taskPin.data == null)
            return;

        TaskGeneratorStructure originStructure = GetOriginStructure(taskPin);
        ResolveExpiredFlow(taskPin, originStructure);
    }

    public void RegisterOrder(TaskPin taskPin, RestaurantOrder order)
    {
        if (taskPin == null || order == null)
            return;

        ordersByTaskPin[taskPin] = order;

        if (taskPin.Instance != null)
            taskPin.Instance.SetLinkedOrder(order);
    }

    public bool TryGetRegisteredOrder(TaskPin taskPin, out RestaurantOrder order)
    {
        if (taskPin != null && ordersByTaskPin.TryGetValue(taskPin, out order))
            return true;

        order = null;
        return false;
    }

    public void ClearRegisteredOrder(TaskPin taskPin)
    {
        if (taskPin == null)
            return;

        ordersByTaskPin.Remove(taskPin);

        if (taskPin.Instance != null)
            taskPin.Instance.ClearLinkedOrder();
    }

    public void EnqueuePendingOrder(RestaurantOrder order)
    {
        if (order == null)
            return;

        PendingOrderTicket ticket = new PendingOrderTicket(order);
        pendingOrders.Add(ticket);
        RefreshPendingOrdersUI();
    }

    public void TryDispatchPendingOrders(TaskGeneratorType freedStructureType)
    {
        if (TaskSpawner.Instance == null)
            return;

        for (int i = 0; i < pendingOrders.Count; i++)
        {
            PendingOrderTicket ticket = pendingOrders[i];

            if (ticket == null || ticket.order == null)
                continue;

            if (ticket.order.requiredCookingStructure != freedStructureType)
                continue;

            bool dispatched = TaskSpawner.Instance.TrySpawnCookingTaskForOrder(ticket.order);

            if (!dispatched)
                continue;

            pendingOrders.RemoveAt(i);
            RefreshPendingOrdersUI();
            return;
        }
    }

    private void ResolveSuccessFlow(TaskPin taskPin, TaskGeneratorStructure originStructure)
    {
        switch (taskPin.data.outcomeFlow)
        {
            case TaskOutcomeFlow.None:
                break;

            case TaskOutcomeFlow.GenerateOrder:
                HandleGenerateOrder(taskPin, originStructure);
                break;

            case TaskOutcomeFlow.DeliverOrder:
                HandleDeliverOrder(taskPin, originStructure);
                break;

            case TaskOutcomeFlow.GenerateSinkCleaning:
                HandleGenerateSinkCleaning(taskPin, originStructure);
                break;

            case TaskOutcomeFlow.CleanStructure:
                HandleCleanStructure(originStructure);
                break;

            case TaskOutcomeFlow.RepairStructure:
                HandleRepairStructure(originStructure);
                break;
        }
    }

    private void ResolveFailureFlow(TaskPin taskPin, TaskGeneratorStructure originStructure)
    {
        switch (taskPin.data.outcomeFlow)
        {
            case TaskOutcomeFlow.GenerateOrder:
                HandleGenerateOrder(taskPin, originStructure);
                break;

            case TaskOutcomeFlow.DeliverOrder:
                HandleFailedDelivery(taskPin);
                break;

            case TaskOutcomeFlow.CleanStructure:
                HandleFailedCleanStructure(originStructure);
                break;

            default:
                break;
        }
    }

    private void HandleFailedCleanStructure(TaskGeneratorStructure structure)
    {
        if (structure == null)
            return;

        StopEatingTimer(structure);
        structure.ClearAllOrderVisuals();
        structure.SetState(StructureState.Available);
    }

    private void ResolveExpiredFlow(TaskPin taskPin, TaskGeneratorStructure originStructure)
    {
        switch (taskPin.data.outcomeFlow)
        {
            case TaskOutcomeFlow.GenerateOrder:
                Debug.Log("[TaskFlowManager] Service task expirou antes do atendimento. Cliente saiu sem pedir.");

                if (originStructure != null && originStructure.CurrentState != StructureState.Eating)
                {
                    originStructure.ClearAllOrderVisuals();
                    originStructure.SetState(StructureState.Available);
                }
                break;

            case TaskOutcomeFlow.DeliverOrder:
                HandleFailedDelivery(taskPin);
                break;

            case TaskOutcomeFlow.CleanStructure:
                HandleFailedCleanStructure(originStructure);
                break;

            default:
                break;
        }
    }

    private void HandleGenerateOrder(TaskPin taskPin, TaskGeneratorStructure originStructure)
    {
        if (originStructure == null)
        {
            Debug.LogWarning("[TaskFlowManager] Não foi possível gerar pedido: estrutura de origem não encontrada.");
            return;
        }

        if (originStructure.generatorType == TaskGeneratorType.Table)
            originStructure.SetState(StructureState.WaitingForCooking);

        OnOrderGenerationRequested?.Invoke(taskPin, originStructure);
    }

    private void HandleDeliverOrder(TaskPin taskPin, TaskGeneratorStructure originStructure)
    {
        if (TryGetRegisteredOrder(taskPin, out RestaurantOrder order))
        {
            order.cookingWasSuccessful = true;

            if (order.originStructure != null &&
                order.originStructure.generatorType == TaskGeneratorType.Table)
            {
                order.originStructure.SetState(StructureState.Eating);
                order.originStructure.ShowDeliveredOrderVisual(order.sourceData);
                StartEatingTimer(order.originStructure);
            }

            if (order.cookingStructure != null)
                TryDispatchPendingOrders(order.cookingStructure.generatorType);

            OnOrderDeliveryResolved?.Invoke(taskPin, order);
            ClearRegisteredOrder(taskPin);

            return;
        }

        if (originStructure != null && originStructure.generatorType == TaskGeneratorType.Table)
        {
            originStructure.SetState(StructureState.Eating);
            StartEatingTimer(originStructure);
        }
    }

    private void HandleFailedDelivery(TaskPin taskPin)
    {
        if (TryGetRegisteredOrder(taskPin, out RestaurantOrder order))
        {
            if (order.originStructure != null &&
                order.originStructure.generatorType == TaskGeneratorType.Table)
            {
                order.originStructure.ClearAllOrderVisuals();
                order.originStructure.SetState(StructureState.Available);
            }

            if (order.cookingStructure != null)
                TryDispatchPendingOrders(order.cookingStructure.generatorType);

            ClearRegisteredOrder(taskPin);
        }
    }

    private void HandleGenerateSinkCleaning(TaskPin taskPin, TaskGeneratorStructure originStructure)
    {
        if (originStructure == null)
            return;

        OnSinkCleaningRequested?.Invoke(taskPin, originStructure);
    }

    private void HandleCleanStructure(TaskGeneratorStructure structure)
    {
        if (structure == null)
            return;

        StopEatingTimer(structure);
        structure.ClearAllOrderVisuals();
        structure.SetState(StructureState.Available);
    }

    private void HandleRepairStructure(TaskGeneratorStructure structure)
    {
        if (structure == null)
            return;

        structure.Repair();
    }

    private void TickPendingOrders()
    {
        if (pendingOrders.Count == 0)
            return;

        bool changed = false;

        if (TaskSpawner.Instance != null)
        {
            for (int i = 0; i < pendingOrders.Count; i++)
            {
                PendingOrderTicket ticket = pendingOrders[i];

                if (ticket == null || ticket.order == null)
                    continue;

                bool dispatched = TaskSpawner.Instance.TrySpawnCookingTaskForOrder(ticket.order);

                if (!dispatched)
                    continue;

                pendingOrders.RemoveAt(i);
                i--;
                changed = true;
            }
        }

        if (pendingOrders.Count == 0)
        {
            if (changed)
                RefreshPendingOrdersUI();

            return;
        }

        for (int i = pendingOrders.Count - 1; i >= 0; i--)
        {
            PendingOrderTicket ticket = pendingOrders[i];

            if (ticket == null || ticket.order == null)
            {
                pendingOrders.RemoveAt(i);
                changed = true;
                continue;
            }

            bool expired = ticket.Tick(Time.deltaTime);
            changed = true;

            if (!expired)
                continue;

            FailPendingOrder(ticket.order);
            pendingOrders.RemoveAt(i);
        }

        if (changed)
            RefreshPendingOrdersUI();
    }

    private void FailPendingOrder(RestaurantOrder order)
    {
        if (order == null)
            return;

        if (ResourceManager.Instance != null && order.pendingFailureReputationPenalty > 0)
            ResourceManager.Instance.ModifyReputation(-order.pendingFailureReputationPenalty);

        if (order.originStructure != null &&
            order.originStructure.generatorType == TaskGeneratorType.Table &&
            order.originStructure.CurrentState == StructureState.WaitingForCooking)
        {
            order.originStructure.ClearAllOrderVisuals();
            order.originStructure.SetState(StructureState.Available);
        }

        Debug.Log($"[TaskFlowManager] Pedido pendente '{order.orderName}' expirou na fila.");
    }

    private void StartEatingTimer(TaskGeneratorStructure structure)
    {
        if (structure == null)
            return;

        StopEatingTimer(structure);
        activeEatingCoroutines[structure] = StartCoroutine(EatingRoutine(structure));
    }

    private void StopEatingTimer(TaskGeneratorStructure structure)
    {
        if (structure == null)
            return;

        if (!activeEatingCoroutines.TryGetValue(structure, out Coroutine routine))
            return;

        if (routine != null)
            StopCoroutine(routine);

        activeEatingCoroutines.Remove(structure);
    }

    private IEnumerator EatingRoutine(TaskGeneratorStructure structure)
    {
        float remaining = structure.EatingDuration;

        while (remaining > 0f)
        {
            while (ShouldPausePendingOrderTimers())
                yield return null;

            remaining -= Time.deltaTime;
            yield return null;
        }

        activeEatingCoroutines.Remove(structure);

        if (structure == null)
            yield break;

        if (structure.CurrentState != StructureState.Eating)
            yield break;

        structure.ClearDeliveredOrderVisual();
        structure.SetState(StructureState.Dirty);
        structure.ShowDirtyOrderVisual();

        if (TaskSpawner.Instance != null && structure.CleaningTaskData != null)
            TaskSpawner.Instance.SpawnSpecificTaskOnStructure(structure.CleaningTaskData, structure);
    }

    private void RefreshPendingOrdersUI()
    {
        if (pendingOrdersPanelUI != null)
            pendingOrdersPanelUI.Refresh(pendingOrders);
    }

    private TaskGeneratorStructure GetOriginStructure(TaskPin taskPin)
    {
        if (taskPin == null)
            return null;

        return taskPin.GetComponentInParent<TaskGeneratorStructure>();
    }

    public bool HasUnfinishedRestaurantFlow()
    {
        if (pendingOrders.Count > 0)
            return true;

        if (activeEatingCoroutines.Count > 0)
            return true;

        TaskGeneratorStructure[] allStructures = FindObjectsByType<TaskGeneratorStructure>(FindObjectsSortMode.None);

        for (int i = 0; i < allStructures.Length; i++)
        {
            TaskGeneratorStructure structure = allStructures[i];

            if (structure == null)
                continue;

            if (structure.CurrentState == StructureState.WaitingForCooking ||
                structure.CurrentState == StructureState.Eating ||
                structure.CurrentState == StructureState.Dirty)
            {
                return true;
            }
        }

        return false;
    }
    
}