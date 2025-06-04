using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public Tilemap placementTilemap;
    public Tilemap pathTilemap;
    private GameObject towerToPlace;
    private float towerCost;
    private Dictionary<Vector3Int, GameObject> placedTowers = new Dictionary<Vector3Int, GameObject>();
    private GameObject previewTower;
    private RangeVisualizer previewRangeVisualizer;
    private Vector3Int lastHighlightedCell;
    private GameManager gameManager;
    private static int towerOrderCounter = 0; // Tracks tower placement order

    private void OnValidate()
    {
        if (placementTilemap == null)
            Debug.LogWarning("PlacementTilemap is not assigned in TilemapManager.", this);
        if (pathTilemap == null)
            Debug.LogWarning("PathTilemap is not assigned in TilemapManager.", this);
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void StartPlacingTower(GameObject towerPrefab, float cost)
    {
        CancelPlacement(); // Clear any existing selection
        towerToPlace = towerPrefab;
        towerCost = cost;
        if (towerToPlace != null)
        {
            previewTower = Instantiate(towerToPlace, Vector3.zero, Quaternion.identity);
            // Disable tower scripts to prevent attacking or obstruction
            MonoBehaviour[] scripts = previewTower.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                script.enabled = false;
            }
            SpriteRenderer renderer = previewTower.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = new Color(1f, 1f, 1f, 0.5f); // Semi-transparent
            }
            else
            {
                Debug.LogWarning($"Preview tower {towerPrefab.name} is missing SpriteRenderer.", towerPrefab);
            }
            // Add range visualizer for attacking towers
            Tower tower = previewTower.GetComponent<Tower>();
            if (tower != null && CanAttack(tower))
            {
                previewRangeVisualizer = previewTower.AddComponent<RangeVisualizer>();
                previewRangeVisualizer.SetRange(tower.range);
                previewRangeVisualizer.Show();
            }
        }
    }

    public void CancelPlacement()
    {
        if (towerToPlace != null)
        {
            ClearPreview();
            towerToPlace = null;
            towerCost = 0f;
        }
    }

    public bool IsPlacingTower()
    {
        return towerToPlace != null;
    }

    private void Update()
    {
        if (towerToPlace != null)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0; // Ensure z is 0 for 2D
            bool isShieldTower = towerToPlace.GetComponent<ShieldTower>() != null;
            Tilemap targetTilemap = isShieldTower ? pathTilemap : placementTilemap;
            Tilemap otherTilemap = isShieldTower ? placementTilemap : pathTilemap;
            Vector3Int cellPos = targetTilemap.WorldToCell(worldPos);

            // Update preview position
            if (previewTower != null)
            {
                previewTower.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
            }

            // Highlight tile
            ClearHighlight();
            bool canPlace = targetTilemap != null && targetTilemap.HasTile(cellPos) && !placedTowers.ContainsKey(cellPos) && gameManager.HasEnoughCurrency(towerCost);
            if (isShieldTower)
            {
                // For ShieldTower, allow placement even if PlacementTilemap has a tile
                canPlace = canPlace && pathTilemap.HasTile(cellPos);
            }
            else
            {
                // For other towers, ensure PathTilemap has no tile
                canPlace = canPlace && (otherTilemap == null || !otherTilemap.HasTile(cellPos));
            }

            if (canPlace)
            {
                targetTilemap.SetTileFlags(cellPos, TileFlags.None);
                targetTilemap.SetColor(cellPos, Color.green);
                lastHighlightedCell = cellPos;
            }
            else
            {
                Debug.Log($"Cannot place {towerToPlace.name} at {cellPos} (World: {worldPos}). " +
                          $"Target tilemap ({targetTilemap?.name}): HasTile={targetTilemap?.HasTile(cellPos)}, " +
                          $"Other tilemap ({otherTilemap?.name}): HasTile={otherTilemap != null && otherTilemap.HasTile(cellPos)}, " +
                          $"Tile occupied: {placedTowers.ContainsKey(cellPos)}, " +
                          $"Sufficient currency: {gameManager.HasEnoughCurrency(towerCost)}");
            }

            // Place tower
            if (Input.GetMouseButtonDown(0))
            {
                if (canPlace)
                {
                    GameObject tower = Instantiate(towerToPlace, targetTilemap.GetCellCenterWorld(cellPos), Quaternion.identity);
                    // Assign placement order for layering
                    ZLayering zLayering = tower.GetComponent<ZLayering>();
                    if (zLayering != null)
                    {
                        zLayering.SetOrder(++towerOrderCounter);
                        Debug.Log($"Placed tower: {towerToPlace.name} at {targetTilemap.GetCellCenterWorld(cellPos)}, order={towerOrderCounter}", tower);
                    }
                    else
                    {
                        Debug.LogWarning($"Tower {tower.name} missing ZLayering component.", tower);
                    }
                    gameManager.RemoveCurrency(towerCost); // Deduct currency
                    placedTowers.Add(cellPos, tower);
                    ClearPreview();
                    towerToPlace = null;
                    towerCost = 0f;
                }
                else
                {
                    Debug.LogWarning($"Failed to place {towerToPlace.name} at {cellPos} (World: {worldPos}).");
                }
            }

            // Cancel placement
            if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
    }

    private void ClearPreview()
    {
        if (previewTower != null)
        {
            Destroy(previewTower);
            previewTower = null;
            previewRangeVisualizer = null;
        }
        ClearHighlight();
    }

    private void ClearHighlight()
    {
        if (placementTilemap != null && placementTilemap.HasTile(lastHighlightedCell))
        {
            placementTilemap.SetTileFlags(lastHighlightedCell, TileFlags.None);
            placementTilemap.SetColor(lastHighlightedCell, Color.white);
        }
        if (pathTilemap != null && pathTilemap.HasTile(lastHighlightedCell))
        {
            pathTilemap.SetTileFlags(lastHighlightedCell, TileFlags.None);
            pathTilemap.SetColor(lastHighlightedCell, Color.white);
        }
    }

    public void RemoveTower(Vector3Int cellPos)
    {
        placedTowers.Remove(cellPos);
    }

    private bool CanAttack(Tower tower)
    {
        // Exclude GeneratorTower and ShieldTower
        return tower is ProjectileTower || tower is ScratchTower;
    }
}
