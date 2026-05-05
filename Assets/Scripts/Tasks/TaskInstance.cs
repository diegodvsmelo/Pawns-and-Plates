using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskInstance
{
    public TaskData data;
    public TaskState state;

    public float remainingExpirationTime;
    public float remainingExecutionTime;

    public List<EmployeeData> assignedEmployees = new();

    public float chancePercent;
    public bool isCritical;

    public TaskInstance(TaskData data)
    {
        this.data = data;
        state = TaskState.Available;

        remainingExpirationTime = data != null ? data.expirationTime : 0f;
        remainingExecutionTime = data != null ? data.executionTime : 0f;
    }

    public bool CanExpire()
    {
        return data != null && data.expirationTime > 0f;
    }

    public bool IsAvailable()
    {
        return state == TaskState.Available;
    }

    public bool IsInProgress()
    {
        return state == TaskState.InProgress;
    }

    public bool IsReadyToCollect()
    {
        return state == TaskState.ReadyToCollect;
    }

    public void AssignEmployees(List<EmployeeData> employees)
    {
        assignedEmployees.Clear();

        if (employees == null)
            return;

        assignedEmployees.AddRange(employees);
    }
}