using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;

public class PlayerScript_InputAction : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 3.5f;
    [SerializeField] private float _speedBoostMultiplier = 2.0f;
    [SerializeField] private float _fireRate = 0.15f;

    private float _canFire = -1.0f;
    private Vector2 m_move;
    private Player _player;
    private GameManager _gameManager;
    private UIManager _uIManager;

    private bool _shiftBoostOn = false;
    private bool _speedPowerUpOn = false;
    private float _shiftBoostCoolDownDeltaTime;
    private float _shiftBoostCoolDownDelta;
    private float _shiftBoostCoolDownDuration = 5.0f;
    private float _nextShiftCoolDownCalc = 0.0f;
    private float _shiftBoostCoolDownCalcPerSecond = 15.0f;
    private float _initialMoveSpeed;

    private void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.LogError("PlayerScript_InputAction: Unable to find Player script.");
        }

        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        if (_gameManager == null)
        {
            Debug.LogError("PlayerScript_InputAction: Unable to find GameManager script.");
        }

        _uIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (_uIManager == null)
        {
            Debug.LogError("PlayerScript_InputAction: Unable to find UIManager script.");
        }
        else
        {
            float maxDisplay = _moveSpeed * _speedBoostMultiplier;
            _uIManager.SetMaxThrusterHUD(maxDisplay);
            _uIManager.UpdateThrusterHUD(_moveSpeed);
        }

        _initialMoveSpeed = _moveSpeed;
        _shiftBoostCoolDownDeltaTime = 1.0f / _shiftBoostCoolDownCalcPerSecond;
        _shiftBoostCoolDownDelta = ((_initialMoveSpeed * _speedBoostMultiplier) - _initialMoveSpeed) / (_shiftBoostCoolDownDuration * _shiftBoostCoolDownCalcPerSecond);  
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        m_move = context.ReadValue<Vector2>();
    }

    private float lastClickTime = 0.0f;
    private float debounceDelay = 0.005f;

    public void OnFire(InputAction.CallbackContext context)
    {
        if ((Time.time - lastClickTime) < debounceDelay) // To remove the double trigger on press
        {
            return;
        }
        lastClickTime = Time.time;

        if (lastClickTime > _canFire)
        {
            _canFire = lastClickTime + _fireRate;
            if (_player != null)
            {
                _player.FireLaser();
            }
        }
    }

    public void OnRestart(InputAction.CallbackContext context)
    {
        if ((Time.time - lastClickTime) < debounceDelay) // To remove the double trigger on press
        {
            return;
        }
        lastClickTime = Time.time;

        if (lastClickTime > _canFire)
        {
            _canFire = lastClickTime + _fireRate;
            _gameManager.RestartGame();
        }
    }

    public void OnGameQuit(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ShiftKeyThrustBoost();
        } else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            ShiftKeyThrustDown();
        }

        if (_shiftBoostOn == true)
        {
            if (_speedPowerUpOn == false)
            {
                ShiftThrustCoolDown();
            }
        }
        Move(m_move);

    }


    private void Move(Vector2 direction)
    {
        if (_player != null)
        {
            if (direction.sqrMagnitude < 0.01)
                return;
            var scaledMoveSpeed = _moveSpeed * Time.deltaTime;
            var move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(direction.x, direction.y, 0);
            _player.transform.position += move * scaledMoveSpeed;
        }
    }

    public void SpeedPowerUp()
    {
        if (_speedPowerUpOn == false)
        {
            _moveSpeed = _initialMoveSpeed * _speedBoostMultiplier;
            _uIManager.UpdateThrusterHUD(_moveSpeed);
            _speedPowerUpOn = true;
            StartCoroutine(SpeedPowerDown());
        }
    }

    private IEnumerator SpeedPowerDown()
    {
        yield return new WaitForSeconds(5.0f);
        _moveSpeed = _initialMoveSpeed;
        _uIManager.UpdateThrusterHUD(_moveSpeed);
        _speedPowerUpOn = false;
    }

    private void ShiftKeyThrustBoost()
    {
        if (_speedPowerUpOn == false)
        {
            _moveSpeed *= _speedBoostMultiplier;
            _uIManager.UpdateThrusterHUD(_moveSpeed);
            _shiftBoostOn = true;
        }
    }

    private void ShiftKeyThrustDown()
    {
        if (_shiftBoostOn == true)
        {
            _moveSpeed = _initialMoveSpeed;
            _uIManager.UpdateThrusterHUD(_moveSpeed);
            _shiftBoostOn = false;
        }
    }

    private void ShiftThrustCoolDown()
    {
        if (Time.time > _nextShiftCoolDownCalc)
        {
            _nextShiftCoolDownCalc = Time.time + _shiftBoostCoolDownDeltaTime;
            _moveSpeed -= _shiftBoostCoolDownDelta;
            if (_moveSpeed <= _initialMoveSpeed)
            {
                ShiftKeyThrustDown();
            }
            _uIManager.UpdateThrusterHUD(_moveSpeed);
        }
    }
}
