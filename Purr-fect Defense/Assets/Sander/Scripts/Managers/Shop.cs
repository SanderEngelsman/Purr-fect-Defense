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
        for (int i = 0; i < Towers.Length; i++)
        {
            var data = Towers[i];
            if (data.towerButton == null)
                Debug.LogWarning($"TowerButton is null for Towers[{i}] ({data.towerPrefab?.name ?? "Unknown"}).", this);
            if (data.costLabel == null)
                Debug.LogWarning($"CostLabel is null for Towers[{i}] ({data.towerPrefab?.name ?? "Unknown"}).", this);
            if (data.resellPrice < 0)
                Debug.LogWarning($"ResellPrice for {data.towerPrefab?.name} is negative.", this);
            if (data.towerButton != null && !data.towerButton.gameObject.activeSelf)
                Debug.LogWarning($"TowerButton {data.towerButton.name} for Towers[{i}] is inactive in Inspector.", this);
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

        // Log existing children
        Debug.Log($"ContentParent has {contentParent.childCount} children before setup.", this);
        foreach (Transform child in contentParent)
        {
            Debug.Log($"Child in ContentParent: {child.name}, Active: {child.gameObject.activeSelf}", child);
        }

        for (int i = 0; i < Towers.Length; i++)
        {
            var data = Towers[i];
            Debug.Log($"Processing tower {i}: {data.towerPrefab?.name ?? "Unknown"}", this);
            if (data.towerButton == null)
            {
                Debug.LogWarning($"TowerButton is null for Towers[{i}] ({data.towerPrefab?.name}). Skipping entry.", this);
                continue;
            }
            if (!data.towerButton.gameObject.activeSelf)
            {
                Debug.Log($"Activating button {data.towerButton.name} for {data.towerPrefab?.name}.", this);
                data.towerButton.gameObject.SetActive(true);
            }
            if (data.towerSprite == null)
            {
                Debug.LogWarning($"TowerSprite is null for Towers[{i}].", this);
            }
            if (string.IsNullOrEmpty(data.description))
            {
                Debug.LogWarning($"Description is empty for Towers[{i}].", this);
            }

            int index = i;
            GameObject container = new GameObject($"TowerEntry_{data.towerPrefab?.name ?? "Unknown"}");
            container.transform.SetParent(contentParent, false);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(180, 200);
            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            GameObject imageObj = new GameObject("TowerIcon");
            imageObj.transform.SetParent(container.transform, false);
            Image towerImage = imageObj.AddComponent<Image>();
            towerImage.sprite = data.towerSprite;
            towerImage.preserveAspect = true;
            RectTransform imageRect = imageObj.GetComponent<RectTransform>();
            imageRect.sizeDelta = new Vector2(80, 80);
            Debug.Log($"Added TowerIcon for {data.towerPrefab?.name}, Active: {imageObj.activeSelf}", imageObj);

            Debug.Log($"Parenting button {data.towerButton.name} to {container.name}, Button Active: {data.towerButton.gameObject.activeSelf}", this);
            data.towerButton.transform.SetParent(container.transform, false);
            RectTransform buttonRect = data.towerButton.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(100, 30);
            buttonRect.localScale = Vector3.one; // Ensure scale is 1
            if (data.towerButton.GetComponent<Image>() == null)
            {
                Debug.Log($"Adding Image component to button {data.towerButton.name}.", this);
                Image buttonImage = data.towerButton.gameObject.AddComponent<Image>();
                buttonImage.color = Color.white; // Default white for visibility
            }
            data.towerButton.onClick.RemoveAllListeners();
            data.towerButton.onClick.AddListener(() => SelectTower(index));
            Debug.Log($"Assigned SelectTower({index}) to button {data.towerButton.name}, Interactable: {data.towerButton.interactable}", this);

            GameObject descObj = new GameObject("DescriptionText");
            descObj.transform.SetParent(container.transform, false);
            TextMeshProUGUI descriptionText = descObj.AddComponent<TextMeshProUGUI>();
            descriptionText.text = data.description;
            descriptionText.fontSize = 14;
            descriptionText.alignment = TextAlignmentOptions.Center;
            descriptionText.enableWordWrapping = true;
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.sizeDelta = new Vector2(160, 40);
            Debug.Log($"Added DescriptionText for {data.towerPrefab?.name}, Active: {descObj.activeSelf}", descObj);
        }
        UpdateCosts();
        Debug.Log($"SetupTowerEntries completed. Created {Towers.Length} entries.", this);
    }

    private bool IsShopTowerButton(Transform child)
    {
        foreach (var data in Towers)
        {
            if (child != null && data.towerButton != null && data.towerButton.transform == child)
            {
                Debug.Log($"Identified {child.name} as a shop tower button.", this);
                return true;
            }
        }
        Debug.Log($"Child {child.name} is not a shop tower button.", this);
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
                Debug.Log($"Updated cost label for {data.towerPrefab?.name}: {data.cost}, Color: {data.costLabel.color}", this);
            }
            else
            {
                Debug.LogWarning($"CostLabel is null for {data.towerPrefab?.name}.", this);
            }
        }
    }
}