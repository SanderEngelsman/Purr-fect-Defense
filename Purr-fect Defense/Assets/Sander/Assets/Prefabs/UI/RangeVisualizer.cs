using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeVisualizer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    [SerializeField] private float radius = 3f;
    [SerializeField] private Color rangeColor = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent red
    private int segments = 50;

    private void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = rangeColor;
        lineRenderer.endColor = rangeColor;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingLayerName = "Towers";
        lineRenderer.sortingOrder = 1; // Slightly above tower sprite
    }

    public void SetRange(float newRadius)
    {
        radius = newRadius;
        UpdateRange();
    }

    private void UpdateRange()
    {
        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0) + transform.position);
            angle += 360f / segments;
        }
    }

    private void Update()
    {
        UpdateRange(); // Update position if tower moves
    }

    public void Show()
    {
        lineRenderer.enabled = true;
    }

    public void Hide()
    {
        lineRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            Destroy(lineRenderer.material);
        }
    }
}
