using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainSceneName = "Main";

    [Header("Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        RegisterButtonListeners();
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
        Debug.Log("Options ainda nao implementado.");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}