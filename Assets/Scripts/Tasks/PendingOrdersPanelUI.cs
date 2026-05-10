using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PendingOrdersPanelUI : MonoBehaviour
{
    [Header("Header Only")]
    [SerializeField] private GameObject headerRoot;

    [Header("Scroll Area")]
    [SerializeField] private GameObject scrollAreaRoot;

    [Header("Cards")]
    [SerializeField] private RectTransform cardsContainer;
    [SerializeField] private PendingOrderCardUI cardPrefab;

    [Header("Structure Icons")]
    [SerializeField] private Sprite cashierIcon;
    [SerializeField] private Sprite tableIcon;
    [SerializeField] private Sprite ovenIcon;
    [SerializeField] private Sprite grillIcon;
    [SerializeField] private Sprite stoveIcon;
    [SerializeField] private Sprite sinkIcon;
    [SerializeField] private Sprite counterIcon;
    [SerializeField] private Sprite genericOperationalIcon;

    private readonly List<PendingOrderTicket> displayedTickets = new();
    private readonly List<PendingOrderCardUI> spawnedCards = new();

    private void Awake()
    {
        SetVisible(false);
    }

    public void Refresh(IReadOnlyList<PendingOrderTicket> tickets)
    {
        if (tickets == null || tickets.Count == 0)
        {
            RemoveAllAnimated();
            return;
        }

        SetVisible(true);

        int previousCount = spawnedCards.Count;
        int sharedCount = Mathf.Min(tickets.Count, spawnedCards.Count);

        for (int i = 0; i < sharedCount; i++)
        {
            spawnedCards[i].Setup(tickets[i], GetStructureIcon(tickets[i].order.requiredCookingStructure));
            spawnedCards[i].Refresh();
        }

        if (tickets.Count > spawnedCards.Count)
        {
            for (int i = spawnedCards.Count; i < tickets.Count; i++)
            {
                PendingOrderCardUI newCard = CreateCard(tickets[i]);
                LayoutRebuilder.ForceRebuildLayoutImmediate(cardsContainer);
                newCard.PlayEnterAnimation();
            }
        }
        else if (tickets.Count < spawnedCards.Count)
        {
            for (int i = spawnedCards.Count - 1; i >= tickets.Count; i--)
                RemoveCardAnimated(i);
        }

        displayedTickets.Clear();
        for (int i = 0; i < tickets.Count; i++)
            displayedTickets.Add(tickets[i]);

        LayoutRebuilder.ForceRebuildLayoutImmediate(cardsContainer);
    }

    private PendingOrderCardUI CreateCard(PendingOrderTicket ticket)
    {
        PendingOrderCardUI newCard = Instantiate(cardPrefab, cardsContainer);
        newCard.Setup(ticket, GetStructureIcon(ticket.order.requiredCookingStructure));
        spawnedCards.Add(newCard);
        return newCard;
    }

    private void RemoveCardAnimated(int index)
    {
        if (index < 0 || index >= spawnedCards.Count)
            return;

        PendingOrderCardUI card = spawnedCards[index];
        spawnedCards.RemoveAt(index);

        if (card == null)
            return;

        card.PlayExitAnimation(() =>
        {
            if (card != null)
                Destroy(card.gameObject);

            LayoutRebuilder.ForceRebuildLayoutImmediate(cardsContainer);

            if (spawnedCards.Count == 0)
                SetVisible(false);
        });
    }

    private void RemoveAllAnimated()
    {
        if (spawnedCards.Count == 0)
        {
            SetVisible(false);
            displayedTickets.Clear();
            return;
        }

        for (int i = spawnedCards.Count - 1; i >= 0; i--)
            RemoveCardAnimated(i);

        displayedTickets.Clear();
    }

    private void SetVisible(bool visible)
    {
        if (headerRoot != null)
            headerRoot.SetActive(visible);

        if (scrollAreaRoot != null)
            scrollAreaRoot.SetActive(visible);
        else if (cardsContainer != null)
            cardsContainer.gameObject.SetActive(visible);
    }

    private Sprite GetStructureIcon(TaskGeneratorType generatorType)
    {
        switch (generatorType)
        {
            case TaskGeneratorType.Cashier: return cashierIcon;
            case TaskGeneratorType.Table: return tableIcon;
            case TaskGeneratorType.Oven: return ovenIcon;
            case TaskGeneratorType.Grill: return grillIcon;
            case TaskGeneratorType.Stove: return stoveIcon;
            case TaskGeneratorType.Sink: return sinkIcon;
            case TaskGeneratorType.Counter: return counterIcon;
            case TaskGeneratorType.GenericOperational: return genericOperationalIcon;
            default: return null;
        }
    }
}