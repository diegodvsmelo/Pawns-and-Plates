using System.Collections.Generic;
using UnityEngine;

public class EmployeeCardListUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private EmployeeCardUI cardPrefab;

    private readonly List<EmployeeCardUI> spawnedCards = new();

    private void Awake()
    {
        if (cardsContainer == null)
            cardsContainer = transform;
    }

    public void Rebuild(List<EmployeeData> employees)
    {
        Clear();

        if (employees == null)
            return;

        foreach (EmployeeData employee in employees)
        {
            if (employee == null)
                continue;

            EmployeeCardUI card = Instantiate(cardPrefab, cardsContainer);
            card.Setup(employee);
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

    public void Clear()
    {
        foreach (EmployeeCardUI card in spawnedCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        spawnedCards.Clear();
    }
}