using System;
using com.ajc.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, GameInputSystem.IMainPlayerActions
{
    #region Public

    

    #endregion
    
    #region Private
    
    /*[Flags]
    private enum PlayerState
    {
        Idle = 0,
        Moving = 1,
        Grounded = 2,
        Jumping = 4,
        DoubleJumping = 8,
    }*/
    
    [Flags]
    private enum PlayerState
    {
        Idle = 0,
        Moving = 1 << 0 & ~Idle,
        Grounded = 1 << 1 & ~Jumping & ~DoubleJumping,
        Jumping = 1 << 2,
        DoubleJumping = 1 << 3 | Jumping,
    }
    
    private PlayerState _playerState;
    private GameInputSystem _inputSystem;
    private Transform _playerTransform;
    private Vector2 _movementInput;
    private Rigidbody2D _playerRigidbody2D;
    private float _jumpTime = 0f;
    private float _maxJumpTime = 1f;
    
    [SerializeField] private float _runSpeed = 5; 
    [SerializeField] private float _jumpForce = 5;
    [SerializeField] private float _fallForce = 20;
    [SerializeField] private float _maxRunSpeed = 15;

    #endregion
    
    #region Unity Methods
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _jumpTime = 0f;
        _playerState = PlayerState.Idle;
        _playerTransform = transform;
        _playerRigidbody2D = GetComponentInChildren<Rigidbody2D>();
        
        _inputSystem = new GameInputSystem();
        _inputSystem.Enable();
        _inputSystem.MainPlayer.SetCallbacks(this);
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Make it so the state manager executes the methods depending on state
        // Jump should still be called when spacebar is pressed
        ManageStates();
        //Move(_movementInput);
        //Jump();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
        if (_movementInput.magnitude < _maxRunSpeed)
        {
            _playerState |= PlayerState.Moving;
            _playerState &= ~PlayerState.Idle;
        }
        //_playerState = (movementInput.magnitude > 0) ? (PlayerState)((int)_playerState >> (1)) : _playerState | PlayerState.Idle;
        //_playerState = (movementInput.magnitude > 0) ? ^PlayerState.Moving : PlayerState.Idle;
        if (Mathf.Approximately(_movementInput.magnitude, 0))
        {
            _playerState &= ~PlayerState.Moving;
            _playerState |= PlayerState.Idle;
        }
        /*else
        {
            _playerState &= ~PlayerState.Idle;
        }*/
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        
        //_playerState = !_playerRigidbody2D.IsTouchingLayers(LayerMask.GetMask("Ground")) ? _playerState | PlayerState.Jumping : _playerState ^ PlayerState.Jumping;
        //if (!IsEqualState(_playerState, PlayerState.Grounded)) return;
        if(context.performed)
        {
            _playerState |= PlayerState.Jumping;
            //_playerState &= ~PlayerState.Grounded;
            Debug.Log("Jumping");
            Jump();
        }
        //_playerState = (_playerState & PlayerState.Jumping) != 0 ? _playerState | PlayerState.Falling : _playerState & PlayerState.Falling;
    }
    
    #endregion

    #region Utils

    private bool IsEqualState(PlayerState state1, PlayerState state2)
    {
        int commonBitMask = (int)state1 & (int)state2;
        //return Enum.GetValues(typeof(PlayerState)).Cast<PlayerState>().Any(state => (commonBitMask & (int)state) != 0);
        bool isEqual = false;
        foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
        {
            if ((commonBitMask & (int)state) != 0)
            {
                isEqual = true;
                break;
            }
        }
        return isEqual;
    }
    private int GetEqualState(PlayerState state1, PlayerState state2)
    {
        int commonBitMask = (int)state1 & (int)state2;
        //return Enum.GetValues(typeof(PlayerState)).Cast<PlayerState>().Any(state => (commonBitMask & (int)state) != 0);
        foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
        {
            if ((commonBitMask & (int)state) != 0)
            {
                break;
            }
        }
        return commonBitMask;
    }

    private void ManageStates()
    {
        var binaryState = Convert.ToString((int)_playerState, 2);
        //Debug.Log($"(Magnitude {_movementInput.magnitude}) Current player state: {_playerState}, Binary state: {binaryState}");
        
        foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
        {
            switch (GetEqualState(_playerState, state))
            {
                case (int)PlayerState.Moving:
                    //_playerState &= ~PlayerState.Idle;
                    Move(_movementInput);
                    Debug.Log($"(Moving) Current player state: {_playerState}, Binary state: {binaryState}");
                    break;
                case (int)PlayerState.Idle:
                    //_playerState &= ~PlayerState.Moving;
                    Debug.Log($"(Idle) Current player state: {_playerState}, Binary state: {binaryState}");
                    break;
                case (int)PlayerState.Jumping: case (int)PlayerState.DoubleJumping:
                    _jumpTime += Time.deltaTime;
                    _playerState &= ~PlayerState.Grounded;
                    Jump();
                    Debug.Log($"(Jumping) Current player state: {_playerState}, Binary state: {binaryState}");
                    break;
                case (int)PlayerState.Grounded:
                    _jumpTime = 0;
                    _playerState &= ~PlayerState.Jumping;
                    //_playerState &= ~PlayerState.DoubleJumping;
                    Jump();
                    Debug.Log($"(Grounded) Current player state: {_playerState}, Binary state: {binaryState}");
                    break;
                default:
                    break;
            }
        }
        
        //_playerState = _playerRigidbody2D.IsTouchingLayers(LayerMask.GetMask("Ground")) ? _playerState | PlayerState.Grounded : _playerState | PlayerState.Jumping;
        if (_playerRigidbody2D.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            _playerState |= PlayerState.Grounded;
            //_playerState &= ~PlayerState.Jumping;
            //_playerState &= ~PlayerState.DoubleJumping;
        }

        /*if (IsEqualState(_playerState, PlayerState.Grounded))
        {
            _playerState ^= PlayerState.Jumping;
            _playerState ^= PlayerState.DoubleJumping;
        }

        if (IsEqualState(_playerState, PlayerState.Jumping) || IsEqualState(_playerState, PlayerState.DoubleJumping))
        {
            _jumpTime += Time.deltaTime;
            _playerState ^= PlayerState.Grounded;
        }*/

        /*switch (_playerState)
        {
            case (PlayerState.Grounded):
                _playerState ^= PlayerState.Jumping;
                _playerState ^= PlayerState.DoubleJumping;
                break;
            case (PlayerState.Jumping): case PlayerState.DoubleJumping:
                _jumpTime += Time.deltaTime;
                _playerState ^= PlayerState.Grounded;
                break;
            case PlayerState.Moving:
                _playerState ^= PlayerState.Idle;
                break;
            case PlayerState.Idle:
                _playerState ^= PlayerState.Moving;
                break;
            default:
                break;
        }*/
        
        //_playerState = (_playerState & PlayerState.Grounded) != 0 ? _playerState ^ PlayerState.Jumping : _playerState & PlayerState.Jumping;
    }

    private void Move(Vector2 movementInput)
    {

        if (IsEqualState(_playerState, PlayerState.Moving))
        {
            _playerRigidbody2D.AddForce(_playerTransform.right * (movementInput.x * _runSpeed));
        }
        /*if (movementInput.magnitude < _maxRunSpeed)
        {
            _playerRigidbody2D.AddForce(_playerTransform.right * (movementInput * _runSpeed));
            _playerState |= PlayerState.Moving;
        }
        //_playerState = (movementInput.magnitude > 0) ? (PlayerState)((int)_playerState >> (1)) : _playerState | PlayerState.Idle;
        //_playerState = (movementInput.magnitude > 0) ? ^PlayerState.Moving : PlayerState.Idle;
        if (Mathf.Approximately(movementInput.magnitude, 0))
        {
            _playerState ^= PlayerState.Moving;
        }
        else
        {
            _playerState ^= PlayerState.Idle;
        }*/
    }

    private void Jump()
    {
        
        /*if (IsEqualState(_playerState, PlayerState.Grounded))
        {
            _jumpTime = 0;
        }*/
        if (!IsEqualState(_playerState, PlayerState.Grounded) && _jumpTime >= _maxJumpTime)
        {
            //_jumpTime += Time.deltaTime;
            _playerRigidbody2D.AddForce(Vector2.down * (_fallForce /** Time.deltaTime*/), ForceMode2D.Impulse);
            _playerState |= PlayerState.Jumping;
            //_playerState |= PlayerState.Falling;
        }

        if (IsEqualState(_playerState, PlayerState.Jumping) && _jumpTime < _maxJumpTime)
        {
            //_jumpTime += Time.deltaTime;
            _playerRigidbody2D.AddForce(Vector2.up * (_jumpForce /** Time.deltaTime*/), ForceMode2D.Impulse);
            //_playerState &= ~PlayerState.Jumping;
            //_playerState |= PlayerState.Falling;
        }
        //_jumpTime += Time.deltaTime;
        /*else
        {
            _playerState ^= PlayerState.Jumping;
        }*/
        /*if (_playerRigidbody2D.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            _playerState |= PlayerState.Grounded;
            _playerState &= ~PlayerState.Jumping;
            _playerState &= ~PlayerState.DoubleJumping;
            return;
        }*/
        
        // var binaryState = Convert.ToString((int)_playerState, 2);
        // Debug.Log($"Current player state: {_playerState}, Binary state: {binaryState}");
    }

    #endregion
}
