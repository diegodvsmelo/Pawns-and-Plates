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

    public event Action OnSpawningStarted;
    public event Action OnSpawningStopped;
    public event Action<TaskPin> OnTaskSpawned;
    public event Action<TaskPin> OnTaskSelected;

    private bool isSpawningActive;
    private GameManager gameManager;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
                if (gameManager == null || !gameManager.isGamePaused)
                    counter += Time.deltaTime;

                yield return null;
            }

            if (isSpawningActive && (gameManager == null || !gameManager.isGamePaused))
                SpawnRandomTaskOnStructure();
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
                {
                    validOptions.Add(new TaskSpawnOption(task, structure));
                }
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
        if (taskPin == null || taskPin.data == null)
            return;

        Debug.Log($"Clicou na task: {taskPin.data.taskName}. Concluindo task para teste.");

        OnTaskSelected?.Invoke(taskPin);

        taskPin.CompleteAndDestroy();
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