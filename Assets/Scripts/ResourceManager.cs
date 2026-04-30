using UnityEngine;
using TMPro;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI reputationText;

    [Header("Starting Values")]
    public int currentMoney = 100;
    public int currentReputation = 50;

    // OBSERVERS
    public event Action<int> OnMoneyChanged;
    public event Action<int> OnReputationChanged;
    public event Action OnMoneyInsufficient;
    public event Action OnReputationReachedZero;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        NotifyAll();
    }

    public void ModifyMoney(int amount)
    {
        currentMoney += amount;

        OnMoneyChanged?.Invoke(currentMoney);
    }

    public bool TrySpendMoney(int amount)
    {
        if (currentMoney < amount)
        {
            OnMoneyInsufficient?.Invoke();
            return false;
        }

        ModifyMoney(-amount);
        return true;
    }

    public void ModifyReputation(int amount)
    {
        currentReputation += amount;

        if (currentReputation < 0)
            currentReputation = 0;

        OnReputationChanged?.Invoke(currentReputation);

        if (currentReputation <= 0)
        {
            OnReputationReachedZero?.Invoke();
        }
    }

    private void NotifyAll()
    {
        OnMoneyChanged?.Invoke(currentMoney);
        OnReputationChanged?.Invoke(currentReputation);
    }
}