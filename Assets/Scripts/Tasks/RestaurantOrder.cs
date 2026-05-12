using UnityEngine;

[System.Serializable]
public class RestaurantOrder
{
    public OrderRecipeData sourceData;

    public string orderName;
    public Sprite orderIcon;
    public TaskGeneratorType requiredCookingStructure;
    public TaskData cookingTaskData;

    public int baseReward = 10;
    public int reputationReward = 1;
    public int cookingRequirement = 7;

    public float maxPendingWaitTime = 20f;
    public int pendingFailureReputationPenalty = 2;

    public TaskGeneratorStructure originStructure;
    public TaskGeneratorStructure cookingStructure;

    public bool serviceWasSuccessful;
    public bool cookingWasSuccessful;

    public RestaurantOrder(
        OrderRecipeData data,
        TaskGeneratorStructure origin,
        TaskGeneratorStructure cooking,
        bool serviceSuccess = false)
    {
        sourceData = data;

        orderName = data != null ? data.orderName : "Unknown Order";
        orderIcon = data != null ? data.orderIcon : null;
        requiredCookingStructure = data != null ? data.requiredCookingStructure : TaskGeneratorType.Stove;
        cookingTaskData = data != null ? data.cookingTaskData : null;

        baseReward = data != null ? data.baseReward : 10;
        reputationReward = data != null ? data.reputationReward : 1;
        cookingRequirement = data != null ? data.cookingRequirement : 7;

        maxPendingWaitTime = data != null ? data.maxPendingWaitTime : 20f;
        pendingFailureReputationPenalty = data != null ? data.pendingFailureReputationPenalty : 2;

        originStructure = origin;
        cookingStructure = cooking;

        serviceWasSuccessful = serviceSuccess;
        cookingWasSuccessful = false;
    }
}