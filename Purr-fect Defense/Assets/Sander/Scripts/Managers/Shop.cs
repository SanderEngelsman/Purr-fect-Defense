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
    [Tooltip("Width multiplier for TowerIcon (e.g., 2 for double width, 1 for default)")]
    public float iconWidthMultiplier;
}

public class Shop : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button shopButton;
    [SerializeField] public ShopTowerData[] Towers;
    [SerializeField, Tooltip("Font for tower description text")]
    private TMP_FontAsset descriptionFont;
    [SerializeField, Tooltip("Base width for TowerIcon (height remains unchanged)")]
    private float baseIconWidth = 80f;
    private Image towerIcon;
    private TextMeshProUGUI descriptionText;
    private GameManager gameManager;
    private TilemapManager tilemapManager;
    private ShopPanelAnimator shopPanelAnimator;
    private Canvas shopCanvas;
    private bool isOpen = false;
    private float lastToggleTime;
    private int currentTowerIndex = 0;
    private bool needsCanvasUpdate = false;
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
        if (baseIconWidth <= 0)
            Debug.LogWarning("BaseIconWidth must be positive.", this);
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
            if (string.IsNullOrEmpty(data.description))
                Debug.LogWarning($"Description is empty for Towers[{i}].", this);
            if (data.resellPrice < 0)
                Debug.LogWarning($"ResellPrice for {data.towerPrefab?.name} is negative.", this);
            if (data.iconWidthMultiplier <= 0)
                Debug.LogWarning($"IconWidthMultiplier must be positive for Towers[{i}].", this);
        }
    }

    private void Awake()
    {
        Debug.Log($"Shop Awake called on {gameObject.name}.", this);
        shopCanvas = shopPanel != null ? shopPanel.GetComponentInParent<Canvas>() : null;
        if (shopCanvas == null)
            Debug.LogError("ShopPanel is not under a Canvas.", this);
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

    private void LateUpdate()
    {
        if (needsCanvasUpdate)
        {
            Canvas.ForceUpdateCanvases();
            needsCanvasUpdate = false;
            Debug.Log("Forced canvas update in LateUpdate.", this);
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
        if (shopCanvas == null)
        {
            shopCanvas = shopPanel.GetComponentInParent<Canvas>();
            if (shopCanvas == null)
            {
                Debug.LogError("ShopPanel is not under a Canvas in SetupShopPanel.", this);
                return;
            }
        }

        towerIcon = shopPanel.transform.Find("TowerIcon")?.GetComponent<Image>();
        if (towerIcon == null)
        {
            Debug.LogError("TowerIcon not found or missing Image component in ShopPanel.", this);
            return;
        }

        descriptionText = shopPanel.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        if (descriptionText == null)
        {
            Debug.LogWarning("DescriptionText not found via Find. Searching all children of ShopPanel.", this);
            descriptionText = shopPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (descriptionText == null)
            {
                Debug.LogError("No TextMeshProUGUI found in ShopPanel children.", this);
                return;
            }
            else
            {
                Debug.Log($"Found TextMeshProUGUI: {descriptionText.name}.", descriptionText);
            }
        }
        if (descriptionFont != null)
        {
            descriptionText.font = descriptionFont;
            Debug.Log($"Applied font {descriptionFont.name} to DescriptionText ({descriptionText.name}).", descriptionText);
        }

        // Ensure all tower buttons are parented and inactive
        foreach (var data in Towers)
        {
            if (data.towerButton != null)
            {
                if (data.towerButton.transform.parent != shopPanel.transform)
                {
                    data.towerButton.transform.SetParent(shopPanel.transform, false);
                    Debug.Log($"Parented tower button {data.towerButton.name} to ShopPanel.", data.towerButton);
                }
                data.towerButton.gameObject.SetActive(false);
                Debug.Log($"Deactivated tower button: {data.towerButton.name}.", data.towerButton);
            }
            else
            {
                Debug.LogWarning($"TowerButton is null for tower: {data.towerPrefab?.name ?? "Unknown"}.", this);
            }
        }

        Debug.Log($"SetupShopPanel completed. Tower count: {Towers.Length}, DescriptionText: {descriptionText?.name ?? "null"}.", this);
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
            towerIcon.enabled = data.towerSprite != null;
            RectTransform iconRect = towerIcon.GetComponent<RectTransform>();
            if (iconRect != null)
            {
                Vector2 size = iconRect.sizeDelta;
                size.x = baseIconWidth * data.iconWidthMultiplier;
                iconRect.sizeDelta = size;
                Debug.Log($"Set TowerIcon width to {size.x} (multiplier: {data.iconWidthMultiplier}).", towerIcon);
            }
            Debug.Log($"Updated TowerIcon to {data.towerSprite?.name ?? "None"}, Enabled: {towerIcon.enabled}.", towerIcon);
        }
        if (descriptionText != null)
        {
            descriptionText.text = data.description;
            descriptionText.enabled = !string.IsNullOrEmpty(data.description);
            needsCanvasUpdate = true;
            Debug.Log($"Updated DescriptionText to: '{data.description}', Enabled: {descriptionText.enabled}, Name: {descriptionText.name}.", descriptionText);
        }
        if (data.costLabel != null)
        {
            data.costLabel.text = data.cost.ToString();
            Debug.Log($"Updated CostLabel to: {data.cost}.", data.costLabel);
        }
        if (data.towerButton != null)
        {
            data.towerButton.gameObject.SetActive(true);
            data.towerButton.onClick.RemoveAllListeners();
            data.towerButton.onClick.AddListener(() => SelectTower(currentTowerIndex));
            data.towerButton.enabled = true;
            data.towerButton.interactable = true;
            Debug.Log($"Updated tower button: {data.towerButton.name}, Active: {data.towerButton.gameObject.activeSelf}.", data.towerButton);
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
                Debug.Log($"Updated cost label for {data.towerPrefab?.name}: {data.cost}", this);
            }
        }
    }
}