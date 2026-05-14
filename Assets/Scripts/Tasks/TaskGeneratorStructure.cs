using System.Collections;
using UnityEngine;

public class TaskGeneratorStructure : MonoBehaviour
{
    [Header("Identity")]
    public string structureName;
    public TaskGeneratorType generatorType;

    [Header("Spawn")]
    [SerializeField] private Transform pinContainer;
    [SerializeField] private bool allowMultiplePins = false;

    [Header("State")]
    [SerializeField] private StructureState currentState = StructureState.Available;

    [Header("Table Flow")]
    [SerializeField] private float eatingDuration = 8f;
    [SerializeField] private TaskData cleaningTaskData;

    [Header("Wear / Malfunction")]
    [SerializeField] private bool usesWearSystem = false;
    [SerializeField] private int currentWear = 0;
    [SerializeField] private int wearPerCookingTask = 10;
    [SerializeField] private int wearToStartRisk = 30;
    [SerializeField] private int maxWear = 100;

    [Header("Malfunction Rules")]
    [SerializeField] private MalfunctionMode malfunctionMode = MalfunctionMode.PenalizeNextTasks;
    [SerializeField] private float brokenDuration = 20f;
    [SerializeField] private Vector2Int penalizedTaskRange = new Vector2Int(1, 3);
    [Range(0f, 1f)]
    [SerializeField] private float attributePenaltyPercent = 0.25f;

    private TaskPin currentPin;
    private int remainingPenalizedTasks;

    private StructureVisualStateController visualStateController;
    private OrderRecipeData currentEatingOrderRecipe;
    private OrderRecipeData currentDirtyOrderRecipe;

    public Transform PinContainer => pinContainer;
    public StructureState CurrentState => currentState;
    public bool IsBroken => currentState == StructureState.Broken || currentState == StructureState.Disabled;
    public float AttributePenaltyPercent => remainingPenalizedTasks > 0 ? attributePenaltyPercent : 0f;
    public float EatingDuration => eatingDuration;
    public TaskData CleaningTaskData => cleaningTaskData;

    private void Awake()
    {
        visualStateController = GetComponent<StructureVisualStateController>();
    }

    public bool CanReceiveTask(TaskType taskType)
    {
        if (pinContainer == null)
            return false;

        if (!allowMultiplePins && currentPin != null)
            return false;

        if (currentState == StructureState.Broken || currentState == StructureState.Disabled)
            return false;

        if (taskType == TaskType.Service)
        {
            // Service só deve entrar em estruturas realmente disponíveis.
            if (currentState != StructureState.Available)
                return false;

            return generatorType == TaskGeneratorType.Cashier ||
                   generatorType == TaskGeneratorType.Table;
        }

        if (taskType == TaskType.Cooking)
        {
            if (currentState != StructureState.Available)
                return false;

            return generatorType == TaskGeneratorType.Oven ||
                   generatorType == TaskGeneratorType.Grill ||
                   generatorType == TaskGeneratorType.Stove ||
                   generatorType == TaskGeneratorType.Counter;
        }

        if (taskType == TaskType.Operational)
        {
            // Operational pode acontecer quando a estrutura estiver Available ou Dirty.
            // Dirty é o caso da task de limpeza da mesa.
            return currentState == StructureState.Available ||
                   currentState == StructureState.Dirty;
        }

        return false;
    }

    public void RegisterPin(TaskPin pin)
    {
        currentPin = pin;

        if (currentState == StructureState.Available)
            currentState = StructureState.Occupied;
    }

    public void ClearPin(TaskPin pin)
    {
        if (currentPin == pin)
            currentPin = null;

        if (currentState == StructureState.Occupied)
            currentState = StructureState.Available;
    }

    public void SetState(StructureState newState)
    {
        currentState = newState;

        if (visualStateController != null)
        {
            if (currentState == StructureState.Eating && currentEatingOrderRecipe != null)
                visualStateController.ShowEatingOrder(currentEatingOrderRecipe);
            else
                visualStateController.ClearEatingOrder();

            if (currentState == StructureState.Dirty && currentDirtyOrderRecipe != null)
                visualStateController.ShowDirtyOrder(currentDirtyOrderRecipe);
            else
                visualStateController.ClearDirtyOrder();

            visualStateController.RefreshByStructureState();
        }
    }

    public void ShowDeliveredOrderVisual(OrderRecipeData recipeData)
    {
        currentEatingOrderRecipe = recipeData;
        currentDirtyOrderRecipe = recipeData;

        if (currentState == StructureState.Eating && visualStateController != null)
            visualStateController.ShowEatingOrder(recipeData);
    }

    public void ClearDeliveredOrderVisual()
    {
        currentEatingOrderRecipe = null;

        if (visualStateController != null)
            visualStateController.ClearEatingOrder();
    }

    public void AddCookingWear()
    {
        if (!usesWearSystem)
            return;

        currentWear += wearPerCookingTask;
        currentWear = Mathf.Clamp(currentWear, 0, maxWear);

        TryRollMalfunction();
    }

    public void ShowDirtyOrderVisual()
    {
        if (visualStateController == null)
            return;

        if (currentDirtyOrderRecipe != null && currentState == StructureState.Dirty)
            visualStateController.ShowDirtyOrder(currentDirtyOrderRecipe);
    }

    public void ClearAllOrderVisuals()
    {
        currentEatingOrderRecipe = null;
        currentDirtyOrderRecipe = null;

        if (visualStateController != null)
            visualStateController.ClearAllOrderVisuals();
    }
    
    private void TryRollMalfunction()
    {
        if (currentWear < wearToStartRisk)
            return;

        float risk = (float)currentWear / maxWear;
        float roll = Random.value;

        if (roll <= risk)
            TriggerMalfunction();
    }

    private void TriggerMalfunction()
    {
        currentWear = 0;

        if (malfunctionMode == MalfunctionMode.None)
            return;

        if (malfunctionMode == MalfunctionMode.BlockForSeconds)
        {
            StartCoroutine(BlockTemporarily());
            return;
        }

        if (malfunctionMode == MalfunctionMode.PenalizeNextTasks)
        {
            remainingPenalizedTasks = Random.Range(
                penalizedTaskRange.x,
                penalizedTaskRange.y + 1
            );
        }
    }

    private IEnumerator BlockTemporarily()
    {
        currentState = StructureState.Broken;

        yield return new WaitForSeconds(brokenDuration);

        if (currentState == StructureState.Broken)
            currentState = StructureState.Available;
    }

    public float PreviewStructurePenalty(float originalValue)
    {
        if (remainingPenalizedTasks <= 0)
            return originalValue;

        float multiplier = 1f - attributePenaltyPercent;
        return originalValue * multiplier;
    }

    public float ConsumeStructurePenalty(float originalValue)
    {
        if (remainingPenalizedTasks <= 0)
            return originalValue;

        float multiplier = 1f - attributePenaltyPercent;
        float modifiedValue = originalValue * multiplier;

        remainingPenalizedTasks--;

        return modifiedValue;
    }

    public int PreviewStructurePenalty(int originalValue)
    {
        return Mathf.FloorToInt(PreviewStructurePenalty((float)originalValue));
    }

    public int ConsumeStructurePenalty(int originalValue)
    {
        return Mathf.FloorToInt(ConsumeStructurePenalty((float)originalValue));
    }

    public void Repair()
    {
        currentWear = 0;
        remainingPenalizedTasks = 0;

        if (currentState == StructureState.Broken || currentState == StructureState.Disabled)
            currentState = StructureState.Available;
    }
}