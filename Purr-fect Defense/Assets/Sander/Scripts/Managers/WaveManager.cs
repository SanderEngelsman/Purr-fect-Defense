using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Wave
{
    public GameObject[] enemies; // Enemies to spawn in order
}
public class WaveManager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float timeBetweenWaves = 30f;
    [SerializeField] private Wave[] waves; // Configurable in Inspector

    public int currentWave = 0;

    private void Start()
    {
        StartCoroutine(StartWave());
    }

    private IEnumerator StartWave()
    {
        while (currentWave < waves.Length)
        {
            yield return new WaitForSeconds(timeBetweenWaves);
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
