using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveMine : MonoBehaviour
{
    [SerializeField] private GameObject _explosionEffect;
    [SerializeField] private float _blastRadius = 8.0f;

    public void Explode()
    {
        Instantiate(_explosionEffect, transform.position, Quaternion.identity);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _blastRadius);

        foreach (Collider2D nearbyObject in colliders)
        {
            Rigidbody2D rb = nearbyObject.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                if (nearbyObject.tag == "Enemy")
                {
                    GameObject enemy = nearbyObject.gameObject;
                    enemy.GetComponent<Enemy>().LaserDestroysEnemy();
                }
            }
        }

        Destroy(this.gameObject);
    }
}
