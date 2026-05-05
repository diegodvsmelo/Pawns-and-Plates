using UnityEngine;
using UnityEngine.UI;

public class AttributeSquaresUI : MonoBehaviour
{
    [Header("Squares")]
    [SerializeField] private Image[] squares = new Image[10];

    [Header("Auto Setup")]
    [SerializeField] private bool autoFindChildImages = true;

    [Header("Sprites")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite filledSprite;

    [Header("Filled Color")]
    [SerializeField] private Color filledColor = Color.white;

    [Header("Rules")]
    [SerializeField] private int maxValue = 10;

    private void Awake()
    {
        AutoFindSquaresIfNeeded();
    }

    private void OnValidate()
    {
        if (maxValue < 1)
            maxValue = 1;

        if (autoFindChildImages)
            AutoFindSquaresIfNeeded();
    }

    public void UpdateValue(int value)
    {
        AutoFindSquaresIfNeeded();

        value = Mathf.Clamp(value, 0, maxValue);

        for (int i = 0; i < squares.Length; i++)
        {
            Image square = squares[i];

            if (square == null)
                continue;

            bool isFilled = i < value;

            if (isFilled)
            {
                square.sprite = filledSprite;
                square.color = filledColor;
            }
            else
            {
                square.sprite = emptySprite;
                square.color = Color.white;
            }
        }
    }

    private void AutoFindSquaresIfNeeded()
    {
        if (!autoFindChildImages)
            return;

        bool needsAutoFind = squares == null || squares.Length == 0;

        if (!needsAutoFind)
        {
            for (int i = 0; i < squares.Length; i++)
            {
                if (squares[i] == null)
                {
                    needsAutoFind = true;
                    break;
                }
            }
        }

        if (!needsAutoFind)
            return;

        Image[] childImages = GetComponentsInChildren<Image>(true);

        if (childImages == null || childImages.Length == 0)
            return;

        squares = childImages;
    }

    [ContextMenu("Test Value 5")]
    private void TestValue5()
    {
        UpdateValue(5);
    }

    [ContextMenu("Test Value 10")]
    private void TestValue10()
    {
        UpdateValue(10);
    }

    [ContextMenu("Clear Value")]
    private void ClearValue()
    {
        UpdateValue(0);
    }
}