using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public Tilemap placementTilemap;
    public Tilemap pathTilemap;
    public GameObject towerToPlace;
    private Dictionary<Vector3Int, GameObject> placedTowers = new Dictionary<Vector3Int, GameObject>();
    private GameObject previewTower;
    private Vector3Int lastHighlightedCell;

    private void OnValidate()
    {
        if (placementTilemap == null)
            Debug.LogWarning("PlacementTilemap is not assigned in TilemapManager.", this);
        if (pathTilemap == null)
            Debug.LogWarning("PathTilemap is not assigned in TilemapManager.", this);
    }

    public void StartPlacingTower(GameObject towerPrefab)
    {
        towerToPlace = towerPrefab;
        if (towerToPlace != null)
        {
            previewTower = Instantiate(towerToPlace, Vector3.zero, Quaternion.identity);
            SpriteRenderer renderer = previewTower.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = new Color(1f, 1f, 1f, 0.5f); // Semi-transparent
            }
            else
            {
                Debug.LogWarning($"Preview tower {towerPrefab.name} is missing SpriteRenderer.", towerPrefab);
            }
        }
    }

    public void CancelPlacement()
    {
        if (towerToPlace != null)
        {
            ClearPreview();
            towerToPlace = null;
        }
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
            bool canPlace = targetTilemap != null && targetTilemap.HasTile(cellPos) && !placedTowers.ContainsKey(cellPos);
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
                          $"Tile occupied: {placedTowers.ContainsKey(cellPos)}");
            }

            // Place tower
            if (Input.GetMouseButtonDown(0))
            {
                if (canPlace)
                {
                    GameObject tower = Instantiate(towerToPlace, targetTilemap.GetCellCenterWorld(cellPos), Quaternion.identity);
                    placedTowers.Add(cellPos, tower);
                    ClearPreview();
                    towerToPlace = null;
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
}
