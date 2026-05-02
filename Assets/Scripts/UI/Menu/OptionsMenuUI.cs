using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuUI : UIScreenBase
{
    [Header("Dependencies")]
    [SerializeField] private MainMenuController mainMenuController;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    private void Awake()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

        if (backButton != null)
            backButton.onClick.AddListener(BackToMainMenu);
    }

    private void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);

        if (backButton != null)
            backButton.onClick.RemoveListener(BackToMainMenu);
    }

    private void Start()
    {
        RefreshSliders();
        Hide();
    }

    public override void Show()
    {
        base.Show();
        RefreshSliders();
    }

    public void BackToMainMenu()
    {
        Hide();

        if (mainMenuController != null)
            mainMenuController.ShowElements();
    }

    private void RefreshSliders()
    {
        if (AudioManager.Instance == null)
            return;

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);
    }

    private void OnMusicSliderChanged(float value)
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnSfxSliderChanged(float value)
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.SetSfxVolume(value);
    }
}