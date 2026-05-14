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

    [Header("Generic State Visuals (Optional)")]
    [SerializeField] private GameObject brokenVisual;

    [Header("Eating Order Visuals")]
    [SerializeField] private List<OrderVisualEntry> eatingOrderVisuals = new();

    [Header("Dirty Order Visuals")]
    [SerializeField] private List<OrderVisualEntry> dirtyOrderVisuals = new();

    private OrderRecipeData currentEatingRecipe;
    private OrderRecipeData currentDirtyRecipe;

    private void Awake()
    {
        if (structure == null)
            structure = GetComponent<TaskGeneratorStructure>();

        HideAllImmediate();
        RefreshByStructureState();
    }

    public void ShowEatingOrder(OrderRecipeData recipeData)
    {
        currentEatingRecipe = recipeData;
        RefreshByStructureState();
    }

    public void ClearEatingOrder()
    {
        currentEatingRecipe = null;
        RefreshByStructureState();
    }

    public void ShowDirtyOrder(OrderRecipeData recipeData)
    {
        currentDirtyRecipe = recipeData;
        RefreshByStructureState();
    }

    public void ClearDirtyOrder()
    {
        currentDirtyRecipe = null;
        RefreshByStructureState();
    }

    public void ClearAllOrderVisuals()
    {
        currentEatingRecipe = null;
        currentDirtyRecipe = null;
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

        if (structure.CurrentState == StructureState.Eating && currentEatingRecipe != null)
        {
            ShowMappedVisual(eatingOrderVisuals, currentEatingRecipe);
            return;
        }

        if (structure.CurrentState == StructureState.Dirty && currentDirtyRecipe != null)
        {
            ShowMappedVisual(dirtyOrderVisuals, currentDirtyRecipe);
            return;
        }
    }

    private void ShowMappedVisual(List<OrderVisualEntry> entries, OrderRecipeData recipeData)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            OrderVisualEntry entry = entries[i];

            if (entry == null)
                continue;

            if (entry.recipeData != recipeData)
                continue;

            if (entry.visualObject != null)
                entry.visualObject.SetActive(true);

            return;
        }
    }

    private void HideAllImmediate()
    {
        if (brokenVisual != null)
            brokenVisual.SetActive(false);

        HideEntryList(eatingOrderVisuals);
        HideEntryList(dirtyOrderVisuals);
    }

    private void HideEntryList(List<OrderVisualEntry> entries)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            OrderVisualEntry entry = entries[i];

            if (entry == null)
                continue;

            if (entry.visualObject != null)
                entry.visualObject.SetActive(false);
        }
    }
}