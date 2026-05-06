using TMPro;
using UnityEngine;

public class TaskHintLineUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hintText;

    public void Setup(TaskHintLine data, Color highlightColor)
    {
        string colorHex = ColorUtility.ToHtmlStringRGB(highlightColor);

        string finalText =
            $"• {data.prefixText}<color=#{colorHex}><b>{data.highlightedText}</b></color>{data.suffixText}";

        hintText.text = finalText;
    }
}