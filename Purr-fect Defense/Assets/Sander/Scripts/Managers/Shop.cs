using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private Transform contentParent;
    private GameManager gameManager;
    private TilemapManager tilemapManager;
    private ShopPanelAnimator shopPanelAnimator;
    private bool isOpen = false;
    private float lastToggleTime;
    private const float TOGGLE_DEBOUNCE = 0.2f;

    private void OnValidate()
    {
        Debug.Log("Shop OnValidate triggered.", this);
        if (shopPanel == null)
            Debug.LogWarning("ShopPanel is not assigned in Inspector.", this);
        if (shopButton == null)
            Debug.LogWarning("ShopButton is not assigned in Shop.", this);
        if (contentParent == null)
            Debug.LogWarning("ContentParent is not assigned in Shop.", this);
        if (Towers == null || Towers.Length == 0)
            Debug.LogWarning("Towers array is null or empty in Shop.", this);
        foreach (var data in Towers)
        {
            if (data.towerButton == null)
                Debug.LogWarning("TowerButton is not assigned in ShopTowerData.", this);
            if (data.costLabel == null)
                Debug.LogWarning("CostLabel is not assigned in ShopTowerData.", this);
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
            var shop = FindObjectOfType<Shop>();
            if (shop == null)
            {
                Debug.LogError("No Shop component found in scene at runtime.", this);
            }
            else
            {
                Debug.Log($"Found Shop component on {shop.gameObject.name}.", this);
            }

            gameManager = FindObjectOfType<GameManager>();
            tilemapManager = FindObjectOfType<TilemapManager>();
            shopPanelAnimator = shopPanel != null ? shopPanel.GetComponent<ShopPanelAnimator>() : null;
            if (shopPanelAnimator == null && shopPanel != null)
            {
                Debug.LogWarning("ShopPanelAnimator is missing on ShopPanel.", shopPanel);
            }
            if (shopButton == null)
            {
                Debug.LogError("ShopButton is null in Start. Cannot add ToggleShop listener.", this);
            }
            else
            {
                shopButton.onClick.RemoveAllListeners();
                shopButton.onClick.AddListener(ToggleShop);
                Debug.Log($"ToggleShop listener added to ShopButton ({shopButton.name}).", this);
            }
            SetupTowerEntries();
            UpdateCosts();
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
                Debug.Log("ShopPanel set inactive.", this);
            }
            else
            {
                Debug.LogError("ShopPanel is null in Start.", this);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception in Shop Start: {e.Message}\nStackTrace: {e.StackTrace}", this);
        }
    }

    private void SetupTowerEntries()
    {
        Debug.Log("SetupTowerEntries started.", this);
        if (contentParent == null)
        {
            Debug.LogError("ContentParent is null in SetupTowerEntries.", this);
            return;
        }
        if (Towers == null || Towers.Length == 0)
        {
            Debug.LogError("Towers array is null or empty in SetupTowerEntries.", this);
            return;
        }

        foreach (Transform child in contentParent)
        {
            if (!IsShopTowerButton(child))
                Destroy(child.gameObject);
        }
        for (int i = 0; i < Towers.Length; i++)
        {
            Debug.Log($"Creating entry for tower {i}: {Towers[i].towerPrefab?.name}", this);
            if (Towers[i].towerButton == null)
            {
                Debug.LogWarning($"TowerButton is null for Towers[{i}]. Skipping.", this);
                continue;
            }
            if (Towers[i].towerSprite == null)
            {
                Debug.LogWarning($"TowerSprite is null for Towers[{i}].", this);
            }
            if (string.IsNullOrEmpty(Towers[i].description))
            {
                Debug.LogWarning($"Description is empty for Towers[{i}].", this);
            }

            int index = i;
            GameObject container = new GameObject($"TowerEntry_{Towers[i].towerPrefab?.name ?? "Unknown"}");
            container.transform.SetParent(contentParent, false);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(200, 300);
            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 15;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            GameObject imageObj = new GameObject("TowerImage");
            imageObj.transform.SetParent(container.transform, false);
            Image towerImage = imageObj.AddComponent<Image>();
            towerImage.sprite = Towers[i].towerSprite;
            towerImage.preserveAspect = true;
            RectTransform imageRect = imageObj.GetComponent<RectTransform>();
            imageRect.sizeDelta = new Vector2(100, 100);
            Towers[i].towerButton.transform.SetParent(container.transform, false);
            Towers[i].towerButton.onClick.RemoveAllListeners();
            Towers[i].towerButton.onClick.AddListener(() => SelectTower(index));
            GameObject descObj = new GameObject("DescriptionText");
            descObj.transform.SetParent(container.transform, false);
            TextMeshProUGUI descriptionText = descObj.AddComponent<TextMeshProUGUI>();
            descriptionText.text = Towers[i].description;
            descriptionText.fontSize = 16;
            descriptionText.alignment = TextAlignmentOptions.Center;
            descriptionText.enableWordWrapping = true;
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.sizeDelta = new Vector2(180, 30);
        }
        UpdateCosts();
        Debug.Log($"SetupTowerEntries completed. {Towers.Length} entries created.", this);
    }

    private bool IsShopTowerButton(Transform child)
    {
        foreach (var data in Towers)
        {
            if (child != null && data.towerButton != null && data.towerButton.transform == child)
                return true;
        }
        return false;
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
        shopPanel.SetActive(true);
        if (isOpen)
        {
            shopPanelAnimator?.OpenPanel();
            tilemapManager?.CancelPlacement();
            UpdateCosts();
            Debug.Log("Shop opened.", this);
        }
        else
        {
            shopPanelAnimator?.ClosePanel();
            tilemapManager?.CancelPlacement();
            Debug.Log("Shop closed.", this);
        }
    }

    public void SelectTower(int index)
    {
        if (!gameManager.HasEnoughCurrency(Towers[index].cost))
        {
            Debug.Log($"Not enough currency to select {Towers[index].towerPrefab?.name}. Required: {Towers[index].cost}", this);
            return;
        }
        tilemapManager?.StartPlacingTower(Towers[index].towerPrefab, Towers[index].cost);
        ToggleShop();
        Debug.Log($"Selected tower: {Towers[index].towerPrefab?.name}, Cost: {Towers[index].cost}", this);
    }

    public void UpdateCosts()
    {
        foreach (var data in Towers)
        {
            if (data.costLabel != null)
            {
                data.costLabel.text = data.cost.ToString();
                bool canAfford = gameManager.HasEnoughCurrency(data.cost);
                data.costLabel.color = canAfford ? Color.white : Color.red;
            }
        }
    }
}