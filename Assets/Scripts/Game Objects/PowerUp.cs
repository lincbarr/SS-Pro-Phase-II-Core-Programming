using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private float _speed = 3.0f;
    [SerializeField] private int _powerUpID; // 0 = TripleShot, 1 = PowerUp, 2 = Shields, 3 = Ammo, 4 = Health, 5 = Space Mine, 6 = Homing Projectile, 7 = Negative
    [SerializeField] private bool _seekPlayer = false;
    [SerializeField] private GameObject _explosionEffect;

    private GameManager _gameManager;
    private AudioSource _audioSource;
    private AudioSource _destroyedSound;
    private GameObject _player;
    
    private void Start()
    {
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        if (_gameManager == null)
        {
            Debug.LogError("PowerUp: Unable to find GameManager script");
        }

        _audioSource = GameObject.Find("Audio_Manager").transform.GetChild(1).GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            Debug.LogError("PowerUp: Unable to find AudioSource component of PowerUp object.");
        }

        _destroyedSound = GameObject.Find("Audio_Manager").transform.GetChild(2).GetComponent<AudioSource>();
        if (_destroyedSound == null)
        {
            Debug.LogError("PowerUp: Unable to find PowerUpDestroyed sound.");
        }
            

        _player = GameObject.Find("Player");
        if (_player == null)
        {
            Debug.LogError("PowerUp: Unable to find Player object.");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            _seekPlayer = true;
        }
        float normalizedSpeed = _speed * Time.deltaTime;

        if (_seekPlayer == false)
        {
            transform.Translate(Vector3.down * normalizedSpeed);
        } else
        {
            transform.position = Vector3.MoveTowards(transform.position, _player.transform.position, normalizedSpeed * 2f);
            Vector3 direction = _player.transform.position - transform.position;
            float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90.0f;

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, normalizedSpeed * 2f);
        }

        if (transform.position.y < -6.0f)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
                _audioSource.Play();

                switch (_powerUpID)
                {
                    case 0:
                        if (player.GetLaserInventory() >=3 )
                        {
                            player.TripleShotActive();
                        }
                        break;

                    case 1:
                        PlayerScript_InputAction inputAction = _gameManager.GetComponent<PlayerScript_InputAction>();
                        inputAction.SpeedPowerUp();
                        break;

                    case 2:
                        player.ActivateShields();
                        break;

                    case 3: // Ammo
                        player.ReloadLaserInventory();
                        break;

                    case 4: // Health
                        player.AddHealth();
                        break;

                    case 5:  // Homing Projectile
                        player.ActivateHomingProjectile();
                        break;

                    case 6:  // Negative
                        player.Damage();
                        break;

                    case 7: // Space Mine
                        player.LoadMine();
                        break;

                    default:
                        Debug.LogError("Unknown PowerUpID");
                        break;
                }
            }

            Destroy(this.gameObject);
        }
    }

    public void DestroyPowerUp() 
    {
        float audioClipLength = _destroyedSound.clip.length;
        _destroyedSound.Play();
       Instantiate(_explosionEffect, transform.position, Quaternion.identity);
        this.gameObject.SetActive(false);
        Destroy(this.gameObject, audioClipLength);
    }
}
