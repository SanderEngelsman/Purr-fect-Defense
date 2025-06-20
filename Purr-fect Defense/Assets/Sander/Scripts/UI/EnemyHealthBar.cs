using UnityEngine;
using System.Linq;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] private float worldOffset = 1.0f;
    [SerializeField] private float screenOffset = 5f;

    [Header("Colors")]
    [SerializeField] private Color healthColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color damageColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color outlineColor = new Color(0, 0, 0);

    [Header("Size")]
    [SerializeField] private float healthbarWidth = 100f;
    [SerializeField] private float healthbarHeight = 10f;

    [Header("Canvas Overrides")]
    [SerializeField, Tooltip("Canvas GameObjects to ignore (e.g., HUD) when hiding health bars")]
    private GameObject[] excludedCanvasObjects = null;
    [SerializeField, Tooltip("Name of HUD canvas to ignore if not in excludedCanvasObjects (e.g., GameCanvas)")]
    private string hudCanvasName = "GameCanvas";

    private Enemy enemy;
    private ShieldTower shieldTower;
    private bool showHealthBar = false;
    private Camera mainCamera;
    private float health;
    private float maxHealth;

    private void Awake()
    {
        Debug.Log($"EnemyHealthBar Awake on {gameObject.name}. Script is running.", this);
    }

    private void Start()
    {
        mainCamera = Camera.main;
        enemy = GetComponent<Enemy>();
        shieldTower = GetComponent<ShieldTower>();

        if (enemy == null && shieldTower == null)
        {
            Debug.LogWarning($"Neither Enemy nor ShieldTower component found on {gameObject.name}. Health bar disabled.", this);
            enabled = false;
            return;
        }

        // Initialize health
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

        Debug.Log($"HealthBar initialized for {gameObject.name}: health={health}, maxHealth={maxHealth}, showHealthBar={showHealthBar}", this);
    }

    private void Update()
    {
        // Update health
        if (enemy != null)
        {
            health = enemy.health;
        }
        else if (shieldTower != null)
        {
            health = shieldTower.health;
        }

        if (showHealthBar && (health <= 0 || (enemy == null && shieldTower == null)))
        {
            showHealthBar = false;
            Debug.Log($"HealthBar hidden for {gameObject.name}: health={health} or components missing", this);
        }
    }

    public void ShowHealthBar()
    {
        showHealthBar = true;
        Debug.Log($"ShowHealthBar called for {gameObject.name}, showHealthBar={showHealthBar}", this);
    }

    private void OnGUI()
    {
        // Check canvases
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        bool anyActiveCanvas = false;
        string activeCanvasNames = "";

        foreach (var canvas in canvases)
        {
            bool isExcluded = (excludedCanvasObjects != null && excludedCanvasObjects.Any(obj => obj != null && obj.GetComponent<Canvas>() == canvas)) ||
                              (!string.IsNullOrEmpty(hudCanvasName) && canvas.gameObject.name == hudCanvasName);
            if (canvas.isActiveAndEnabled && !isExcluded)
            {
                anyActiveCanvas = true;
                activeCanvasNames += canvas.name + ", ";
            }
        }

        if (anyActiveCanvas)
        {
            Debug.Log($"HealthBar skipped for {gameObject.name}: Active canvases={activeCanvasNames}", this);
            return;
        }

        if (!showHealthBar || (enemy == null && shieldTower == null))
        {
            Debug.Log($"HealthBar not drawn for {gameObject.name}: showHealthBar={showHealthBar}, enemy={enemy != null}, shieldTower={shieldTower != null}", this);
            return;
        }

        if (health <= 0 || maxHealth <= 0)
        {
            Debug.Log($"HealthBar not drawn for {gameObject.name}: health={health}, maxHealth={maxHealth}", this);
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogWarning($"HealthBar not drawn for {gameObject.name}: mainCamera is null", this);
            return;
        }

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
        float healthBarX = screenPosition.x - healthbarWidth / 2;
        float healthBarY = Screen.height - screenPosition.y - healthbarHeight - screenOffset;

        // Draw outline
        GUI.DrawTexture(new Rect(healthBarX - 2, healthBarY - 2, healthbarWidth + 4, healthbarHeight + 4), outlineTexture);
        // Draw damage background
        GUI.DrawTexture(new Rect(healthBarX, healthBarY, healthbarWidth, healthbarHeight), damageTexture);
        // Draw health foreground
        float currentWidth = (health / maxHealth) * healthbarWidth;
        GUI.DrawTexture(new Rect(healthBarX, healthBarY, currentWidth, healthbarHeight), healthTexture);

        Debug.Log($"HealthBar drawn for {gameObject.name}: health={health}/{maxHealth}, position=({healthBarX}, {healthBarY})", this);

        Destroy(damageTexture);
        Destroy(healthTexture);
        Destroy(outlineTexture);
    }

    private void OnValidate()
    {
        if (excludedCanvasObjects != null)
        {
            for (int i = 0; i < excludedCanvasObjects.Length; i++)
            {
                if (excludedCanvasObjects[i] == null)
                {
                    Debug.LogWarning($"ExcludedCanvasObject at index {i} is null in EnemyHealthBar.", this);
                }
                else if (!excludedCanvasObjects[i].GetComponent<Canvas>())
                {
                    Debug.LogWarning($"ExcludedCanvasObject at index {i} ({excludedCanvasObjects[i].name}) has no Canvas component.", this);
                }
            }
        }
    }
}