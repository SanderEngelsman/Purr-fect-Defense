using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip towerPlaceClip; // Played when tower is placed
    [SerializeField] private AudioClip buttonClickClip; // Played on button press
    [SerializeField] private AudioClip baseAttackClip; // Played when enemy attacks base
    [SerializeField] private AudioSource audioSource; // For 2D audio playback

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D audio
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
    }

    public void PlayTowerPlaceSound()
    {
        if (towerPlaceClip != null)
        {
            audioSource.PlayOneShot(towerPlaceClip);
            Debug.Log("Played tower place sound.", this);
        }
        else
        {
            Debug.LogWarning("TowerPlaceClip is null in AudioManager.", this);
        }
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickClip != null)
        {
            audioSource.PlayOneShot(buttonClickClip);
            Debug.Log("Played button click sound.", this);
        }
        else
        {
            Debug.LogWarning("ButtonClickClip is null in AudioManager.", this);
        }
    }

    public void PlayBaseAttackSound()
    {
        if (baseAttackClip != null)
        {
            audioSource.PlayOneShot(baseAttackClip);
            Debug.Log("Played base attack sound.", this);
        }
        else
        {
            Debug.LogWarning("BaseAttackClip is null in AudioManager.", this);
        }
    }
}