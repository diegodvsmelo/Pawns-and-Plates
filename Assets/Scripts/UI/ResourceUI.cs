using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    public void TestAddMoney()
    {
        ResourceManager.Instance.ModifyMoney(10);
    }
    public void TestAddReputation()
    {
        ResourceManager.Instance.ModifyReputation(10);
    }
    public void TestTakeMoney()
    {
        ResourceManager.Instance.ModifyMoney(-10);
    }
    public void TestTakeReputation()
    {
        ResourceManager.Instance.ModifyReputation(-10);
    }
}

    