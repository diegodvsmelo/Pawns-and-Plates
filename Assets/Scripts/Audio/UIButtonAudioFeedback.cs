using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonAudioFeedback : MonoBehaviour, IPointerEnterHandler
{
    [Header("Audio Type")]
    [SerializeField] private ButtonAudioType buttonAudioType = ButtonAudioType.General;

    [Header("Options")]
    [SerializeField] private bool playHoverSound = true;
    [SerializeField] private bool playClickSound = true;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(HandleClickSound);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClickSound);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!playHoverSound)
            return;

        if (AudioManager.Instance == null)
            return;

        if (button != null && !button.interactable)
            return;

        AudioManager.Instance.PlayUiHover(buttonAudioType);
    }

    private void HandleClickSound()
    {
        if (!playClickSound)
            return;

        if (AudioManager.Instance == null)
            return;

        if (button != null && !button.interactable)
            return;

        AudioManager.Instance.PlayUiClick(buttonAudioType);
    }
}