using System.Collections.Generic;
using UnityEngine;

public class TaskHintsPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hintsContainer;
    [SerializeField] private TaskHintLineUI hintLinePrefab;

    [Header("Skill Colors")]
    [SerializeField] private Color cookingColor = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private Color serviceColor = new Color(0.3f, 0.7f, 1f);
    [SerializeField] private Color operationalColor = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private Color agilityColor = new Color(0.4f, 1f, 0.5f);

    private readonly List<TaskHintLineUI> spawnedLines = new();

    public void ShowHints(TaskData taskData)
    {
        ClearHints();

        if (hintsContainer == null)
        {
            Debug.LogWarning("[TaskHintsPanelUI] HintsContainer não está configurado.");
            return;
        }

        if (hintLinePrefab == null)
        {
            Debug.LogWarning("[TaskHintsPanelUI] HintLinePrefab não está configurado.");
            return;
        }

        if (taskData == null)
        {
            Debug.LogWarning("[TaskHintsPanelUI] TaskData recebido está null.");
            return;
        }

        if (taskData.taskHints == null || taskData.taskHints.Count == 0)
        {
            Debug.Log($"[TaskHintsPanelUI] Task '{taskData.taskName}' não possui hints configuradas.");
            return;
        }

        foreach (TaskHintLine hint in taskData.taskHints)
        {
            if (hint == null)
                continue;

            TaskHintLineUI line = Instantiate(hintLinePrefab, hintsContainer);
            line.Setup(hint, GetColorBySkill(hint.relatedSkill));
            spawnedLines.Add(line);
        }

        Debug.Log($"[TaskHintsPanelUI] {spawnedLines.Count} hint(s) instanciada(s) para task '{taskData.taskName}'.");
    }

    public void ClearHints()
    {
        for (int i = 0; i < spawnedLines.Count; i++)
        {
            if (spawnedLines[i] != null)
                Destroy(spawnedLines[i].gameObject);
        }

        spawnedLines.Clear();

        if (hintsContainer == null)
            return;

        for (int i = hintsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(hintsContainer.GetChild(i).gameObject);
        }
    }

    private Color GetColorBySkill(EmployeeSkillType skillType)
    {
        switch (skillType)
        {
            case EmployeeSkillType.Cooking:
                return cookingColor;

            case EmployeeSkillType.Service:
                return serviceColor;

            case EmployeeSkillType.Operational:
                return operationalColor;

            case EmployeeSkillType.Agility:
                return agilityColor;

            default:
                return Color.white;
        }
    }
}