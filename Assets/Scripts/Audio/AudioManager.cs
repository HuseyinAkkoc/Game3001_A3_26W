using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Button SFX")]
    [SerializeField] private AudioClip buttonClickSFX;

    [Header("Scene Music")]
    [SerializeField] private AudioClip startSceneMusic;
    [SerializeField] private AudioClip gameSceneMusic;
    [SerializeField] private AudioClip winSceneMusic;
    [SerializeField] private AudioClip loseSceneMusic;

    [Header("Scene Names")]
    [SerializeField] private string startSceneName = "StartScene";
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string winSceneName = "VictoryScene";
    [SerializeField] private string loseSceneName = "DefeatScene";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        if (musicSource == null)
            return;

        AudioClip targetClip = null;

        if (sceneName == startSceneName)
            targetClip = startSceneMusic;
        else if (sceneName == gameSceneName)
            targetClip = gameSceneMusic;
        else if (sceneName == winSceneName)
            targetClip = winSceneMusic;
        else if (sceneName == loseSceneName)
            targetClip = loseSceneMusic;

        if (targetClip == null)
            return;

        if (musicSource.clip == targetClip && musicSource.isPlaying)
            return;

        musicSource.Stop();
        musicSource.clip = targetClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClickSFX != null)
        {
            sfxSource.PlayOneShot(buttonClickSFX);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}