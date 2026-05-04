using UnityEngine;

[System.Serializable]
public class RestaurantOrder
{
    public string orderName;
    public TaskGeneratorType requiredCookingStructure;

    public int quantity = 1;
    public int baseReward = 10;
    public int reputationReward = 1;

    public TaskGeneratorStructure originStructure;

    public bool serviceWasSuccessful;
    public bool cookingWasSuccessful;
}