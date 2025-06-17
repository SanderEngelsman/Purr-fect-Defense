using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] private float worldOffset = 1.0f;
    [SerializeField] private float screenOffset = 5f;

    [Header("Color Settings")]
    [SerializeField] private Color healthColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color damageColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color outlineColor = new Color(0, 0, 0);

    [Header("Size Settings")]
    [SerializeField] private float healthbarWidth = 100f;
    [SerializeField] private float healthbarHeight = 10f;

    [Header("Overlay Canvases")]
    [SerializeField, Tooltip("Canvases that should hide health bars when active (e.g., shop, pause menu)")]
    private List<GameObject> overlayCanvases = new List<GameObject>();

    private Enemy enemy;
    private ShieldTower shieldTower;
    private bool showHealthbar = false;
    private Camera mainCamera;
    private float health;
    private float maxHealth;

    private void Start()
    {
        mainCamera = Camera.main;
        enemy = GetComponent<Enemy>();
        shieldTower = GetComponent<ShieldTower>();

        if (enemy == null && shieldTower == null)
        {
            Debug.LogWarning($"Neither Enemy nor ShieldTower component found on {gameObject.name}. Health bar will not function.", this);
            enabled = false;
            return;
        }

        // Initialize health and maxHealth
        if (enemy != null)
        {
            health = enemy.health;
            maxHealth = enemy.maxHealth;
        }
        else if (shieldTower != null)
        {
            health = shieldTower.health;
            maxHealth = shieldTower.maxHealth;
        }
    }

    private void Update()
    {
        // Update health value
        if (enemy != null)
        {
            health = enemy.health;
        }
        else if (shieldTower != null)
        {
            health = shieldTower.health;
        }

        if (showHealthbar && (health <= 0 || (enemy == null && shieldTower == null)))
        {
            showHealthbar = false;
        }
    }

    public void ShowHealthBar()
    {
        showHealthbar = true;
    }

    private void OnGUI()
    {
        // Skip drawing if any overlay canvas is active
        foreach (var canvas in overlayCanvases)
        {
            if (canvas != null && canvas.activeInHierarchy)
            {
                return;
            }
        }

        if (!showHealthbar || (enemy == null && shieldTower == null)) return;

        Texture2D damageTexture = new Texture2D(1, 1);
        damageTexture.SetPixel(0, 0, damageColor);
        damageTexture.Apply();

        Texture2D healthTexture = new Texture2D(1, 1);
        healthTexture.SetPixel(0, 0, healthColor);
        healthTexture.Apply();

        Texture2D outlineTexture = new Texture2D(1, 1);
        outlineTexture.SetPixel(0, 0, outlineColor);
        outlineTexture.Apply();

        Vector3 worldPosition = transform.position + Vector3.up * worldOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        float healthbarX = screenPosition.x - healthbarWidth / 2;
        float healthbarY = Screen.height - screenPosition.y - healthbarHeight - screenOffset;

        // Draw outline
        GUI.DrawTexture(new Rect(healthbarX - 2, healthbarY - 2, healthbarWidth + 4, healthbarHeight + 4), outlineTexture);
        // Draw damage background
        GUI.DrawTexture(new Rect(healthbarX, healthbarY, healthbarWidth, healthbarHeight), damageTexture);
        // Draw health foreground
        float currentWidth = (health / maxHealth) * healthbarWidth;
        GUI.DrawTexture(new Rect(healthbarX, healthbarY, currentWidth, healthbarHeight), healthTexture);

        Destroy(damageTexture);
        Destroy(healthTexture);
        Destroy(outlineTexture);
    }

    private void OnValidate()
    {
        // Warn about null or duplicate canvases
        for (int i = 0; i < overlayCanvases.Count; i++)
        {
            if (overlayCanvases[i] == null)
            {
                Debug.LogWarning($"Overlay canvas at index {i} is null in EnemyHealthBar.", this);
            }
            else if (overlayCanvases.IndexOf(overlayCanvases[i]) != i)
            {
                Debug.LogWarning($"Duplicate overlay canvas {overlayCanvases[i].name} at index {i} in EnemyHealthBar.", this);
            }
        }
    }
}