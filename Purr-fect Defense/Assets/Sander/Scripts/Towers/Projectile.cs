using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float speed;

    public void SetTarget(GameObject target, float damage, float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        if (target != null)
        {
            // Match target's Z to avoid Z movement
            transform.position = new Vector3(transform.position.x, transform.position.y, target.transform.position.z);
            Debug.Log($"Projectile initialized: target={target.name}, speed={speed}, Z={transform.position.z}", this);
        }
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move in 2D (X, Y only)
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetPos = new Vector2(target.transform.position.x, target.transform.position.y);
        Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, speed * Time.deltaTime);

        // Update position, keeping target's Z
        transform.position = new Vector3(newPos.x, newPos.y, target.transform.position.z);

        // Check hit in 2D
        if (Vector2.Distance(currentPos, targetPos) < 0.1f)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Projectile hit {target.name}: damage={damage}, isFlying={enemy.isFlying}", this);
            }
            Destroy(gameObject);
        }
    }
}
