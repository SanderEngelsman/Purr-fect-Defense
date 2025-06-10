using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct ShopTowerData
{
    public GameObject towerPrefab;
    public float cost;
    public Button towerButton;
    public TextMeshProUGUI costLabel;
    public Sprite towerSprite;
    public string description;
    public float resellPrice;
}

public class Shop : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button shopButton;
    [SerializeField] public ShopTowerData[] Towers;
    [SerializeField, Tooltip("Font for tower description text")]
    private TMP_FontAsset descriptionFont;
    [SerializeField, Tooltip("Vertical spacing between image, description, and button")]
    public float entrySpacing = 5f;
    private Image towerIcon;
    private TextMeshProUGUI descriptionText;
    private GameManager gameManager;
    private TilemapManager tilemapManager;
    private ShopPanelAnimator shopPanelAnimator;
    private bool isOpen = false;
    private float lastToggleTime;
    private int currentTowerIndex = 0;
    private const float TOGGLE_DEBOUNCE = 0.2f;

    private void OnValidate()
    {
        Debug.Log($"Shop OnValidate triggered on {gameObject.name}.", this);
        if (this == null || !gameObject.activeInHierarchy) return;
        if (shopPanel == null)
            Debug.LogWarning("ShopPanel is not assigned in Inspector.", this);
        if (shopButton == null)
            Debug.LogWarning("ShopButton is not assigned in Shop.", this);
        if (Towers == null || Towers.Length == 0)
            Debug.LogWarning("Towers array is null or empty in Shop.", this);
        if (descriptionFont == null)
            Debug.LogWarning("DescriptionFont is not assigned in Shop.", this);
        if (entrySpacing < 0)
            Debug.LogWarning("EntrySpacing is negative. Should be 0 or positive.", this);
        for (int i = 0; i < (Towers?.Length ?? 0); i++)
        {
            var data = Towers[i];
            if (data.towerPrefab == null)
                Debug.LogWarning($"TowerPrefab is null for Towers[{i}].", this);
            if (data.towerButton == null)
                Debug.LogWarning($"TowerButton is null for Towers[{i}].", this);
            if (data.costLabel == null)
                Debug.LogWarning($"CostLabel is null for Towers[{i}].", this);
            if (data.towerSprite == null)
                Debug.LogWarning($"TowerSprite is null for Towers[{i}].", this);
            if (data.resellPrice < 0)
                Debug.LogWarning($"ResellPrice for {data.towerPrefab?.name} is negative.", this);
        }
    }

    private void Awake()
    {
        Debug.Log($"Shop Awake called on {gameObject.name}.", this);
    }

    private void Start()
    {
        Debug.Log($"Shop Start called on {gameObject.name}.", this);
        try
        {
            if (FindObjectsOfType<Shop>().Length > 1)
                Debug.LogWarning($"Multiple Shop components found in scene. This is on {gameObject.name}.", this);
            gameManager = FindObjectOfType<GameManager>();
            tilemapManager = FindObjectOfType<TilemapManager>();
            shopPanelAnimator = shopPanel != null ? shopPanel.GetComponent<ShopPanelAnimator>() : null;
            if (shopPanel == null)
                Debug.LogError("ShopPanel is null in Start.", this);
            if (shopButton == null)
                Debug.LogError("ShopButton is null in Start. Cannot add ToggleShop listener.", this);
            else
            {
                shopButton.onClick.RemoveAllListeners();
                shopButton.onClick.AddListener(ToggleShop);
                Debug.Log($"ToggleShop listener added to ShopButton ({shopButton.name}).", this);
            }
            SetupShopPanel();
            UpdateTowerDisplay();
            UpdateCosts();
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
                Debug.Log("ShopPanel set inactive.", this);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception in Shop Start: {e.Message}\nStackTrace: {e.StackTrace}", this);
        }
    }

    private void Update()
    {
        if (!isOpen || Towers == null || Towers.Length == 0) return;
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput > 0f)
        {
            CycleTower(1);
            Debug.Log($"Scrolled up. Current tower: {currentTowerIndex} ({Towers[currentTowerIndex].towerPrefab?.name ?? "Unknown"}).", this);
        }
        else if (scrollInput < 0f)
        {
            CycleTower(-1);
            Debug.Log($"Scrolled down. Current tower: {currentTowerIndex} ({Towers[currentTowerIndex].towerPrefab?.name ?? "Unknown"}).", this);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Shop))]
    public class ShopEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            Shop shop = (Shop)target;
            if (GUILayout.Button("Refresh Shop Panel"))
            {
                Undo.RecordObject(shop, "Refresh Shop Panel");
                shop.RefreshShopPanel();
                EditorUtility.SetDirty(shop);
            }
        }
    }
#endif

    [ContextMenu("Refresh Shop Panel")]
    private void RefreshShopPanel()
    {
        Debug.Log($"RefreshShopPanel called on {gameObject.name}.", this);
        SetupShopPanel();
        UpdateTowerDisplay();
    }

    private void SetupShopPanel()
    {
        Debug.Log($"SetupShopPanel started on {gameObject.name}.", this);
        if (shopPanel == null)
        {
            Debug.LogError("ShopPanel is null in SetupShopPanel.", this);
            return;
        }
        if (Towers == null || Towers.Length == 0)
        {
            Debug.LogError("Towers array is null or empty in SetupShopPanel.", this);
            return;
        }

        towerIcon = shopPanel.transform.Find("TowerIcon")?.GetComponent<Image>();
        if (towerIcon == null)
        {
            Debug.LogError("TowerIcon not found or missing Image component in ShopPanel.", this);
            return;
        }
        RectTransform iconRect = towerIcon.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(180, 180);
        iconRect.anchoredPosition = new Vector2(200, -120); // Top position

        descriptionText = shopPanel.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        if (descriptionText == null)
        {
            Debug.LogError("DescriptionText not found or missing TextMeshProUGUI in ShopPanel.", this);
            return;
        }
        if (descriptionFont != null)
        {
            descriptionText.font = descriptionFont;
            Debug.Log($"Applied font {descriptionFont.name} to DescriptionText.", descriptionText);
        }
        RectTransform descRect = descriptionText.GetComponent<RectTransform>();
        descRect.sizeDelta = new Vector2(160, 40);
        descRect.anchoredPosition = new Vector2(0, 100 - 80 / 2 - 40 / 2 - entrySpacing); // Below icon

        // Ensure all tower buttons are initially inactive
        foreach (var data in Towers)
        {
            if (data.towerButton != null)
            {
                data.towerButton.gameObject.SetActive(false);
                Debug.Log($"Deactivated tower button: {data.towerButton.name}.", data.towerButton);
            }
        }

        Debug.Log($"SetupShopPanel completed. Tower count: {Towers.Length}.", this);
    }

    private void CycleTower(int direction)
    {
        if (Towers == null || Towers.Length == 0) return;
        // Deactivate current button
        if (currentTowerIndex >= 0 && currentTowerIndex < Towers.Length && Towers[currentTowerIndex].towerButton != null)
        {
            Towers[currentTowerIndex].towerButton.gameObject.SetActive(false);
            Debug.Log($"Deactivated tower button: {Towers[currentTowerIndex].towerButton.name}.", this);
        }
        currentTowerIndex = (currentTowerIndex + direction + Towers.Length) % Towers.Length;
        UpdateTowerDisplay();
    }

    private void UpdateTowerDisplay()
    {
        if (Towers == null || currentTowerIndex < 0 || currentTowerIndex >= Towers.Length) return;
        var data = Towers[currentTowerIndex];
        if (towerIcon != null)
        {
            towerIcon.sprite = data.towerSprite;
            Debug.Log($"Updated TowerIcon to {data.towerSprite?.name ?? "None"}.", towerIcon);
        }
        if (descriptionText != null)
        {
            descriptionText.text = data.description;
            Debug.Log($"Updated DescriptionText to: {data.description}.", descriptionText);
        }
        if (data.costLabel != null)
        {
            data.costLabel.text = data.cost.ToString();
            bool canAfford = gameManager != null && gameManager.HasEnoughCurrency(data.cost);
            data.costLabel.color = canAfford ? Color.white : Color.red;
            Debug.Log($"Updated CostLabel to: {data.cost}, Color: {data.costLabel.color}.", data.costLabel);
        }
        if (data.towerButton != null)
        {
            data.towerButton.gameObject.SetActive(true);
            RectTransform buttonRect = data.towerButton.GetComponent<RectTransform>();
            if (buttonRect == null)
            {
                buttonRect = data.towerButton.gameObject.AddComponent<RectTransform>();
                Debug.Log($"Added RectTransform to tower button: {data.towerButton.name}.", data.towerButton);
            }
            buttonRect.sizeDelta = new Vector2(100, 30);
            buttonRect.anchoredPosition = new Vector2(0, 100 - 80 / 2 - 40 - entrySpacing * 2 - 30 / 2); // Below description
            data.towerButton.transform.SetParent(shopPanel.transform, false);
            data.towerButton.onClick.RemoveAllListeners();
            data.towerButton.onClick.AddListener(() => SelectTower(currentTowerIndex));
            Debug.Log($"Updated tower button: {data.towerButton.name}, Position: {buttonRect.anchoredPosition}.", data.towerButton);
        }
        else
        {
            Debug.LogWarning($"TowerButton is null for tower: {data.towerPrefab?.name ?? "Unknown"}.", this);
        }
    }

    public void ToggleShop()
    {
        if (Time.unscaledTime - lastToggleTime < TOGGLE_DEBOUNCE)
        {
            Debug.LogWarning("ToggleShop debounced.", this);
            return;
        }
        lastToggleTime = Time.unscaledTime;
        Debug.Log($"ToggleShop called. Current isOpen: {isOpen}.", this);
        isOpen = !isOpen;
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            if (isOpen)
            {
                shopPanelAnimator?.OpenPanel();
                tilemapManager?.CancelPlacement();
                UpdateTowerDisplay();
                UpdateCosts();
                Debug.Log("Shop opened.", this);
            }
            else
            {
                shopPanelAnimator?.ClosePanel();
                tilemapManager?.CancelPlacement();
                // Deactivate current button when closing
                if (currentTowerIndex >= 0 && currentTowerIndex < Towers.Length && Towers[currentTowerIndex].towerButton != null)
                {
                    Towers[currentTowerIndex].towerButton.gameObject.SetActive(false);
                }
                Debug.Log("Shop closed.", this);
            }
        }
        else
        {
            Debug.LogError("ShopPanel is null in ToggleShop.", this);
        }
    }

    public void SelectTower(int index)
    {
        if (index < 0 || index >= Towers.Length)
        {
            Debug.LogError($"Invalid tower index: {index}.", this);
            return;
        }
        if (!gameManager.HasEnoughCurrency(Towers[index].cost))
        {
            Debug.Log($"Not enough currency to select {Towers[index].towerPrefab?.name}. Required: {Towers[index].cost}", this);
            return;
        }
        if (tilemapManager != null && Towers[index].towerPrefab != null)
        {
            tilemapManager.StartPlacingTower(Towers[index].towerPrefab, Towers[index].cost);
            ToggleShop();
            Debug.Log($"Selected tower: {Towers[index].towerPrefab?.name}, Cost: {Towers[index].cost}", this);
        }
        else
        {
            Debug.LogError($"Cannot select tower. TilemapManager: {tilemapManager}, TowerPrefab: {Towers[index].towerPrefab}", this);
        }
    }

    public void UpdateCosts()
    {
        if (Towers == null) return;
        foreach (var data in Towers)
        {
            if (data.costLabel != null)
            {
                data.costLabel.text = data.cost.ToString();
                bool canAfford = gameManager != null && gameManager.HasEnoughCurrency(data.cost);
                data.costLabel.color = canAfford ? Color.white : Color.red;
                Debug.Log($"Updated cost label for {data.towerPrefab?.name}: {data.cost}, Color: {data.costLabel.color}", this);
            }
        }
    }
}