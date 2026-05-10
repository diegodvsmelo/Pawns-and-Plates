using UnityEngine;

[CreateAssetMenu(fileName = "New Order Recipe", menuName = "Restaurant/Order Recipe")]
public class OrderRecipeData : ScriptableObject
{
    [Header("Order Info")]
    public string orderName;
    public Sprite orderIcon;

    [Header("Cooking Target")]
    public TaskGeneratorType requiredCookingStructure;

    [Header("Rewards")]
    public int baseReward = 10;
    public int reputationReward = 1;

    [Header("Difficulty")]
    public int cookingRequirement = 7;

    [Header("Pending Queue")]
    [Min(1f)] public float maxPendingWaitTime = 20f;
    [Min(0)] public int pendingFailureReputationPenalty = 2;

    public RestaurantOrder CreateRuntimeOrder(
        TaskGeneratorStructure origin,
        TaskGeneratorStructure cooking,
        bool serviceSuccess)
    {
        return new RestaurantOrder(this, origin, cooking, serviceSuccess);
    }
}