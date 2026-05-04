using UnityEngine;

[CreateAssetMenu(fileName = "New Order Recipe", menuName = "Restaurant/Order Recipe")]
public class OrderRecipeData : ScriptableObject
{
    [Header("Order Info")]
    public string orderName;

    [Header("Cooking Target")]
    public TaskGeneratorType requiredCookingStructure;

    [Header("Rewards")]
    public int baseReward = 10;
    public int reputationReward = 1;

    [Header("Difficulty")]
    public int cookingRequirement = 7;

    public RestaurantOrder CreateRuntimeOrder(TaskGeneratorStructure origin, bool serviceSuccess)
    {
        return new RestaurantOrder
        {
            orderName = orderName,
            requiredCookingStructure = requiredCookingStructure,
            baseReward = baseReward,
            reputationReward = reputationReward,
            originStructure = origin,
            serviceWasSuccessful = serviceSuccess
        };
    }
}