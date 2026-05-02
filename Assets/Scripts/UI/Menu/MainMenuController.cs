using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainSceneName = "Main";

    [Header("UI Root")]
    [SerializeField] private GameObject elements;

    [Header("Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitButton;

    [Header("Screens")]
    [SerializeField] private OptionsMenuUI optionsMenuUI;

    private void Awake()
    {
        RegisterButtonListeners();
    }

    private void Start()
    {
        ShowElements();
    }

    private void OnDestroy()
    {
        UnregisterButtonListeners();
    }

    private void RegisterButtonListeners()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(NewGame);

        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(LoadGame);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    private void UnregisterButtonListeners()
    {
        if (newGameButton != null)
            newGameButton.onClick.RemoveListener(NewGame);

        if (loadGameButton != null)
            loadGameButton.onClick.RemoveListener(LoadGame);

        if (optionsButton != null)
            optionsButton.onClick.RemoveListener(OpenOptions);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitGame);
    }

    public void NewGame()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    public void LoadGame()
    {
        Debug.Log("Load Game ainda nao implementado.");
    }

    public void OpenOptions()
    {
        HideElements();

        if (optionsMenuUI != null)
            optionsMenuUI.Show();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowElements()
    {
        if (elements != null)
            elements.SetActive(true);
    }

    public void HideElements()
    {
        if (elements != null)
            elements.SetActive(false);
    }
}