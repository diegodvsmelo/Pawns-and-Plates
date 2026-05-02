using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Default Volume")]
    [Range(0f, 1f)][SerializeField] private float defaultMusicVolume = 0.7f;
    [Range(0f, 1f)][SerializeField] private float defaultSfxVolume = 0.8f;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SfxVolume";

    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);

        LoadVolumes();
        ApplyVolumes();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void Start()
    {
        TryOrganizeNearManagers();
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // Garante que nunca carregue preso a algum pai de cena por acidente
        transform.SetParent(null, true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryOrganizeNearManagers();
    }

    private void TryOrganizeNearManagers()
    {
        GameObject managers = GameObject.Find("Managers");
        if (managers == null)
            return;

        // Nao vira filho. Apenas tenta ficar semanticamente “proximo”
        // na hierarquia do objeto persistente, usando nome e ordem.
        transform.SetSiblingIndex(0);
    }

    private void LoadVolumes()
    {
        MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, defaultMusicVolume);
        SfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSfxVolume);
    }

    private void ApplyVolumes()
    {
        if (musicSource != null)
            musicSource.volume = MusicVolume;

        if (sfxSource != null)
            sfxSource.volume = SfxVolume;
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);

        if (musicSource != null)
            musicSource.volume = MusicVolume;

        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, MusicVolume);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);

        if (sfxSource != null)
            sfxSource.volume = SfxVolume;

        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, SfxVolume);
        PlayerPrefs.Save();
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null)
            return;

        if (musicSource.clip == clip)
        {
            if (!musicSource.isPlaying)
                musicSource.Play();

            return;
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip, SfxVolume);
    }
}