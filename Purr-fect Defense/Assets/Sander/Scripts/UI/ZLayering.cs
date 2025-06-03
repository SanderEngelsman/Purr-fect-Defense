using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZLayering : MonoBehaviour
{
    private Enemy enemy;
    private bool isStatic = false;
    private const float BASE_Y = 1.5f;
    private const float FLYING_Z = -99f;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        // Towers are static, enemies are dynamic
        isStatic = GetComponent<Tower>() != null || GetComponent<ShieldTower>() != null;
        if (enemy != null)
        {
            Debug.Log($"ZLayering initialized on {gameObject.name}: isFlying={enemy.isFlying}", this);
        }
        if (isStatic)
        {
            UpdateZPosition();
        }
    }

    private void Update()
    {
        if (!isStatic)
        {
            UpdateZPosition();
        }
    }

    private void UpdateZPosition()
    {
        float yPos = transform.position.y;
        float zPos;

        if (enemy != null && enemy.isFlying)
        {
            zPos = FLYING_Z;
            Debug.Log($"Flying enemy {gameObject.name}: Setting Z to {zPos} at Y={yPos}", this);
        }
        else
        {
            zPos = yPos - BASE_Y; // Higher Y → Higher Z (behind); Lower Y → Lower Z (in front)
            // Debug.Log($"Updated {gameObject.name} Z to {zPos} at Y={yPos}", this);
        }

        transform.position = new Vector3(transform.position.x, yPos, zPos);
    }
}
