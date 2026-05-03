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

    private void OnEnable()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnReputationChanged += UpdateReputationUI;
            UpdateReputationUI(ResourceManager.Instance.CurrentReputation);
        }
    }

    private void OnDisable()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnReputationChanged -= UpdateReputationUI;
    }

    private void UpdateReputationUI(int totalReputation)
    {
        if (totalReputation < 0)
            totalReputation = 0;

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

            if (totalReputation <= 0 || reachedMax)
                progressSlider.value = 0f;
            else
                progressSlider.value = remainder;
        }

        UpdateFillVisibility(totalReputation, filledStars);
    }

    private void UpdateFillVisibility(int totalReputation, int filledStars)
    {
        if (fillImage == null)
            return;

        bool reachedMax = filledStars >= maxStars;
        bool shouldHide = totalReputation <= 0 || reachedMax;

        Color color = fillImage.color;
        color.a = shouldHide ? 0f : 1f;
        fillImage.color = color;
    }
}