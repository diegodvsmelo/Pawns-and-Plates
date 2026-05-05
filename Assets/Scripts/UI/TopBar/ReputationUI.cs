using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReputationUI : MonoBehaviour
{
    [Header("Stars")]
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite emptyStarSprite;
    [SerializeField] private Sprite filledStarSprite;

    [Header("Progress")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private int reputationPerStar = 100;
    [SerializeField] private int maxStars = 5;

    [Header("Animation")]
    [SerializeField] private bool animateChanges = true;
    [SerializeField] private float stepDelay = 0.02f;

    private int displayedReputation;
    private Coroutine animationRoutine;
    private ResourceManager resourceManager;

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
        if (resourceManager != null)
            resourceManager.OnReputationChanged -= HandleReputationChanged;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);
    }

    private void TrySubscribe()
    {
        if (resourceManager != null)
            return;

        if (ResourceManager.Instance == null)
            return;

        resourceManager = ResourceManager.Instance;
        resourceManager.OnReputationChanged += HandleReputationChanged;

        displayedReputation = resourceManager.CurrentReputation;
        UpdateReputationUI(displayedReputation);
    }

    private void HandleReputationChanged(int newReputation)
    {
        newReputation = Mathf.Clamp(newReputation, 0, GetMaxReputation());

        if (!animateChanges)
        {
            displayedReputation = newReputation;
            UpdateReputationUI(displayedReputation);
            return;
        }

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(AnimateReputationChange(newReputation));
    }

    private IEnumerator AnimateReputationChange(int targetReputation)
    {
        targetReputation = Mathf.Clamp(targetReputation, 0, GetMaxReputation());

        while (displayedReputation != targetReputation)
        {
            if (displayedReputation < targetReputation)
                displayedReputation++;
            else
                displayedReputation--;

            UpdateReputationUI(displayedReputation);

            yield return new WaitForSeconds(stepDelay);
        }

        animationRoutine = null;
    }

    private void UpdateReputationUI(int totalReputation)
    {
        totalReputation = Mathf.Clamp(totalReputation, 0, GetMaxReputation());

        int filledStars = totalReputation / reputationPerStar;
        int remainder = totalReputation % reputationPerStar;

        if (filledStars > maxStars)
            filledStars = maxStars;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null)
                continue;

            starImages[i].sprite = i < filledStars ? filledStarSprite : emptyStarSprite;
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = reputationPerStar;

            bool reachedMax = filledStars >= maxStars;
            bool exactStarValue = remainder == 0;

            if (totalReputation <= 0 || reachedMax || exactStarValue)
                progressSlider.value = 0f;
            else
                progressSlider.value = remainder;
        }

        UpdateFillVisibility(totalReputation, filledStars, remainder);
    }

    private void UpdateFillVisibility(int totalReputation, int filledStars, int remainder)
    {
        if (fillImage == null)
            return;

        bool reachedMax = filledStars >= maxStars;
        bool exactStarValue = remainder == 0;

        bool shouldHide =
            totalReputation <= 0 ||
            reachedMax ||
            exactStarValue;

        Color color = fillImage.color;
        color.a = shouldHide ? 0f : 1f;
        fillImage.color = color;
    }

    private int GetMaxReputation()
    {
        return reputationPerStar * maxStars;
    }
}