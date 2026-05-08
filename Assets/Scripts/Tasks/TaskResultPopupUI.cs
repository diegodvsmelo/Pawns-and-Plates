using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskResultPopupUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject elements;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI chanceText;
    [SerializeField] private TextMeshProUGUI rollText;
    [SerializeField] private TextMeshProUGUI summaryText;

    [Header("Optional Groups")]
    [SerializeField] private GameObject chanceGroup;
    [SerializeField] private GameObject rollGroup;
    [SerializeField] private GameObject criticalBadge;

    [Header("Chance Bar")]
    [SerializeField] private Slider chanceSlider;
    [SerializeField] private Image chanceFillImage;

    [Header("Optional Roll Marker")]
    [SerializeField] private RectTransform chanceBarArea;
    [SerializeField] private RectTransform rollMarker;

    [Header("Button")]
    [SerializeField] private Button confirmButton;

    [Header("Colors")]
    [SerializeField] private Color successColor = new Color(0.25f, 0.85f, 0.35f);
    [SerializeField] private Color failureColor = new Color(0.9f, 0.3f, 0.3f);
    [SerializeField] private Color criticalColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color expiredColor = new Color(0.55f, 0.55f, 0.55f);

    private Action onConfirm;

    private void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(HandleConfirm);
            confirmButton.onClick.AddListener(HandleConfirm);
        }

        HideInstant();
    }

    public void ShowTaskResult(TaskInstance instance, bool wasSuccessful, Action confirmCallback)
    {
        if (instance == null || instance.data == null)
            return;

        onConfirm = confirmCallback;

        if (elements != null)
            elements.SetActive(true);

        if (headerText != null)
            headerText.text = BuildHeaderRichText(instance.data.taskName, wasSuccessful, instance.isCritical, false);

        if (chanceGroup != null)
            chanceGroup.SetActive(true);

        if (rollGroup != null)
            rollGroup.SetActive(true);

        if (criticalBadge != null)
            criticalBadge.SetActive(instance.isCritical);

        if (chanceText != null)
            chanceText.text = $"{instance.chancePercent:F1}%";

        if (rollText != null)
            rollText.text = instance.rolledValue >= 0f
                ? $"Roll: {instance.rolledValue:F1}"
                : "Roll: -";

        if (chanceSlider != null)
        {
            chanceSlider.minValue = 0f;
            chanceSlider.maxValue = 1f;
            chanceSlider.value = Mathf.Clamp01(instance.chancePercent / 100f);
        }

        Color resultColor = wasSuccessful
            ? (instance.isCritical ? criticalColor : successColor)
            : failureColor;

        if (chanceFillImage != null)
            chanceFillImage.color = resultColor;

        if (summaryText != null)
            summaryText.text = BuildSummary(instance, wasSuccessful);

        UpdateRollMarker(instance.rolledValue);
    }

    public void ShowExpiredResult(TaskData taskData, Action confirmCallback)
    {
        if (taskData == null)
            return;

        onConfirm = confirmCallback;

        if (elements != null)
            elements.SetActive(true);

        if (headerText != null)
            headerText.text = BuildHeaderRichText(taskData.taskName, false, false, true);

        if (chanceGroup != null)
            chanceGroup.SetActive(false);

        if (rollGroup != null)
            rollGroup.SetActive(false);

        if (criticalBadge != null)
            criticalBadge.SetActive(false);

        if (chanceSlider != null)
        {
            chanceSlider.minValue = 0f;
            chanceSlider.maxValue = 1f;
            chanceSlider.value = 0f;
        }

        if (chanceFillImage != null)
            chanceFillImage.color = expiredColor;

        if (summaryText != null)
            summaryText.text = BuildExpiredSummary(taskData);

        if (rollMarker != null)
            rollMarker.gameObject.SetActive(false);
    }

    private string BuildHeaderRichText(string taskName, bool wasSuccessful, bool isCritical, bool isExpired)
    {
        string resultWord;
        Color resultColor;

        if (isExpired)
        {
            resultWord = "EXPIRED";
            resultColor = expiredColor;
        }
        else if (wasSuccessful)
        {
            resultWord = isCritical ? "CRITICAL SUCCESS" : "SUCCESS";
            resultColor = isCritical ? criticalColor : successColor;
        }
        else
        {
            resultWord = "FAIL";
            resultColor = failureColor;
        }

        string hex = ColorUtility.ToHtmlStringRGB(resultColor);

        return $"{taskName} - <color=#{hex}><b>{resultWord}</b></color>";
    }

    private string BuildSummary(TaskInstance instance, bool wasSuccessful)
    {
        if (instance == null || instance.data == null)
            return "";

        if (wasSuccessful)
        {
            int money = instance.data.GetTotalMoneyReward(instance.isCritical);
            int reputation = instance.data.GetTotalReputationReward(instance.isCritical);
            int xp = instance.data.GetSuccessXP(instance.isCritical);

            return
                $"+${money}\n" +
                $"+{reputation} Reputation\n" +
                $"+{xp} XP for each assigned employee";
        }

        return
            $"-{instance.data.reputationPenalty} Reputation\n" +
            $"+{instance.data.xpOnFailure} XP for each assigned employee";
    }

    private string BuildExpiredSummary(TaskData taskData)
    {
        if (taskData == null)
            return "";

        return $"-{taskData.reputationPenalty} Reputation\nTask expired before collection.";
    }

    private void UpdateRollMarker(float rolledValue)
    {
        if (rollMarker == null || chanceBarArea == null)
            return;

        rollMarker.gameObject.SetActive(rolledValue >= 0f);

        if (rolledValue < 0f)
            return;

        float normalized = Mathf.Clamp01(rolledValue / 100f);
        float width = chanceBarArea.rect.width;
        float x = (normalized * width) - (width * 0.5f);

        Vector2 anchored = rollMarker.anchoredPosition;
        anchored.x = x;
        rollMarker.anchoredPosition = anchored;
    }

    private void HandleConfirm()
    {
        if (elements != null)
            elements.SetActive(false);

        Action callback = onConfirm;
        onConfirm = null;

        callback?.Invoke();
    }

    private void HideInstant()
    {
        if (elements != null)
            elements.SetActive(false);
    }
}