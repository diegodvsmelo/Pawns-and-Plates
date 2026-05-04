using UnityEngine;

public class TaskPinWorldBinding : MonoBehaviour
{
    private TaskGeneratorStructure ownerStructure;
    private TaskPin taskPin;

    public void Setup(TaskGeneratorStructure structure, TaskPin pin)
    {
        ownerStructure = structure;
        taskPin = pin;
    }

    private void OnDestroy()
    {
        if (ownerStructure != null && taskPin != null)
            ownerStructure.ClearPin(taskPin);
    }
}