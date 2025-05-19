using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public Tilemap placementTilemap;
    public GameObject towerToPlace;

    public void StartPlacingTower(GameObject towerPrefab)
    {
        towerToPlace = towerPrefab;
    }

    private void Update()
    {
        if (towerToPlace != null && Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = placementTilemap.WorldToCell(worldPos);
            if (placementTilemap.HasTile(cellPos))
            {
                Instantiate(towerToPlace, placementTilemap.GetCellCenterWorld(cellPos), Quaternion.identity);
                towerToPlace = null;
            }
        }
    }
}
