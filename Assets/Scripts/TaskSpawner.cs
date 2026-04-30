using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class TaskSpawner : MonoBehaviour
{
    public static TaskSpawner Instance { get; private set; }

    [Header("Settings")]
    public GameObject pinPrefab;
    public RectTransform spawnArea;

    [Header("Timers")]
    public float minSpawnTime = 2f;
    public float maxSpawnTime = 5f;

    [Header("Data Source")]
    public List<TaskData> possibleTasks;

    // OBSERVERS
    public event Action OnSpawningStarted;
    public event Action OnSpawningStopped;
    public event Action<TaskPin> OnTaskSpawned;
    public event Action<TaskData> OnTaskSelected;

    private bool isSpawningActive = false;
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
        Debug.Log(this.gameObject.name);
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
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            float counter = 0f;

            while (counter < waitTime)
            {
                if (gameManager != null && !gameManager.isGamePaused)
                {
                    counter += Time.deltaTime;
                }

                yield return null;
            }

            if (isSpawningActive && gameManager != null && !gameManager.isGamePaused)
            {
                SpawnTask();
            }
        }
    }

    private void SpawnTask()
    {
        if (possibleTasks == null || possibleTasks.Count == 0)
            return;

        if (pinPrefab == null || spawnArea == null)
        {
            Debug.LogWarning("TaskSpawner está sem pinPrefab ou spawnArea.");
            return;
        }

        TaskData randomTask = possibleTasks[Random.Range(0, possibleTasks.Count)];
        GameObject newPin = Instantiate(pinPrefab, spawnArea);

        float width = spawnArea.rect.width;
        float height = spawnArea.rect.height;

        float randomX = Random.Range(-width / 2f, width / 2f);
        float randomY = Random.Range(-height / 2f, height / 2f);

        RectTransform pinRect = newPin.GetComponent<RectTransform>();

        if (pinRect != null)
        {
            pinRect.anchoredPosition = new Vector2(randomX * 0.9f, randomY * 0.9f);
        }

        TaskPin pinScript = newPin.GetComponent<TaskPin>();

        if (pinScript != null)
        {
            pinScript.Setup(randomTask, OnTaskPinClicked);
            OnTaskSpawned?.Invoke(pinScript);
        }
    }

    private void OnTaskPinClicked(TaskData task)
    {
        Debug.Log($"Jogador clicou na missão: {task.taskName}");

        OnTaskSelected?.Invoke(task);

        StopSpawning();

        if (gameManager != null)
        {
            gameManager.OpenMissionWindow(task);
        }
    }
}