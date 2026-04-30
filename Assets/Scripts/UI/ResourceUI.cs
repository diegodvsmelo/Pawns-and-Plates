using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI reputationText;

    private ResourceManager resourceManager;

    private void OnEnable()
    {
        resourceManager = ResourceManager.Instance;

        if (resourceManager == null)
            return;

        resourceManager.OnMoneyChanged += UpdateMoneyUI;
        resourceManager.OnReputationChanged += UpdateReputationUI;

        UpdateMoneyUI(resourceManager.currentMoney);
        UpdateReputationUI(resourceManager.currentReputation);
    }

    private void OnDisable()
    {
        if (resourceManager == null)
            return;

        resourceManager.OnMoneyChanged -= UpdateMoneyUI;
        resourceManager.OnReputationChanged -= UpdateReputationUI;
    }

    private void UpdateMoneyUI(int money)
    {
        if (moneyText != null)
            moneyText.text = $"$: {money}";
    }

    private void UpdateReputationUI(int reputation)
    {
        if (reputationText != null)
            reputationText.text = $"*: {reputation}";
    }
}