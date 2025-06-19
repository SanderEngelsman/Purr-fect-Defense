using UnityEngine;

public class ModTools : MonoBehaviour
{
    [SerializeField, Tooltip("GameManager for currency and health modifications")]
    private GameManager gameManager;
    [SerializeField, Tooltip("WaveManager for enemy spawn point")]
    private WaveManager waveManager;
    [SerializeField, Tooltip("Amount of currency to add with Right Shift + C")]
    private float currencyAmount = 100f;
    [SerializeField, Tooltip("Amount of health to add with Right Shift + H")]
    private float healthAmount = 50f;
    [SerializeField, Tooltip("Fast Enemy prefab for Right Shift + 1")]
    private GameObject fastEnemyPrefab;
    [SerializeField, Tooltip("Tank Enemy prefab for Right Shift + 2")]
    private GameObject tankEnemyPrefab;
    [SerializeField, Tooltip("Flying Enemy prefab for Right Shift + 3")]
    private GameObject flyingEnemyPrefab;
    [SerializeField, Tooltip("Stun Enemy prefab for Right Shift + 4")]
    private GameObject stunEnemyPrefab;

    private void OnValidate()
    {
        if (gameManager == null)
            Debug.LogWarning("GameManager not assigned in ModTools.", this);
        if (waveManager == null)
            Debug.LogWarning("WaveManager not assigned in ModTools.", this);
        if (fastEnemyPrefab == null)
            Debug.LogWarning("FastEnemyPrefab not assigned in ModTools.", this);
        if (tankEnemyPrefab == null)
            Debug.LogWarning("TankEnemyPrefab not assigned in ModTools.", this);
        if (flyingEnemyPrefab == null)
            Debug.LogWarning("FlyingEnemyPrefab not assigned in ModTools.", this);
        if (stunEnemyPrefab == null)
            Debug.LogWarning("StunEnemyPrefab not assigned in ModTools.", this);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (gameManager != null)
                {
                    gameManager.AddCurrency(currencyAmount);
                    Debug.Log($"ModTools: Added {currencyAmount} currency.", this);
                }
                else
                {
                    Debug.LogWarning("GameManager is null, cannot add currency.", this);
                }
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                if (gameManager != null)
                {
                    gameManager.AddBaseHealth(healthAmount);
                    Debug.Log($"ModTools: Added {healthAmount} health.", this);
                }
                else
                {
                    Debug.LogWarning("GameManager is null, cannot add health.", this);
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SpawnEnemy(fastEnemyPrefab, "FastEnemy");
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SpawnEnemy(tankEnemyPrefab, "TankEnemy");
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SpawnEnemy(flyingEnemyPrefab, "FlyingEnemy");
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SpawnEnemy(stunEnemyPrefab, "StunEnemy");
            }
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab, string enemyName)
    {
        if (waveManager == null || waveManager.spawnPoint == null)
        {
            Debug.LogWarning("WaveManager or spawnPoint is null, cannot spawn enemy.", this);
            return;
        }
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"{enemyName} prefab is null, cannot spawn.", this);
            return;
        }
        GameObject enemy = Instantiate(enemyPrefab, waveManager.spawnPoint.position, Quaternion.identity);
        Debug.Log($"ModTools: Spawned {enemyName} at {waveManager.spawnPoint.position}.", this);
    }
}