using UnityEngine;

public class TaskFlowManager : MonoBehaviour
{
    public static TaskFlowManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ResolveTask(TaskPin taskPin, bool success)
    {
        if (taskPin == null)
            return;

        Debug.Log($"TaskFlowManager temporário: task resolvida. Sucesso: {success}");

        taskPin.CompleteAndDestroy();
    }
}