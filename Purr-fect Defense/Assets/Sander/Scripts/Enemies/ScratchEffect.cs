using UnityEngine;

public class ScratchEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.3f; // Configurable duration

    private void Start()
    {
        Destroy(gameObject, duration); // Auto-destroy after duration
    }
}