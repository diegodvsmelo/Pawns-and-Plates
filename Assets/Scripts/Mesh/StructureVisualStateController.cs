using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OrderVisualEntry
{
    public OrderRecipeData recipeData;
    public GameObject visualObject;
}

public class StructureVisualStateController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TaskGeneratorStructure structure;

    [Header("State Visuals (Optional)")]
    [SerializeField] private GameObject dirtyVisual;
    [SerializeField] private GameObject brokenVisual;

    [Header("Eating Order Visuals")]
    [SerializeField] private List<OrderVisualEntry> orderVisuals = new();

    private OrderRecipeData currentDisplayedOrder;

    private void Awake()
    {
        if (structure == null)
            structure = GetComponent<TaskGeneratorStructure>();

        HideAllImmediate();
        RefreshByStructureState();
    }

    public void ShowEatingOrder(OrderRecipeData recipeData)
    {
        currentDisplayedOrder = recipeData;
        RefreshByStructureState();
    }

    public void ClearEatingOrder()
    {
        currentDisplayedOrder = null;
        RefreshByStructureState();
    }

    public void RefreshByStructureState()
    {
        HideAllImmediate();

        if (structure == null)
            return;

        if (structure.CurrentState == StructureState.Broken || structure.CurrentState == StructureState.Disabled)
        {
            if (brokenVisual != null)
                brokenVisual.SetActive(true);

            return;
        }

        if (structure.CurrentState == StructureState.Dirty)
        {
            if (dirtyVisual != null)
                dirtyVisual.SetActive(true);

            return;
        }

        if (structure.CurrentState == StructureState.Eating && currentDisplayedOrder != null)
        {
            for (int i = 0; i < orderVisuals.Count; i++)
            {
                if (orderVisuals[i] == null)
                    continue;

                if (orderVisuals[i].recipeData != currentDisplayedOrder)
                    continue;

                if (orderVisuals[i].visualObject != null)
                    orderVisuals[i].visualObject.SetActive(true);

                return;
            }
        }
    }

    private void HideAllImmediate()
    {
        if (dirtyVisual != null)
            dirtyVisual.SetActive(false);

        if (brokenVisual != null)
            brokenVisual.SetActive(false);

        for (int i = 0; i < orderVisuals.Count; i++)
        {
            if (orderVisuals[i] == null)
                continue;

            if (orderVisuals[i].visualObject != null)
                orderVisuals[i].visualObject.SetActive(false);
        }
    }
}