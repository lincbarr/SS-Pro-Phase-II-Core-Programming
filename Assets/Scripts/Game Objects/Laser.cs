using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private float _speed = 8.0f;

    private bool _isEnemyLaser = false;
    private bool _isEnemyLaserUp = false;

    private bool _isHoming = false;
    private bool _foundHomingTarget = false;
    private GameObject _closestEnemy;

    private void Start()
    {
        if (_isEnemyLaser || _isEnemyLaserUp)
        {
            transform.tag = "EnemyLaser";
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_isEnemyLaser == false)
        {
            if (_isHoming == true)
            {
                Home();
            } else
            {
                MoveUp();
            }
        } else
        {
            MoveDown();
        }
    }

    private void MoveUp()
    {
        transform.Translate(Vector3.up * _speed * Time.deltaTime);

        if (transform.position.y > 8.0f)
        {
            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }

            Destroy(this.gameObject);
        }
    }

    private void MoveDown()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y < -8.0f || transform.position.x < -9f || transform.position.x > 9f)
        {
            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }

            Destroy(this.gameObject);
        }
    }

    public void AssignEnemyLaser()
    {
        _isEnemyLaser = true;
    }

    public void AssignEnemyLaserUp()
    {
        _isEnemyLaserUp = true;
    }

    public bool IsEnemyLasers()
    {
        return _isEnemyLaser || _isEnemyLaserUp;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && (_isEnemyLaser == true || _isEnemyLaserUp == true))
        {
            Player player = other.GetComponent<Player>();

            if (player != null)
            {
                player.Damage();
            }

            Destroy(this.gameObject);
        }

        if (other.tag == "PowerUp" && _isEnemyLaser == true)
        {
            PowerUp powerUp = other.gameObject.GetComponent<PowerUp>();
            powerUp.DestroyPowerUp();

            Destroy(this.gameObject);
        }

        if (other.tag == "BossEnemy" && _isEnemyLaser == false)
        {
            Destroy(this.gameObject);
        }


    }

    // Homing

    private GameObject FindClosestEnemy()
    {
        GameObject[] enemies;
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) //No enemies found
        {
            return null;
        }
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject enemy in enemies)
        {
            Vector3 diff = enemy.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = enemy;
                distance = curDistance;
            }
        }
        return closest;
    }

    private void Home()
    {
        if (_foundHomingTarget == false)
        {
            _closestEnemy = FindClosestEnemy();
            if (_closestEnemy == null)
            {
                MoveUp();
            } else
            {
                _foundHomingTarget = true;
            }

        } else
        {
            if (_closestEnemy != null) {
                float normalizedSpeed = _speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, _closestEnemy.transform.position, normalizedSpeed);

                Vector3 direction = _closestEnemy.transform.position - transform.position;
                float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90.0f;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = rotation;
            } else
            {
                _foundHomingTarget = false;
                Destroy(this.gameObject);
            }
            
        }
        
    }

    public void ActivateHoming()
    {
        _isHoming = true;
    }


}
