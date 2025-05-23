using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldTower : MonoBehaviour
{
    [SerializeField] private float maxHealth = 500f;
    private float health;
    private TilemapManager tilemapManager;

    private void Start()
    {
        health = maxHealth;
        tilemapManager = FindObjectOfType<TilemapManager>();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Vector3Int cellPos = tilemapManager.placementTilemap.WorldToCell(transform.position);
            tilemapManager.RemoveTower(cellPos);
            Destroy(gameObject);
        }
    }
}
