using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField, Tooltip("Played when tower is placed")] private AudioClip towerPlaceClip;
    [SerializeField, Tooltip("Played to button click")] private AudioClip buttonClickClip;
    [SerializeField, Tooltip("Played when enemy attacks base")] private AudioClip baseAttackClip;
    [SerializeField, Tooltip("Played when win screen is shown")] private AudioClip winClip;
    [SerializeField, Tooltip("Played when lose screen is shown")] private AudioClip loseClip;
    [SerializeField, Tooltip("AudioSource for 2D audio playback")] private AudioSource audioSource;
    [SerializeField, Tooltip("AudioSource for background music (on separate GameObject)")] private AudioSource backgroundMusicAudioSource;
    [SerializeField, Tooltip("Name of BackgroundMusic GameObject (e.g., BackgroundMusic)")] private string backgroundMusicObjectName = "BackgroundMusic";
    [SerializeField] private AudioSource towerPlaceAudioSource;

    private bool isGameEnded = false;
    private float lastResumeTime = -1f;
    private const float resumeDebounceTime = 1f;

    private void Awake()
    {
        // Check for existing AudioManager instances
        AudioManager[] managers = FindObjectsOfType<AudioManager>();
        if (managers.Length > 1)
        {
            foreach (var manager in managers)
            {
                if (manager != this)
                {
                    Debug.LogWarning($"Duplicate AudioManager found on {manager.gameObject.name}. Destroying this instance: {gameObject.name}.", this);
                    Destroy(gameObject);
                    return;
                }
            }
        }

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"AudioManager singleton conflict. Existing: {Instance.gameObject.name}, New: {gameObject.name}. Destroying new instance.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.name = "AudioManager (Persistent)"; // Tag persistent instance

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D audio
            Debug.Log("AudioSource created on AudioManager.", this);
        }

        Debug.Log($"AudioManager singleton initialized. GameObject: {gameObject.name}", this);
        EnsureBackgroundMusicSource();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BackgroundMusic[] bgMusicObjects = FindObjectsOfType<BackgroundMusic>();
        Debug.Log($"Scene loaded: {scene.name}. Found {bgMusicObjects.Length} BackgroundMusic GameObjects.", this);
        foreach (var bgMusic in bgMusicObjects)
        {
            Debug.Log($"BackgroundMusic: {bgMusic.gameObject.name}, AudioSource: {bgMusic.GetComponent<AudioSource>()?.clip?.name}", this);
        }
        EnsureBackgroundMusicSource();
    }

    private void Update()
    {
        if (backgroundMusicAudioSource != null && backgroundMusicAudioSource.isPlaying && isGameEnded)
        {
            Debug.LogWarning($"Background music is playing during game end! Stopping it. Clip: {backgroundMusicAudioSource.clip?.name}, Enabled: {backgroundMusicAudioSource.enabled}", this);
            backgroundMusicAudioSource.Stop();
        }
    }

    private void EnsureBackgroundMusicSource()
    {
        if (backgroundMusicAudioSource == null)
        {
            GameObject bgMusicObj = GameObject.Find(backgroundMusicObjectName);
            if (bgMusicObj != null)
            {
                backgroundMusicAudioSource = bgMusicObj.GetComponent<AudioSource>();
                if (backgroundMusicAudioSource != null)
                {
                    backgroundMusicAudioSource.playOnAwake = false;
                    Debug.Log($"BackgroundMusicAudioSource assigned from {backgroundMusicObjectName}. Clip: {backgroundMusicAudioSource.clip?.name}, Enabled: {backgroundMusicAudioSource.enabled}", this);
                }
                else
                {
                    Debug.LogWarning($"No AudioSource found on {backgroundMusicObjectName}.", this);
                }
            }
            else
            {
                Debug.LogWarning($"BackgroundMusic GameObject '{backgroundMusicObjectName}' not found.", this);
            }
        }
    }

    private void OnValidate()
    {
        if (audioSource == null)
            Debug.LogWarning("AudioSource not assigned in AudioManager.", this);
        if (towerPlaceClip == null)
            Debug.LogWarning("TowerPlaceClip not assigned in AudioManager.", this);
        if (buttonClickClip == null)
            Debug.LogWarning("ButtonClickClip not assigned in AudioManager.", this);
        if (baseAttackClip == null)
            Debug.LogWarning("BaseAttackClip is null in AudioManager.", this);
        if (winClip == null)
            Debug.LogWarning("WinClip is null in AudioManager.", this);
        if (loseClip == null)
            Debug.LogWarning("LoseClip is null in AudioManager.", this);
        if (backgroundMusicAudioSource == null)
            Debug.LogWarning("BackgroundMusicAudioSource is null in AudioManager.", this);
    }

    public void PlayTowerPlaceSound()
    {
        if (towerPlaceClip != null && towerPlaceAudioSource != null)
        {
            towerPlaceAudioSource.PlayOneShot(towerPlaceClip);
            Debug.Log("Played tower place sound.", this);
        }
        else
        {
            Debug.LogWarning($"TowerPlaceClip or AudioSource is null in AudioManager. Clip: {towerPlaceClip}, Source: {audioSource}", this);
        }
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickClip);
            Debug.Log("Played button click sound.", this);
        }
        else
        {
            Debug.LogWarning($"ButtonClickClip or AudioSource is null in AudioManager. Clip: {buttonClickClip}, Source: {audioSource}", this);
        }
    }

    public void PlayBaseAttackSound()
    {
        if (baseAttackClip != null && towerPlaceAudioSource != null)
        {
            towerPlaceAudioSource.PlayOneShot(baseAttackClip);
            Debug.Log("Played base attack sound.", this);
        }
        else
        {
            Debug.LogWarning($"BaseAttackClip or AudioSource is null in AudioManager. Clip: {baseAttackClip}, Source: {audioSource}", this);
        }
    }

    public void PlayWinSound()
    {
        if (winClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(winClip);
            Debug.Log($"Played win sound. Clip: {winClip.name}, Duration: {winClip.length}", this);
        }
        else
        {
            Debug.LogWarning($"WinClip or AudioSource is null in AudioManager. Clip: {winClip}, Source: {audioSource}", this);
        }

        isGameEnded = true;
        EnsureBackgroundMusicSource();
        if (backgroundMusicAudioSource != null)
        {
            backgroundMusicAudioSource.Stop();
            Debug.Log($"Background music stopped for win screen. IsPlaying: {backgroundMusicAudioSource.isPlaying}, Enabled: {backgroundMusicAudioSource.enabled}, Clip: {backgroundMusicAudioSource.clip?.name}", this);
        }
        else
        {
            Debug.LogWarning("BackgroundMusicAudioSource is null, cannot stop music.", this);
        }
    }

    public void PlayLoseSound()
    {
        if (loseClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(loseClip);
            Debug.Log($"Played lose sound. Clip: {loseClip.name}, Duration: {loseClip.length}", this);
        }
        else
        {
            Debug.LogWarning($"LoseClip or AudioSource is null in AudioManager. Clip: {loseClip}, Source: {audioSource}", this);
        }

        isGameEnded = true;
        EnsureBackgroundMusicSource();
        if (backgroundMusicAudioSource != null)
        {
            if (backgroundMusicAudioSource.isPlaying)
            {
                backgroundMusicAudioSource.Stop();
                Debug.Log($"Background music stopped for lose screen. IsPlaying: {backgroundMusicAudioSource.isPlaying}, Enabled: {backgroundMusicAudioSource.enabled}, Clip: {backgroundMusicAudioSource.clip?.name}", this);
            }
            else
            {
                Debug.Log($"Background music already stopped for lose screen. IsPlaying: {backgroundMusicAudioSource.isPlaying}", this);
            }
        }
        else
        {
            Debug.LogWarning("BackgroundMusicAudioSource is null, cannot stop music.", this);
        }
    }

    public void ResumeBackgroundMusic()
    {
        Debug.Log($"ResumeBackgroundMusic called. Time since last: {Time.time - lastResumeTime}s, Scene: {SceneManager.GetActiveScene().name}", this);

        if (Time.time - lastResumeTime < resumeDebounceTime)
        {
            Debug.LogWarning($"ResumeBackgroundMusic blocked by debounce. Time since last: {Time.time - lastResumeTime}s", this);
            return;
        }

        lastResumeTime = Time.time;

        if (isGameEnded)
        {
            Debug.Log("Resuming background music after game end.", this);
            isGameEnded = false;
        }

        EnsureBackgroundMusicSource();
        if (backgroundMusicAudioSource != null)
        {
            Debug.Log($"BackgroundMusicAudioSource state: Enabled: {backgroundMusicAudioSource.enabled}, Clip: {backgroundMusicAudioSource.clip?.name}, IsPlaying: {backgroundMusicAudioSource.isPlaying}", this);
            if (!backgroundMusicAudioSource.isPlaying)
            {
                backgroundMusicAudioSource.Play();
                Debug.Log($"Background music resumed. IsPlaying: {backgroundMusicAudioSource.isPlaying}, Clip: {backgroundMusicAudioSource.clip?.name}, CallStack: {System.Environment.StackTrace}", this);
            }
            else
            {
                Debug.Log("Background music already playing, no need to resume.", this);
            }
        }
        else
        {
            Debug.LogWarning("BackgroundMusicAudioSource is null, cannot resume music.", this);
        }
    }

    public void StopAllSounds()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            Debug.Log("All sounds stopped on AudioManager audioSource.", this);
        }
        else
        {
            Debug.LogWarning("AudioSource is null, cannot stop sounds.", this);
        }
    }
}