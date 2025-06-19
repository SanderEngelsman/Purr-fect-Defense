using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct Wave
{
    public GameObject[] enemies; // Enemies to spawn in order
}

public class WaveManager : MonoBehaviour
{
    [SerializeField] public Transform spawnPoint;
    [SerializeField] private Wave[] waves;
    [SerializeField] private Button startWaveButton;
    [SerializeField] private TextMeshProUGUI waveLabel;
    private int waveNumber = 0; // Display wave number, starts at 0
    private int currentWave = 0; // Index into waves array
    private bool isPreGame = true;
    private bool isWaveActive = false;
    private List<GameObject> activeEnemies = new List<GameObject>(); // Track spawned enemies

    public bool IsPreGame => isPreGame;
    public bool IsWaveActive => isWaveActive;
    public int WavesLength => waves.Length; // Expose waves length
    public int CurrentWave => currentWave; // Expose current wave
    public int ActiveEnemiesCount => activeEnemies.Count; // Expose enemy count

    private void OnValidate()
    {
        if (startWaveButton == null)
        {
            Debug.LogWarning("StartWaveButton is not assigned in WaveManager.", this);
        }
        if (waveLabel == null)
        {
            Debug.LogWarning("WaveLabel is not assigned in WaveManager.", this);
        }
        if (spawnPoint == null)
        {
            Debug.LogWarning("SpawnPoint is not assigned in WaveManager.", this);
        }
        if (waves == null || waves.Length == 0)
        {
            Debug.LogWarning("Waves array is empty or null in WaveManager.", this);
        }
    }

    private void Start()
    {
        UpdateWaveLabel();
        if (startWaveButton != null)
        {
            startWaveButton.gameObject.SetActive(true); // Ensure button is visible
        }
    }

    private void Update()
    {
        if (isWaveActive && activeEnemies.Count > 0)
        {
            // Remove destroyed enemies
            activeEnemies.RemoveAll(enemy => enemy == null);
            if (activeEnemies.Count == 0)
            {
                EndWave();
            }
        }
    }

    public void StartNextWave()
    {
        if (isPreGame || !isWaveActive)
        {
            if (currentWave < waves.Length)
            {
                isPreGame = false;
                isWaveActive = true;
                waveNumber = currentWave + 1; // Display wave number (1-based)
                UpdateWaveLabel();
                StartCoroutine(StartWave());
                if (startWaveButton != null)
                {
                    startWaveButton.gameObject.SetActive(false); // Hide button during wave
                }
            }
            else
            {
                Debug.Log("All waves completed!");
                if (startWaveButton != null)
                {
                    startWaveButton.gameObject.SetActive(false); // Hide button at game end
                }
            }
        }
    }

    private IEnumerator StartWave()
    {
        Wave wave = waves[currentWave];
        activeEnemies.Clear(); // Reset for new wave
        for (int i = 0; i < wave.enemies.Length; i++)
        {
            if (wave.enemies[i] != null)
            {
                GameObject enemy = Instantiate(wave.enemies[i], spawnPoint.position, Quaternion.identity);
                activeEnemies.Add(enemy); // Track enemy
            }
            else
            {
                Debug.LogWarning($"Wave {currentWave + 1}, Enemy {i + 1} is null!");
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void EndWave()
    {
        isWaveActive = false;
        currentWave++;
        if (currentWave < waves.Length)
        {
            if (startWaveButton != null)
            {
                startWaveButton.gameObject.SetActive(true); // Show button for next wave
            }
            UpdateWaveLabel(); // Show next wave number
        }
        else
        {
            Debug.Log("All waves completed!");
            if (startWaveButton != null)
            {
                startWaveButton.gameObject.SetActive(false); // Hide button at game end
            }
        }
    }

    private void UpdateWaveLabel()
    {
        if (waveLabel != null)
        {
            waveLabel.text = $"Wave {waveNumber}/{waves.Length}";
            Debug.Log($"Wave UI updated: {waveLabel.text}", this);
        }
        else
        {
            Debug.LogWarning("WaveLabel is null in WaveManager.", this);
        }
    }
}