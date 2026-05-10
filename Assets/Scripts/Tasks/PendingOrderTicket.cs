using UnityEngine;

[System.Serializable]
public class PendingOrderTicket
{
    public RestaurantOrder order;
    public float remainingTime;

    public PendingOrderTicket(RestaurantOrder order)
    {
        this.order = order;
        remainingTime = order != null ? order.maxPendingWaitTime : 0f;
    }

    public float GetNormalizedTime()
    {
        if (order == null || order.maxPendingWaitTime <= 0f)
            return 0f;

        return Mathf.Clamp01(remainingTime / order.maxPendingWaitTime);
    }

    public bool Tick(float deltaTime)
    {
        remainingTime -= deltaTime;
        return remainingTime <= 0f;
    }
}