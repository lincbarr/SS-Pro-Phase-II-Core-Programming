using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [SerializeField] private GameObject _laser;
    [SerializeField] private AudioClip _explosionAudioClip;

    private RotateObjectTowardsPlayer _rotateScript;
    private GameManager _gameManager;
    private UIManager _uiManager;

    private Player _player;
    private bool _seekPlayer;

    private Animator _anim;
    private AudioSource _audioSource;
    private float _destroyClipLength;

    private bool _startedRotation = false;
    private bool _fire = false;

    //Hits
    private int _hitCount = 0;
    private int _maxHits = 10;

    // Sphere
    private GameObject _sphere;
    private Renderer _sphereRenderer;

    private float _speed = 3.0f;
    private float _fireRate = 3.0f;
    private float _canFire;

    // Laser Origins
    private Transform _forwardRightGunLocation;
    private Transform _forwardLeftGunLocation;
    private Transform _middleRightGunLocation;
    private Transform _middleLeftGunLocation;
    private Transform _rearRightGunLocation;
    private Transform _rearLeftGunLocation;

    private Transform[] _positions;

    private void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.LogError("Enemy: Unable to find Player script.");
        }

        _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (_uiManager == null)
        {
            Debug.LogError("Player: Unable to find UIManager script.");
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

        _positions = new Transform[6];

        _forwardRightGunLocation = transform.GetChild(0).GetChild(0).transform;
        if (_forwardRightGunLocation == null)
        {
            Debug.LogError("BossEnemy: Unable to find Forward_Right_Gun_Location transform.");
        } else
        {
            _positions[0] = _forwardRightGunLocation;
        }

        _forwardLeftGunLocation = transform.GetChild(0).GetChild(1).transform;
        if (_forwardRightGunLocation == null)
        {
            Debug.LogError("BossEnemy: Unable to find Forward_Left_Gun_Location transform.");
        } else
        {
            _positions[1] = _forwardLeftGunLocation;
        }

        _middleRightGunLocation = transform.GetChild(0).GetChild(2).transform;
        if (_forwardRightGunLocation == null)
        {
            Debug.LogError("BossEnemy: Unable to find Middle_Right_Gun_Location transform.");
        } else
        {
            _positions[2] = _middleRightGunLocation;
        }

        _middleLeftGunLocation = transform.GetChild(0).GetChild(3).transform;
        if (_forwardRightGunLocation == null)
        {
            Debug.LogError("BossEnemy: Unable to find Middle_Left_Gun_Location transform.");
        } else
        {
            _positions[3] = _middleLeftGunLocation;
        }

        _rearRightGunLocation = transform.GetChild(0).GetChild(4).transform;
        if (_forwardRightGunLocation == null)
        {
            Debug.LogError("BossEnemy: Unable to find Rear_Right_Gun_Location transform.");
        } else
        {
            _positions[4] = _rearRightGunLocation;
        }

        _rearLeftGunLocation = transform.GetChild(0).GetChild(5).transform;
        if (_forwardRightGunLocation == null)
        {
            Debug.LogError("BossEnemy: Unable to find Rear_Left_Gun_Location transform.");
        } else
        {
            _positions[5] = _rearLeftGunLocation;
        }

        _gameManager = GameObject.Find("Game_Manager").transform.GetComponent<GameManager>();

        _sphere = transform.GetChild(1).gameObject;
        if (_sphere == null)
        {
            Debug.LogError("BossEnemy: Unable to find sphere.");
        } else
        {
            _sphereRenderer = _sphere.GetComponent<Renderer>();
            if (_sphereRenderer == null)
            {
                Debug.LogError("BossEnemy: Unable to find Renderer on Sphere.");
            } else
            {
                AdjustSphereColor();
            }
        }
        


        _rotateScript = GetComponent<RotateObjectTowardsPlayer>();

        _canFire = Time.time + _fireRate;
    }

    float _priorAngle = 0f;

    // Update is called once per frame
    void FixedUpdate()
    {
        CalculateMovement();

        if (_startedRotation == true)
        {
            //FireAllLasers();
        }

        if (_speed == 0f && _startedRotation == false)
        {
            _rotateScript.StartRotation();
            _startedRotation = true;
        }

        float diff = 10f;
        float angle = 0f;

        if (_player != null)
        {
            Vector2 direction = _player.transform.position - transform.position;

            direction = _player.transform.InverseTransformDirection(direction);

            angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            diff = Mathf.Abs(angle - _priorAngle);
        }
        
        if (Mathf.Abs(diff) == 0f)
        {
            FireAllLasers();
            _priorAngle = angle;
        } else
        {
            _priorAngle = angle;
            _fire = true;
        }
    }

    private void CalculateMovement()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y <= 1.0f)
        {
            _speed = 0.0f;
        }
    }

    private void FireAllLasers()
    {
        if (_fire == true && Time.time >= _canFire && _gameManager.IsGameOver() == false)
        {
            _canFire = Time.time + _fireRate;
            Quaternion fireRotation = _forwardRightGunLocation.rotation;

            foreach (Transform pos in _positions)
            {
                GameObject enemyLaser = Instantiate(_laser, pos.position, fireRotation);
                enemyLaser.GetComponent<Laser>().AssignEnemyLaser();
            }

            _fire = false;
            
        }
    }

    public void AdjustSphereColor()
    {
        float red = _hitCount >= (_maxHits / 2.0f) ? 1.0f : _hitCount / (_maxHits / 2.0f);
        float green = _hitCount >= (_maxHits / 2.0f) ? (_maxHits - _hitCount) / (_maxHits / 2.0f) : 1.0f;
        Color sphereColor = new Color(red, green, 0.0f, 1.0f);

        MaterialPropertyBlock props = new MaterialPropertyBlock();
        props.SetColor("_Color", sphereColor);
        _sphereRenderer.SetPropertyBlock(props);
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

                _hitCount++;
                AdjustSphereColor();

                if (_hitCount >= _maxHits )
                {
                    DestroyBoss();
                }
                break;

            case "PlayerLaser":

                Laser laserScript = other.GetComponent<Laser>();
                if (laserScript == null)
                {
                    Debug.LogError("BossEnemy: Unable to get Laser script.");
                }
                else
                {
                    if (laserScript.IsEnemyLasers() == false)
                    {
                        _hitCount++;
                        AdjustSphereColor();
                        
                        if (_hitCount >= _maxHits)
                        {
                            DestroyBoss();
                        }
                    }
                }

                break;

            case "Mine":

                GameObject _explosiveMine = GameObject.Find("Explosive_Mine(Clone)");
                if (_explosiveMine == null)
                {
                    Debug.Log("BossEnemy: Unable to find Explosive_Mine");
                    return;
                }
                if (_player != null)
                {
                    _player.AddScore(10);
                }

                _hitCount += 4;
                AdjustSphereColor();

                if (_hitCount >= _maxHits)
                {
                    _explosiveMine.gameObject.GetComponent<ExplosiveMine>().Explode();

                    DestroyBoss();
                }

                break;
        }
    }

    public void DestroyBoss()
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
        _sphere.SetActive(false);
        Destroy(this.gameObject, _destroyClipLength);
        _uiManager.ShowYouWonText();
        _uiManager.UpdateLives(0);
    }
}
