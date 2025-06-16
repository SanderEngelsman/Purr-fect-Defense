using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    [SerializeField] private Tilemap placementTilemap;
    [SerializeField] private Tilemap pathTilemap;
    [SerializeField] private Tilemap rightFacingScratchTowerMap;
    private GameObject towerToPlace;
    private float towerCost;
    private Dictionary<Vector3Int, GameObject> placedTowers = new Dictionary<Vector3Int, GameObject>();
    private GameObject previewTower;
    private RangeVisualizer previewRangeVisualizer;
    private Vector3Int lastHighlightedCell;
    private Vector3Int lastHighlightedLeftCell;
    private GameManager gameManager;
    private static int towerOrderCounter = 0;

    public Tilemap PlacementTilemap => placementTilemap;
    public Tilemap PathTilemap => pathTilemap;
    public Tilemap RightFacingScratchTowerMap => rightFacingScratchTowerMap;

    private void OnValidate()
    {
        if (placementTilemap == null)
            Debug.LogWarning("PlacementTilemap is not assigned in TilemapManager.", this);
        if (pathTilemap == null)
            Debug.LogWarning("PathTilemap is not assigned in TilemapManager.", this);
        if (rightFacingScratchTowerMap == null)
            Debug.LogWarning("RightFacingScratchTowerMap is not assigned in TilemapManager.", this);
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void StartPlacingTower(GameObject towerPrefab, float cost)
    {
        CancelPlacement();
        towerToPlace = towerPrefab;
        towerCost = cost;
        if (towerToPlace != null)
        {
            previewTower = Instantiate(towerPrefab, Vector3.zero, Quaternion.identity);
            MonoBehaviour[] scripts = previewTower.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                script.enabled = false;
            }
            SpriteRenderer renderer = previewTower.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = new Color(1f, 1f, 1f, 0.6f);
            }
            else
            {
                Debug.LogWarning($"Preview tower {towerPrefab.name} missing SpriteRenderer.", this);
            }
            Tower tower = previewTower.GetComponent<Tower>();
            if (tower != null && CanAttack(tower))
            {
                previewRangeVisualizer = previewTower.AddComponent<RangeVisualizer>();
                previewRangeVisualizer.SetRange(tower.range);
                previewRangeVisualizer.Show();
            }
            Debug.Log($"Started placing tower: {towerPrefab.name}, cost: {cost}", this);
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
            worldPos.z = 0;
            bool isShieldTower = towerToPlace.GetComponent<ShieldTower>() != null;
            bool isGeneratorTower = towerToPlace.GetComponent<GeneratorTower>() != null;
            bool isScratchTower = towerToPlace.GetComponent<ScratchTower>() != null;
            Tilemap targetTilemap = isShieldTower ? pathTilemap : (isScratchTower ? GetScratchTowerTilemap(worldPos) : placementTilemap);
            Tilemap otherTilemap = isShieldTower ? placementTilemap : pathTilemap;
            Vector3Int cellPos = targetTilemap.WorldToCell(worldPos);
            Vector3Int leftCellPos = cellPos + Vector3Int.left;

            if (previewTower != null)
            {
                Vector3 previewPos = isGeneratorTower ?
                    targetTilemap.GetCellCenterWorld(cellPos) - new Vector3(0.5f, 0, 0) :
                    targetTilemap.GetCellCenterWorld(cellPos);
                previewTower.transform.position = new Vector3(previewPos.x, previewPos.y, 0);
                if (isScratchTower)
                {
                    ScratchTower scratchTower = previewTower.GetComponent<ScratchTower>();
                    if (scratchTower != null)
                    {
                        bool faceRight = rightFacingScratchTowerMap != null && rightFacingScratchTowerMap.HasTile(cellPos);
                        scratchTower.SetFacingDirection(faceRight);
                    }
                }
            }

            ClearHighlight();
            bool canPlace = IsValidPlacementPosition(cellPos, isShieldTower, isGeneratorTower, isScratchTower);
            if (isGeneratorTower)
            {
                canPlace &= IsValidPlacementPosition(leftCellPos, isShieldTower, isGeneratorTower, isScratchTower);
            }

            if (canPlace)
            {
                targetTilemap.SetTileFlags(cellPos, TileFlags.None);
                targetTilemap.SetColor(cellPos, Color.green);
                lastHighlightedCell = cellPos;
                if (isGeneratorTower)
                {
                    targetTilemap.SetTileFlags(leftCellPos, TileFlags.None);
                    targetTilemap.SetColor(leftCellPos, Color.green);
                    lastHighlightedLeftCell = leftCellPos;
                }
                Debug.Log($"Highlighting tile(s) at {cellPos}{(isGeneratorTower ? $" and {leftCellPos}" : "")} for {towerToPlace.name}", this);
            }
            else
            {
                Debug.Log($"Cannot place {towerToPlace.name} at {cellPos}{(isGeneratorTower ? $" or {leftCellPos}" : "")}. " +
                          $"Target tilemap ({targetTilemap?.name}): HasTile={targetTilemap?.HasTile(cellPos)}, " +
                          $"Left tile HasTile={targetTilemap?.HasTile(leftCellPos)}, " +
                          $"Other tilemap ({otherTilemap?.name}): HasTile={otherTilemap != null && otherTilemap.HasTile(cellPos)}, " +
                          $"Tile occupied: {placedTowers.ContainsKey(cellPos)} or {placedTowers.ContainsKey(leftCellPos)}, " +
                          $"Sufficient currency: {gameManager.HasEnoughCurrency(towerCost)}", this);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (canPlace)
                {
                    Vector3 placePos = isGeneratorTower ?
                        targetTilemap.GetCellCenterWorld(cellPos) - new Vector3(0.5f, 0, 0) :
                        targetTilemap.GetCellCenterWorld(cellPos);
                    GameObject tower = Instantiate(towerToPlace, placePos, Quaternion.identity);
                    if (isScratchTower)
                    {
                        ScratchTower scratchTower = tower.GetComponent<ScratchTower>();
                        if (scratchTower != null)
                        {
                            bool faceRight = rightFacingScratchTowerMap != null && rightFacingScratchTowerMap.HasTile(cellPos);
                            scratchTower.SetFacingDirection(faceRight);
                        }
                    }
                    ZLayering zLayering = tower.GetComponent<ZLayering>();
                    if (zLayering != null)
                    {
                        zLayering.SetOrder(++towerOrderCounter);
                        Debug.Log($"Placed tower: {towerToPlace.name} at {placePos}, order={towerOrderCounter}", tower);
                    }
                    else
                    {
                        Debug.LogWarning($"Tower {tower.name} missing ZLayering component.", tower);
                    }
                    gameManager.RemoveCurrency(towerCost);
                    placedTowers.Add(cellPos, tower);
                    if (isGeneratorTower)
                    {
                        placedTowers.Add(leftCellPos, tower);
                    }
                    AudioManager.Instance?.PlayTowerPlaceSound();
                    ClearPreview();
                    towerToPlace = null;
                    towerCost = 0f;
                }
                else
                {
                    Debug.LogWarning($"Failed to place {towerToPlace.name} at {cellPos}.", this);
                }
            }

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
        if (placementTilemap != null && placementTilemap.HasTile(lastHighlightedLeftCell))
        {
            placementTilemap.SetTileFlags(lastHighlightedLeftCell, TileFlags.None);
            placementTilemap.SetColor(lastHighlightedLeftCell, Color.white);
        }
        if (pathTilemap != null && pathTilemap.HasTile(lastHighlightedCell))
        {
            pathTilemap.SetTileFlags(lastHighlightedCell, TileFlags.None);
            pathTilemap.SetColor(lastHighlightedCell, Color.white);
        }
        if (rightFacingScratchTowerMap != null && rightFacingScratchTowerMap.HasTile(lastHighlightedCell))
        {
            rightFacingScratchTowerMap.SetTileFlags(lastHighlightedCell, TileFlags.None);
            rightFacingScratchTowerMap.SetColor(lastHighlightedCell, Color.white);
        }
    }

    public void RemoveTower(Vector3Int cellPos)
    {
        if (placedTowers.ContainsKey(cellPos))
        {
            GameObject tower = placedTowers[cellPos];
            placedTowers.Remove(cellPos);
            Vector3Int leftCellPos = cellPos + Vector3Int.left;
            if (placedTowers.ContainsKey(leftCellPos) && placedTowers[leftCellPos] == tower)
            {
                placedTowers.Remove(leftCellPos);
            }
            Vector3Int rightCellPos = cellPos + Vector3Int.right;
            if (placedTowers.ContainsKey(rightCellPos) && placedTowers[rightCellPos] == tower)
            {
                placedTowers.Remove(rightCellPos);
            }
        }
    }

    private bool IsValidPlacementPosition(Vector3Int cellPos, bool isShieldTower, bool isGeneratorTower, bool isScratchTower)
    {
        if (towerToPlace == null)
        {
            return false;
        }

        Tilemap targetTilemap = isShieldTower ? pathTilemap : (isScratchTower ? GetScratchTowerTilemap(cellPos) : placementTilemap);
        Tilemap otherTilemap = isShieldTower ? placementTilemap : pathTilemap;

        bool hasTile = targetTilemap != null && targetTilemap.HasTile(cellPos);
        bool isPathTile = otherTilemap != null && otherTilemap.HasTile(cellPos);
        bool isOccupied = placedTowers.ContainsKey(cellPos);
        bool hasEnoughCurrency = gameManager.HasEnoughCurrency(towerCost);

        if (isShieldTower)
        {
            return hasTile && !isOccupied && hasEnoughCurrency;
        }
        else
        {
            return hasTile && !isPathTile && !isOccupied && hasEnoughCurrency;
        }
    }

    private Tilemap GetScratchTowerTilemap(Vector3 worldPos)
    {
        Vector3Int cellPos = rightFacingScratchTowerMap.WorldToCell(worldPos);
        if (rightFacingScratchTowerMap != null && rightFacingScratchTowerMap.HasTile(cellPos))
        {
            return rightFacingScratchTowerMap;
        }
        return placementTilemap;
    }

    private Tilemap GetScratchTowerTilemap(Vector3Int cellPos)
    {
        if (rightFacingScratchTowerMap != null && rightFacingScratchTowerMap.HasTile(cellPos))
        {
            return rightFacingScratchTowerMap;
        }
        return placementTilemap;
    }

    private bool CanAttack(Tower tower)
    {
        return tower is ProjectileTower || tower is ScratchTower;
    }
}