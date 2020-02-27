                                                                                                                                       using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float _speed = 4.0f;
    [SerializeField] private AudioClip _explosionAudioClip;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private GameObject _laserBeamOrigin;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private GameObject _rearFiringOrigin;
    [SerializeField] private GameObject _shieldVisualizer;


    private Player _player;
    private Animator _anim;
    private AudioSource _audioSource;
    private float _fireRate = 3.0f;
    private float _canFire = -1.0f;
    private bool _rayTriggeredFiring = false;

    private float _destroyClipLength;

    //Aggressive Type
    private float _closeDistance = 4f;
    [Header("Enemy Type")]
    [SerializeField] private bool _aggressive = false;
    [SerializeField] private bool _fireLaserBeam = false;

    //Shield
    [SerializeField] private bool _activateShield = false;
    private int _shieldCount = 0;
    private int _maxShieldHits = 1;

    // Move on angle
    [SerializeField] private bool _moveOnAngle = false;
    private bool _moveOnAngleTriggered = false;
    private float _minAngle = -30f;  // In degrees
    private float _maxAngle = 30f;  // In degrees
    private float _currentAngle = 0f;

    // Avoid Player Lasers
    [SerializeField] private bool _isAvoidingLasers = false;
    private float _avoidanceRange = 5.0f;
    private bool _foundPlayerLaser = false;

    // Is behind Player
    [SerializeField] private bool _shootBehindPlayer = false;


    private void Start()
    {
        Physics2D.queriesStartInColliders = false; // To prevent Raycast from triggering on this gameObject

        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.LogError("Enemy: Unable to find Player script.");
        }

        _anim = GetComponent<Animator>();
        if (_anim == null)
        {
            Debug.LogError("Enemy: Unable to find Animator");
        }

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            Debug.LogError("Enemy: Unable to find AudioSource");
        }

        AnimationClip[] clips = _anim.runtimeAnimatorController.animationClips;
        _destroyClipLength = clips[0].length;

        _audioSource.clip = _explosionAudioClip;

        if (_activateShield == true)
        {
            _shieldCount = _maxShieldHits;
            _shieldVisualizer.SetActive(true);
        }

        _canFire = Random.Range(Time.time, Time.time + _fireRate);

    }

    private void FixedUpdate()
    {
        CalculateMovement();

        if (_shootBehindPlayer == true)
        {
            //Debug.DrawRay(_rearFiringOrigin.transform.position, transform.up * 6f, Color.red);

            RaycastHit2D backwardHit = Physics2D.Raycast(_rearFiringOrigin.transform.position, Vector2.up * 6f);

            if (backwardHit.collider != null)
            {
                if (backwardHit.transform.tag == "Player" && _rayTriggeredFiring == false)
                {
                    _rayTriggeredFiring = true;
                    FireLasers(aimForward: false, hitInfo: backwardHit);
                    _player.Damage();
                }
            }
        }

        //Debug.DrawRay(_laserBeamOrigin.transform.position, -Vector2.up * 10f);

        RaycastHit2D forwardHit = Physics2D.Raycast(_laserBeamOrigin.transform.position, -Vector2.up, 10f);

        if (forwardHit.collider != null)
        {
            if (forwardHit.transform.tag == "PowerUp" && _rayTriggeredFiring == false)
            {
                _rayTriggeredFiring = true;
                FireLasers(aimForward: true, hitInfo: forwardHit);
                forwardHit.transform.gameObject.GetComponent<PowerUp>().DestroyPowerUp();
            }
        }

        // Agressive behavior
        if (_aggressive == true)
        {
            Vector3 distanceToPlayer = _player.transform.position - transform.position;
            float sqaredDist = distanceToPlayer.sqrMagnitude;
            if (sqaredDist <= (_closeDistance * _closeDistance))
            {
                float normalizedSpeed = _speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, _player.transform.position, normalizedSpeed );
                Vector3 direction = _player.transform.position - transform.position;
                float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90.0f;

                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, normalizedSpeed );
            }
        }
        

        if (Time.time > _canFire)
        {
            if (_rayTriggeredFiring == false)
            {
                FireLasers(aimForward: true, hitInfo: forwardHit);
            }
        }
    }

    private void FireLasers(bool aimForward, RaycastHit2D hitInfo)
    {
        _fireRate = Random.Range(3.0f, 7.0f);
        _canFire = Time.time + _fireRate;

        GameObject enemyLaser;
        if (aimForward == true)
        {
            if (_fireLaserBeam == true)
            { 
                enemyLaser = null;
                Vector2 laserBeamEnd;

                if ( hitInfo == true && ( hitInfo.transform.tag == "PowerUp" || hitInfo.transform.tag == "Player") )
                {
                    laserBeamEnd = hitInfo.point;
                    if (hitInfo.transform.tag == "Player")
                    {
                        _player.transform.gameObject.GetComponent<Player>().Damage();
                    }
                } else
                {
                    laserBeamEnd = _laserBeamOrigin.transform.position - transform.up * 10f;
                }
                _lineRenderer.SetPosition(0, _laserBeamOrigin.transform.position);
                _lineRenderer.SetPosition(1, laserBeamEnd);
                _lineRenderer.gameObject.SetActive(true);
                StartCoroutine(ResetLineRenderer());
            } else
            {
                enemyLaser = Instantiate(_laserPrefab, transform.position, transform.rotation);
            }
        } else
        {
           if (_fireLaserBeam == true)
            {
                enemyLaser = null;
                if ( hitInfo == true && hitInfo.transform.tag == "Player")
                {
                    _lineRenderer.SetPosition(0, _rearFiringOrigin.transform.position);
                    _lineRenderer.SetPosition(1, hitInfo.point);
                    _lineRenderer.gameObject.SetActive(true);
                    StartCoroutine(ResetLineRenderer());
                }
            } else
            {
                enemyLaser = Instantiate(_laserPrefab, _rearFiringOrigin.transform.position, _rearFiringOrigin.transform.rotation);
            }
        }

        if (enemyLaser != null)
        {
            Laser[] lasers = enemyLaser.GetComponentsInChildren<Laser>();
            if (aimForward == true)
            {
                for (int i = 0; i < lasers.Length; i++)
                {
                    lasers[i].AssignEnemyLaser();
                }
            }
            else
            {
                for (int i = 0; i < lasers.Length; i++)
                {
                    lasers[i].AssignEnemyLaserUp();
                }
            }
        }
        
    }

    IEnumerator ResetLineRenderer()
    {
        yield return new WaitForSeconds(0.2f);
        if (_lineRenderer.gameObject.activeSelf == true)
        {
            _lineRenderer.gameObject.SetActive(false);
        }
    }

    private void CalculateMovement()
    {
        if (_moveOnAngle == true && _moveOnAngleTriggered == false)
        {
            float angle = Random.Range(_minAngle, _maxAngle);

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = rotation;
            _moveOnAngleTriggered = true;
        }

        _currentAngle = Vector2.Angle(transform.up, Vector2.right) - 90f;


        if (_isAvoidingLasers == true)
        {
            AvoidPlayerLaser();
        }

        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y < -6.0f || transform.position.x < -11.0f || transform.position.x > 11.0f)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Player":
                
                if (_player != null)
                {
                    _player.Damage();
                }

                if (_activateShield == true && _shieldCount >= 0)
                {
                    RemoveShield();
                } else
                {
                    _speed = 0;
                    _canFire = Time.time + _destroyClipLength + 1.0f;  //Turn off Laser firing until Enemy is destroyed
                    _anim.SetTrigger("OnEnemyDeath");
                    _audioSource.Play();
                    Destroy(GetComponent<Collider2D>());
                    Destroy(this.gameObject, _destroyClipLength);
                }
                break;

            case "PlayerLaser":
                if (_activateShield == true && _shieldCount >= 0)
                {
                    RemoveShield();
                } else
                {
                    Laser laserScript = other.GetComponent<Laser>();
                    if (laserScript == null)
                    {
                        Debug.LogError("Enemy: Unable to get Laser script.");
                    }
                    else
                    {
                        if (laserScript.IsEnemyLasers() == false)
                        {
                            Destroy(other.gameObject);
                            if (_shieldCount > 0)
                            {
                                RemoveShield();
                            }
                            else
                            {
                                LaserDestroysEnemy();
                            }
                        }
                    }
                }
                
                break;

            case "Mine":
                GameObject _explosiveMine = GameObject.Find("Explosive_Mine(Clone)");
                if (_explosiveMine == null)
                {
                    Debug.Log("Enemy: Unable to find Explosive_Mine");
                    return;
                }
                if (_player != null)
                {
                    _player.AddScore(10);
                }
                if (_shieldCount >= 0)
                {
                    RemoveShield();
                } else
                {
                    _speed = 0;
                    _canFire = Time.time + _destroyClipLength + 1.0f; //Turn off Laser firing until Enemy is destroyed
                    _explosiveMine.gameObject.GetComponent<ExplosiveMine>().Explode();
                    _anim.SetTrigger("OnEnemyDeath");
                    _audioSource.Play();
                    Destroy(GetComponent<Collider2D>());
                    Destroy(this.gameObject, _destroyClipLength);
                }
                
                break;
        }
    }

    public void LaserDestroysEnemy()
    {
        if (_player != null)
        {
            _player.AddScore(10);
        }

        _speed = 0;
        _canFire = Time.time + _destroyClipLength + 1.0f; //Turn off Laser firing until Enemy is destroyed
        _anim.SetTrigger("OnEnemyDeath");
        _audioSource.Play();
        Destroy(GetComponent<Collider2D>());
        Destroy(this.gameObject, _destroyClipLength);
    }

    public void RemoveShield()
    {
        _shieldCount--;
        if (_shieldCount <= 0)
        {
            _activateShield = false;
            _shieldVisualizer.SetActive(false);
        }
    }

    private void AvoidPlayerLaser()
    {
        GameObject closestLaser = FindClosestLaser();
        if (closestLaser == null)
        {
            return;
        } else
        {
            if (_foundPlayerLaser == false)
            {
                float angle = AvoidanceManuver(closestLaser: closestLaser);

                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = rotation;
                _foundPlayerLaser = true;
            }
            
        }
    }

    private GameObject FindClosestLaser()
    {
        GameObject[] playerLasers;
        playerLasers = GameObject.FindGameObjectsWithTag("PlayerLaser");
        if (playerLasers.Length == 0) //No enemies found
        {
            return null;
        }
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject player in playerLasers)
        {
            Vector3 diff = player.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance * distance)
            {
                closest = player;
                distance = curDistance;
            }
        }

        if (distance <= _avoidanceRange)
        {
            return closest;
        }
        else
        {
            return null;
        }
    }

    private float AvoidanceManuver(GameObject closestLaser)
    {
        // _currentAngle is the current motion of the Enemy measured in CalculateMovement method above.
        // 0 degrees in going in the straight down direction.
        // An angle to the left of straight down is positive, to the left negative

        Vector3 direction = closestLaser.transform.position - transform.position;
        float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90.0f;

        if (angle >= 0f)  // Laser to the right of Enemy as you look at the screen
        {
            if (_currentAngle < 30f)
            {
                angle = -30f;
            } else
            {
                angle = -_currentAngle;
            }
        } else  // Laser to the left of Enemy as you look at the screen
        {
            if (_currentAngle > -30f)
            {
                angle = 30f;
            } else
            {
                angle = -_currentAngle;
            }

        }
        _speed *= 2.0f;
        return angle;
    }
}

