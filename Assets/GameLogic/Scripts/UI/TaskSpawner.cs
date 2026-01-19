using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TaskSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject pinPrefab; 
    public RectTransform spawnArea; 
    
    [Header("Timers")]
    public float minSpawnTime = 2f;
    public float maxSpawnTime = 5f;

    [Header("Data Source")]
    public List<TaskData> possibleTasks;

    // Removemos a variável local de pausa. Usaremos a global.
    private bool isSpawningActive = false;
    private GameManager gameManager; 

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        StartSpawning();
    }

    public void StartSpawning()
    {
        if (!isSpawningActive)
        {
            isSpawningActive = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        isSpawningActive = false;
        StopAllCoroutines();
    }

    IEnumerator SpawnRoutine()
    {
        while (isSpawningActive)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            float counter = 0;
            
            // Loop que respeita a Pausa Global
            while (counter < waitTime)
            {
                // Só conta o tempo se o GameManager permitir
                if (gameManager != null && !gameManager.isGamePaused)
                {
                    counter += Time.deltaTime;
                }
                yield return null; 
            }

            // Só spawna se não estiver pausado na hora H
            if (isSpawningActive && gameManager != null && !gameManager.isGamePaused)
            {
                SpawnTask();
            }
        }
    }

    void SpawnTask()
    {
        if (possibleTasks.Count == 0) return;

        TaskData randomTask = possibleTasks[Random.Range(0, possibleTasks.Count)];
        GameObject newPin = Instantiate(pinPrefab, spawnArea);

        float width = spawnArea.rect.width;
        float height = spawnArea.rect.height;
        float randomX = Random.Range(-width / 2f, width / 2f);
        float randomY = Random.Range(-height / 2f, height / 2f);

        newPin.GetComponent<RectTransform>().anchoredPosition = new Vector2(randomX * 0.9f, randomY * 0.9f);

        TaskPin pinScript = newPin.GetComponent<TaskPin>();
        pinScript.Setup(randomTask, OnTaskPinClicked);
    }

    void OnTaskPinClicked(TaskData task)
    {
        Debug.Log($"Jogador clicou na missão: {task.taskName}");
        StopSpawning();
        gameManager.OpenMissionWindow(task);
    }
}