using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Tower target;
    private float stunDuration;

    public void SetTarget(Tower target, float stunDuration)
    {
        this.target = target;
        this.stunDuration = stunDuration;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.transform.position) < 0.1f)
        {
            target.Stun(stunDuration);
            Destroy(gameObject);
        }
    }
}
