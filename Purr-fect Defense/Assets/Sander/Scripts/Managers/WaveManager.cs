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
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float timeBetweenWaves = 30f;
    [SerializeField] private Wave[] waves;
    [SerializeField] private Button startWaveButton;
    [SerializeField] private TextMeshProUGUI waveLabel;
    private int waveNumber = 0; // Display wave number, starts at 0
    public int currentWave = 0; // Index into waves array
    private bool isPreGame = true;

    public bool IsPreGame => isPreGame;

    private void OnValidate()
    {
        if (startWaveButton == null)
        {
            Debug.LogWarning("StartWaveButton is not assigned in WaveManager. Button will not be destroyed after starting the first wave.", this);
        }
        if (waveLabel == null)
        {
            Debug.LogWarning("WaveLabel is not assigned in WaveManager. Wave counter will not display.", this);
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
    }

    public void StartFirstWave()
    {
        if (isPreGame)
        {
            isPreGame = false;
            waveNumber = 1; // Set to 1 when starting first wave
            UpdateWaveLabel();
            StartCoroutine(StartWave());
            // Destroy the start wave button
            if (startWaveButton != null)
            {
                Debug.Log("Destroying Start Wave button.", startWaveButton.gameObject);
                Destroy(startWaveButton.gameObject);
            }
            else
            {
                Debug.LogWarning("StartWaveButton is null. Cannot destroy button.", this);
            }
        }
    }

    private IEnumerator StartWave()
    {
        while (currentWave < waves.Length)
        {
            if (currentWave > 0) // Skip delay for first wave after pre-game
            {
                yield return new WaitForSeconds(timeBetweenWaves);
                waveNumber++; // Increment for next wave
                UpdateWaveLabel();
            }
            Wave wave = waves[currentWave];
            for (int i = 0; i < wave.enemies.Length; i++)
            {
                if (wave.enemies[i] != null)
                {
                    Instantiate(wave.enemies[i], spawnPoint.position, Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning($"Wave {currentWave + 1}, Enemy {i + 1} is null!");
                }
                yield return new WaitForSeconds(1f);
            }
            currentWave++;
        }
        Debug.Log("All waves completed!");
    }

    private void UpdateWaveLabel()
    {
        if (waveLabel != null)
        {
            waveLabel.text = $"Wave {waveNumber}/{waves.Length}"; // Display Wave X/Y
            Debug.Log($"Wave UI updated: {waveLabel.text}", this);
        }
        else
        {
            Debug.LogWarning("WaveLabel is null in WaveManager.", this);
        }
    }
}
