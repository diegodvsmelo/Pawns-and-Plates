using System.Collections.Generic;
using UnityEngine;

public class StaffSidebarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private EmployeeCardUI employeeCardPrefab;

    [Header("Rules")]
    [SerializeField] private int currentMaxEmployees = 3;

    private readonly List<EmployeeCardUI> spawnedCards = new();

    public void SetMaxEmployees(int maxEmployees)
    {
        currentMaxEmployees = Mathf.Max(0, maxEmployees);
    }

    public void Rebuild(List<EmployeeData> currentEmployees)
    {
        Clear();

        if (currentEmployees == null)
            return;

        int amountToShow = Mathf.Min(currentEmployees.Count, currentMaxEmployees);

        for (int i = 0; i < amountToShow; i++)
        {
            EmployeeCardUI card = Instantiate(employeeCardPrefab, cardsContainer);
            card.Setup(currentEmployees[i]);
            spawnedCards.Add(card);
        }
    }

    public void Refresh()
    {
        foreach (EmployeeCardUI card in spawnedCards)
        {
            if (card != null)
                card.Refresh();
        }
    }

    private void Clear()
    {
        foreach (EmployeeCardUI card in spawnedCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        spawnedCards.Clear();
    }
}