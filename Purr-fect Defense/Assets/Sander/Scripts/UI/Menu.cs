using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class Menu : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    private void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogWarning("VideoPlayer not assigned in Menu.", this);
        }
        // Rely on button event for ResumeBackgroundMusic
        Debug.Log("Menu Start, skipping auto music resume to avoid duplication.", this);
    }

    private bool GameEndManagerExistsAndGameEnded()
    {
        GameEndManager gameEndManager = FindObjectOfType<GameEndManager>();
        if (gameEndManager != null)
        {
            return gameEndManager.gameObject.activeInHierarchy; // Approximation
        }
        return false;
    }

    public void Freeze()
    {
        Time.timeScale = 0f;
    }

    public void Unfreeze()
    {
        Time.timeScale = 1f;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        StartCoroutine(RestartWithSoundDelay());
    }

    private IEnumerator RestartWithSoundDelay()
    {
        yield return new WaitForSecondsRealtime(1f); // Allow win/lose sounds
        AudioManager.Instance.StopAllSounds();
        // Remove ResumeBackgroundMusic, rely on button event
        Debug.Log("Restarting scene with audio reset, skipping music resume.", this);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
        else
        {
            Debug.LogWarning("VideoPlayer is null, cannot play video.", this);
        }
    }

    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        else
        {
            Debug.LogWarning("VideoPlayer is null, cannot stop video.", this);
        }
    }
}