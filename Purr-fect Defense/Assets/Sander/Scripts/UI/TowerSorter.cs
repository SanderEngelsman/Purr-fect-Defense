using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"SpriteRenderer missing on {gameObject.name}. Sorting will not work.", this);
        }
    }

    private void Update()
    {
        if (spriteRenderer != null)
        {
            // Lower Y = higher sortingOrder (appears in front)
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
        }
    }
}
