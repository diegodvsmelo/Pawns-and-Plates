using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    private ResourceManager resourceManager;
    private bool isSubscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
            return;

        if (ResourceManager.Instance == null)
        {
            Debug.LogWarning("MoneyUI: ResourceManager.Instance ainda não existe.");
            return;
        }

        resourceManager = ResourceManager.Instance;
        resourceManager.OnMoneyChanged += UpdateMoney;
        isSubscribed = true;

        UpdateMoney(resourceManager.CurrentMoney);
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || resourceManager == null)
            return;

        resourceManager.OnMoneyChanged -= UpdateMoney;
        isSubscribed = false;
        resourceManager = null;
    }

    private void UpdateMoney(int value)
    {
        if (moneyText == null)
        {
            Debug.LogWarning("MoneyUI: moneyText não foi atribuído no Inspector.");
            return;
        }

        moneyText.text = $"${value}";
    }
}