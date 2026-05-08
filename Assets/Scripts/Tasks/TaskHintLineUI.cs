using TMPro;
using UnityEngine;

public class TaskHintLineUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hintText;

    public void Setup(TaskHintLine data, Color highlightColor)
    {
        if (hintText == null)
        {
            Debug.LogWarning("[TaskHintLineUI] hintText não está configurado no prefab.");
            return;
        }

        if (data == null)
        {
            hintText.text = "";
            return;
        }

        string colorHex = ColorUtility.ToHtmlStringRGB(highlightColor);

        hintText.text =
            $"• {data.prefixText}<color=#{colorHex}><b>{data.highlightedText}</b></color>{data.suffixText}";
    }
}