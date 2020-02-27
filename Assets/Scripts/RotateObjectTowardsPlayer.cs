using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectTowardsPlayer : MonoBehaviour
{
    private GameObject _player;
    [SerializeField] private float _speed = 1f;

    private bool _rotate = false;

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.Find("Player");
        if (_player == null)
        {
            Debug.LogError("BossEnemy: Unable to find Player gameObject.");
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_player != null && _rotate == true)
        {
            Vector2 direction = _player.transform.position - transform.position;

            float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90.0f;

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, _speed * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(0f, Vector3.forward), _speed * Time.deltaTime);
        }
        
    }

    public void StartRotation()
    {
        _rotate = true;
    }
}
