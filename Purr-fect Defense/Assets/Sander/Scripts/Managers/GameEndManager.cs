using UnityEngine;

public class GameEndManager : MonoBehaviour
{
    [SerializeField] private GameObject winScreenCanvas;
    [SerializeField] private GameObject loseScreenCanvas;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private GameManager gameManager;
    private bool gameEnded = false;

    private void Start()
    {
        if (winScreenCanvas == null)
        {
            Debug.LogWarning("WinScreenCanvas not assigned in GameEndManager.", this);
        }
        else
        {
            winScreenCanvas.SetActive(false);
        }

        if (loseScreenCanvas == null)
        {
            Debug.LogWarning("LoseScreenCanvas not assigned in GameEndManager.", this);
        }
        else
        {
            loseScreenCanvas.SetActive(false);
        }

        if (waveManager == null)
        {
            Debug.LogWarning("WaveManager not assigned in GameEndManager.", this);
        }

        if (gameManager == null)
        {
            Debug.LogWarning("GameManager not assigned in GameEndManager.", this);
        }
    }

    private void Update()
    {
        if (gameEnded) return;

        // Check win condition
        if (waveManager != null && waveManager.CurrentWave >= waveManager.WavesLength && waveManager.ActiveEnemiesCount == 0)
        {
            TriggerWinScreen();
        }
    }

    public void TriggerWinScreen()
    {
        if (winScreenCanvas != null)
        {
            winScreenCanvas.SetActive(true);
            Debug.Log("Win Screen activated.", this);
            gameEnded = true;
            Time.timeScale = 0; // Pause game
        }
    }

    public void TriggerLoseScreen()
    {
        if (loseScreenCanvas != null)
        {
            loseScreenCanvas.SetActive(true);
            Debug.Log("Lose Screen activated.", this);
            gameEnded = true;
        }
    }
}