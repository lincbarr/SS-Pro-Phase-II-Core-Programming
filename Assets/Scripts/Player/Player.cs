using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{ 
    [SerializeField] private int _lives = 3;
    [SerializeField] private int _maxLasers = 15;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private GameObject _tripleShotPrefab;
    [SerializeField] private GameObject _shieldVisualizer;
    [SerializeField] private GameObject _explosiveMinePrefab;
    [SerializeField] private GameObject _rightEngine, _leftEngine;
    [SerializeField] private AudioClip _laserSoundClip;
    [SerializeField] private AudioClip _powerDownClip;
    [SerializeField] private GameObject _explosiveMine;

    private AudioSource _audioSource;
    private GameObject _thruster;
    
    private int _score;
    private int _laserInventory;

    private UIManager _uiManager;
    private CameraShake _cameraShake;

    // Triple Shot
    private bool _isTripleShotActive = false;
    private bool _homingLaser = false;

    // Shields
    private int _shieldCount = 0;
    private int _maxShieldHits = 3;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 0, 0);
        _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        _audioSource = GetComponent<AudioSource>();

        if (_uiManager == null)
        {
            Debug.LogError("Player: Unable to find UIManager script.");
        }

        if (_audioSource == null)
        {
            Debug.LogError("Player: Unable to find AudioSource");
        } 

        _cameraShake = GameObject.Find("Camera_Parent").GetComponent<CameraShake>();
        if (_cameraShake == null)
        {
            Debug.LogError("UIManager: Unable to find CameraShake script.");
        }

        _thruster = GameObject.Find("Thruster");
        if (_thruster == null)
        {
            Debug.LogError("Player: Unable to find Thruster object.");
        }

        _rightEngine.SetActive(false);
        _leftEngine.SetActive(false);
        _explosiveMine.SetActive(false);

        ReloadLaserInventory();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CalculateMovement();
    }

    private void CalculateMovement()
    {
        transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, -4.0f, 2.0f));

        if (transform.position.x >= 11.4f)
        {
            transform.position = new Vector3(-11.4f, transform.position.y, 0);
        }
        else if (transform.position.x <= -11.4f)
        {
            transform.position = new Vector3(11.4f, transform.position.y, 0);
        }
    }

    public void Damage()
    { 
        if (_shieldCount > 0)
        {
            _shieldCount--;

            if (_shieldCount < 1)
            {
                _shieldVisualizer.SetActive(false);
                _shieldCount = 0;
            }

            _uiManager.UpdateShields(_shieldCount);
            return;
        }

        _lives--;

        
        SetEngines(_lives);
        if (_lives >= 1)
        {
            _audioSource.clip = _powerDownClip;
            _audioSource.Play();
        }
        _cameraShake.ShakeCamera(1.0f, 0.5f);
        _uiManager.UpdateLives(_lives);

    }

    public void SetEngines(int lives)
    {
        switch (lives)
        {
            case 0:
                _leftEngine.SetActive(false);
                _rightEngine.SetActive(false);
                _thruster.SetActive(false);
                break;

            case 1:
                _leftEngine.SetActive(true);
                _rightEngine.SetActive(true);
                break;

            case 2:
                _leftEngine.SetActive(true);
                _rightEngine.SetActive(false);
                break;

            case 3:
                _leftEngine.SetActive(false);
                _rightEngine.SetActive(false);
                break;
        }
    }

    public void FireLaser()
    {
        if (_explosiveMine.activeSelf == true)
        {
            PlaceMine();
        } else if (_laserInventory > 0)
        {
            if (_isTripleShotActive == true && _laserInventory > 2)
            {
                GameObject tripleLasers =  Instantiate(_tripleShotPrefab, transform.position, Quaternion.identity);
                if (_homingLaser == true && tripleLasers != null)
                {
                    foreach(Transform laser in tripleLasers.transform)
                    {
                        Laser laserScript = laser.GetComponent<Laser>();
                        if (laserScript == null)
                        {
                            Debug.LogError("Player: Unable to find Laser script.");
                        }
                        else
                        {
                            laserScript.ActivateHoming();
                        }
                    }
                }
                _laserInventory -= 3;
            }
            else
            {
                GameObject laser = Instantiate(_laserPrefab, transform.position + new Vector3(0, 1.05f, 0), Quaternion.identity);
                if (_homingLaser == true && laser != null)
                {
                    Laser laserScript = laser.GetComponent<Laser>();
                    if (laserScript == null)
                    {
                        Debug.LogError("Player: Unable to find Laser script.");
                    }
                    else
                    {
                        laserScript.ActivateHoming();
                    }
                }
                _laserInventory--;
            }

            _uiManager.UpdateLaserInventory(_laserInventory);
            _audioSource.clip = _laserSoundClip;
            _audioSource.Play();
        }
        
    }

    public void TripleShotActive()
    {
        _isTripleShotActive = true;
        StartCoroutine(TripleShotPowerDownRoutine());
    }

    private IEnumerator TripleShotPowerDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isTripleShotActive = false;
    }

    public void ActivateShields()
    {
        _shieldCount = _maxShieldHits;
        _uiManager.UpdateShields(_shieldCount);
        _shieldVisualizer.SetActive(true);
    }

    public void AddScore(int points)
    {
        _score += points;
        _uiManager.UpdateScore(_score);
    }

    public void UpdateShieldHits()
    {
        _uiManager.UpdateShields(_shieldCount);
    }

    public void AddHealth()
    {
        if (_lives < 3)
        {
            _lives++;
            _uiManager.UpdateLives(_lives);
            SetEngines(_lives);
        }
    }

    public void ActivateHomingProjectile()
    {
        _homingLaser = true;
        StartCoroutine(DeactivateHomingProjectile());
    }

    private IEnumerator DeactivateHomingProjectile()
    {
        yield return new WaitForSeconds(5.0f);
        _homingLaser = false;
    }

    public int GetLaserInventory()
    {
        return _laserInventory;
    }

    public int GetMaxLasers()
    {
        return _maxLasers;
    }
       

    public void ReloadLaserInventory()
    {
        _laserInventory = _maxLasers;
        _uiManager.UpdateLaserInventory(_laserInventory);
    }

    public void LoadMine()
    {
        _explosiveMine.SetActive(true);
        _uiManager.UpdateLaserInventory(_laserInventory);
    }

    public void RemoveMine()
    {
        _explosiveMine.SetActive(false);
        _uiManager.UpdateLaserInventory(_laserInventory);
    }

    public bool IsMineLoaded()
    {
        return _explosiveMine.activeSelf;
    }

    private void PlaceMine()
    {
        Instantiate(_explosiveMinePrefab, _explosiveMine.gameObject.transform.position, Quaternion.identity);
        RemoveMine();
    }

}
