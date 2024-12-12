using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _forwardSpeed = 5f; // Initial speed of forward movement
    private float _laneChangeSpeed = 7f; // Speed of lane changing
    private bool _canMove = false;
    private string _obstacleTag = "Obstacle";
    private float _speedScale = 1;

    private float _lastZPosition;
    private float _speedIncreaseInterval = 100f;
    private float _speedIncreasePercentage = 0.2f;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _jumpDuration = 0.5f;
    [SerializeField] private LayerMask _groundLayer;
    private bool _isGrounded;
    private int _jumpAnimation = Animator.StringToHash("Jump");

    [Header("Roll Settings")]
    [SerializeField] private float _rollDuration = 0.5f;
    private string _isRolling = "isRolling";

    [Header("Capsule Collider Settings")]
    private CapsuleCollider _capsuleCollider;
    private float _originalHeight;
    private Vector3 _originalCenter;

    [Header("Lane Settings")]
    private int _currentLane = 1;
    private readonly float[] _lanes = { LevelBoundary._leftSide, LevelBoundary._center, LevelBoundary._rightSide };
    private Vector3 _targetPosition;
    
    [Header("Components")]
    private Rigidbody _rb;
    private Animator _animator;
    [SerializeField] private GameObject _player;
    [SerializeField] private ScoreCounter _scoreCounter;
    [SerializeField] private UIManager _uiManager;
    private Vector2 _lastSwipeDirection = Vector2.zero;
    private bool _actionPerformed = false;

    private Vector2 _startTouchPosition, _endTouchPosition, _currentSwipe;
    private float _minSwipeDistance = 30f;
    private int _roll = Animator.StringToHash("Roll");


    private void Awake()
    {
        _targetPosition = transform.position;
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        
        if (_capsuleCollider != null)
        {
            _originalHeight = _capsuleCollider.height;
            _originalCenter = _capsuleCollider.center;
        }
        
        _animator = _player.GetComponentInChildren<Animator>();

        if (_animator == null)
            Debug.LogWarning("Animator component not found. Ensure it is attached to a child object.");

        _lastZPosition = transform.position.z;
    }

    private void Update()
    {
        if (_canMove)
        {
            transform.Translate(Vector3.forward * _forwardSpeed * _speedScale * Time.deltaTime);

            if (transform.position.z >= _lastZPosition + _speedIncreaseInterval)
            {
                _speedScale += _speedIncreasePercentage;
                _lastZPosition = transform.position.z;
                _animator.speed = 1 + _speedScale / 10;
            }
            DetectSwipe();
            Vector3 desiredPosition = new Vector3(_targetPosition.x, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * _laneChangeSpeed);

            if (_scoreCounter.isActiveAndEnabled)
            {
                _scoreCounter.Score = (int)transform.position.z + 10;
            }
        }
    }

    private void DetectSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                _startTouchPosition = touch.position;
                _actionPerformed = false;
            }
            else if (touch.phase == TouchPhase.Moved && !_actionPerformed)
            {
                _endTouchPosition = touch.position;
                if (CheckSwipe())
                {
                    _actionPerformed = true;
                }
            }
        }
    }

    private bool CheckSwipe()
    {
        float distance = Vector2.Distance(_startTouchPosition, _endTouchPosition);
        if (distance >= _minSwipeDistance)
        {
            Vector2 direction = _endTouchPosition - _startTouchPosition;
            Vector2 normalizedDirection = direction.normalized;

            if (Mathf.Abs(normalizedDirection.x) > Mathf.Abs(normalizedDirection.y))
            {
                if (normalizedDirection.x > 0)
                {
                    MoveRight();
                }
                else
                {
                    MoveLeft();
                }
            }
            else
            {
                if (normalizedDirection.y > 0)
                {
                    Jump();
                }
                else
                {
                    Slide();
                }
            }

            return true;
        }

        return false;
    }

    private void Jump()
    {
        if (_isGrounded)
        {
            _animator.SetTrigger(_jumpAnimation); 
            Debug.Log("Jumping...");
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            AdjustCapsuleCollider(height: _originalHeight * 0.5f, center: new Vector3(_originalCenter.x, _originalCenter.y + 0.5f, _originalCenter.z));

            Invoke(nameof(ResetCapsuleCollider), _jumpDuration);
        }
    }

    private void Slide()
    {
        _animator.SetTrigger(_roll);
        _animator.SetBool(_isRolling, true);

        AdjustCapsuleCollider(height: _originalHeight * 0.5f, center: new Vector3(_originalCenter.x, _originalCenter.y - 0.5f, _originalCenter.z));
        
        _animator.SetBool(_isRolling, false);
        
        Invoke(nameof(ResetCapsuleCollider), _rollDuration);
    }

    private void FixedUpdate()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        float rayLength = (_capsuleCollider.height / 2f) + 1.5f;
        _isGrounded = Physics.Raycast(rayOrigin, Vector3.down, rayLength, _groundLayer);

        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, _isGrounded ? Color.green : Color.red);
    }

    private void MoveLeft()
    {
        if (_currentLane > 0)
        {
            _currentLane--;
            _targetPosition.x = _lanes[_currentLane];
        }
    }

    private void MoveRight()
    {
        if (_currentLane < _lanes.Count() - 1)
        {
            _currentLane++;
            _targetPosition.x = _lanes[_currentLane];
        }
    }
    private void AdjustCapsuleCollider(float height, Vector3 center)
    {
        if (_capsuleCollider != null)
        {
            _capsuleCollider.height = height;
            _capsuleCollider.center = center;
        }
    }

    private void ResetCapsuleCollider()
    {
        if (_capsuleCollider != null)
        {
            _capsuleCollider.height = _originalHeight;
            _capsuleCollider.center = _originalCenter;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_obstacleTag))
        {
            Debug.Log("trigger");
            _animator.SetTrigger("hit");
            _canMove = false;
            _uiManager.Hit();
            StartCoroutine(FirestoreManager.Instance.UpdateHighScore(_scoreCounter.Score));
            other.gameObject.SetActive(false);
        }
    }

    public void StartMove()
    {
        _animator.SetTrigger("start");
        _canMove = true;
    }
    public void Continue()
    {
        _animator.SetTrigger("start");
        _canMove = true;
    }
}
