using UnityEngine;

public class ShieldTower : MonoBehaviour
{
    public float maxHealth = 500f;
    public float health;
    private TilemapManager tilemapManager;
    private EnemyHealthBar healthBar;
    private Animator animator; // Added for idle animation

    private void Start()
    {
        health = maxHealth;
        tilemapManager = FindObjectOfType<TilemapManager>();
        healthBar = GetComponent<EnemyHealthBar>();
        animator = GetComponent<Animator>(); // Cache Animator
        if (animator == null)
        {
            Debug.LogWarning($"Animator missing on {gameObject.name}. Animations will not work.", this);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (healthBar != null)
        {
            healthBar.ShowHealthBar();
        }
        if (health <= 0)
        {
            Vector3Int cellPos = tilemapManager.placementTilemap.WorldToCell(transform.position);
            tilemapManager.RemoveTower(cellPos);
            Destroy(gameObject);
        }
    }
}