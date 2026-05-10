using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PendingOrderCardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform visualRoot;
    [SerializeField] private Image orderIconImage;
    [SerializeField] private Image structureIconImage;
    [SerializeField] private Image progressFillImage;

    [Header("Layout")]
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private float preferredWidth = 120f;
    [SerializeField] private float preferredHeight = 84f;

    [Header("Animation")]
    [SerializeField] private float enterDuration = 0.18f;
    [SerializeField] private float exitDuration = 0.16f;
    [SerializeField] private float enterOffsetX = 60f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Coroutine animationRoutine;

    public PendingOrderTicket BoundTicket { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (layoutElement == null)
            layoutElement = GetComponent<LayoutElement>();

        if (layoutElement != null)
        {
            layoutElement.preferredWidth = preferredWidth;
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.minWidth = preferredWidth;
            layoutElement.minHeight = preferredHeight;
        }

        if (visualRoot == null)
            visualRoot = rectTransform;
    }

    public void Setup(PendingOrderTicket ticket, Sprite structureIcon)
    {
        BoundTicket = ticket;

        if (orderIconImage != null)
        {
            orderIconImage.sprite = ticket != null && ticket.order != null ? ticket.order.orderIcon : null;
            orderIconImage.gameObject.SetActive(orderIconImage.sprite != null);
        }

        if (structureIconImage != null)
        {
            structureIconImage.sprite = structureIcon;
            structureIconImage.gameObject.SetActive(structureIcon != null);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (progressFillImage != null && BoundTicket != null)
            progressFillImage.fillAmount = BoundTicket.GetNormalizedTime();
    }

    public void PlayEnterAnimation()
    {
        if (!gameObject.activeInHierarchy)
        {
            SetInstantVisible();
            return;
        }

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(EnterRoutine());
    }

    public void PlayExitAnimation(Action onFinished)
    {
        if (!gameObject.activeInHierarchy)
        {
            onFinished?.Invoke();
            return;
        }

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(ExitRoutine(onFinished));
    }

    private IEnumerator EnterRoutine()
    {
        canvasGroup.alpha = 0f;

        if (layoutElement != null)
        {
            layoutElement.preferredWidth = preferredWidth;
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.minWidth = preferredWidth;
            layoutElement.minHeight = preferredHeight;
        }

        visualRoot.localScale = Vector3.zero;
        visualRoot.anchoredPosition = new Vector2(enterOffsetX, 0f);

        float elapsed = 0f;

        while (elapsed < enterDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / enterDuration);
            float eased = EaseOutBack(t);

            canvasGroup.alpha = t;
            visualRoot.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, eased);
            visualRoot.anchoredPosition = Vector2.LerpUnclamped(
                new Vector2(enterOffsetX, 0f),
                Vector2.zero,
                eased
            );

            yield return null;
        }

        SetInstantVisible();
        animationRoutine = null;
    }

    private IEnumerator ExitRoutine(Action onFinished)
    {
        float startWidth = layoutElement != null ? layoutElement.preferredWidth : preferredWidth;
        float elapsed = 0f;

        while (elapsed < exitDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / exitDuration);
            float eased = EaseInCubic(t);

            canvasGroup.alpha = 1f - t;
            visualRoot.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, eased);
            visualRoot.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(-20f, 0f), eased);

            if (layoutElement != null)
            {
                float width = Mathf.Lerp(startWidth, 0f, eased);
                layoutElement.preferredWidth = width;
                layoutElement.minWidth = 0f;
            }

            yield return null;
        }

        if (layoutElement != null)
        {
            layoutElement.preferredWidth = 0f;
            layoutElement.minWidth = 0f;
        }

        canvasGroup.alpha = 0f;
        visualRoot.localScale = Vector3.zero;
        animationRoutine = null;

        onFinished?.Invoke();
    }

    private void SetInstantVisible()
    {
        canvasGroup.alpha = 1f;
        visualRoot.localScale = Vector3.one;
        visualRoot.anchoredPosition = Vector2.zero;

        if (layoutElement != null)
        {
            layoutElement.preferredWidth = preferredWidth;
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.minWidth = preferredWidth;
            layoutElement.minHeight = preferredHeight;
        }
    }

    private float EaseInCubic(float t)
    {
        return t * t * t;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}