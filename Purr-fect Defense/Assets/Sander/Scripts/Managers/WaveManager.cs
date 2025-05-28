using System.Collections;
using System.Collections.Generic;
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
    public int currentWave = 0;
    private bool isPreGame = true;

    public bool IsPreGame => isPreGame;

    private void OnValidate()
    {
        if (startWaveButton == null)
        {
            Debug.LogWarning("StartWaveButton is not assigned in WaveManager. Button will not be destroyed after starting the first wave.", this);
        }
    }

    public void StartFirstWave()
    {
        if (isPreGame)
        {
            isPreGame = false;
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
}
