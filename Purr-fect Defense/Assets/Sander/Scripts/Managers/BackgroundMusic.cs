using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic Instance { get; set; }
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate BackgroundMusic found on {gameObject.name}, destroying this instance.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            Debug.Log($"BackgroundMusic singleton initialized. GameObject: {gameObject.name}, Clip: {audioSource.clip?.name}", this);
        }
        else
        {
            Debug.LogWarning($"No AudioSource on BackgroundMusic GameObject: {gameObject.name}.", this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Debug.Log($"BackgroundMusic singleton destroyed. GameObject: {gameObject.name}", this);
            Instance = null;
        }
    }
}