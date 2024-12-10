using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    private float _forwardSpeed = 5f; // Initial speed of forward movement
    private float _laneChangeSpeed = 7f; // Speed of lane changing
    private bool _canMove = false;
    private string _obstacleTag = "Obstacle";
    private float _speedScale = 1;

    private float _lastZPosition; // To track the last position of Z
    private float _speedIncreaseInterval = 100f; // Every 100 units, increase speed
    private float _speedIncreasePercentage = 0.2f; // Speed increase percentage (10%)

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 5f; // Jump force
    [SerializeField] private LayerMask _groundLayer; // Layer for ground detection
    private bool _isGrounded; // Is the player on the ground
    private bool _isFirst = false;

    [Header("Roll Settings")]
    [SerializeField] private float _rollDuration = 0.5f; // Duration of the roll
    private bool _isRolling; // Is the player rolling

    [Header("Capsule Collider Settings")]
    private CapsuleCollider _capsuleCollider;
    private float _originalHeight;
    private Vector3 _originalCenter;

    [Header("Lane Settings")]
    private int _currentLane = 1; // Current lane: 0 = left, 1 = center, 2 = right
    private readonly float[] _lanes = { LevelBoundary._leftSide, LevelBoundary._center, LevelBoundary._rightSide }; // Coordinates of the lanes on the X-axis
    private Vector3 _targetPosition; // Target position for lane changing

    [Header("Components")]
    private Rigidbody _rb;
    private Animator _animator;
    [SerializeField] private GameObject player;
    [SerializeField] private ScoreCounter _scoreCounter;
    [SerializeField] private FirestoreManager _fireStore;
    [SerializeField] private UIManager _uiManager;

    private Vector2 _startTouchPosition, _endTouchPosition, _currentSwipe; // Start position of touch for swipe detection
    private float _minSwipeDistance = 50f;

    private void Awake()
    {
        _targetPosition = transform.position;
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        // Save the initial Capsule Collider parameters
        if (_capsuleCollider != null)
        {
            _originalHeight = _capsuleCollider.height;
            _originalCenter = _capsuleCollider.center;
        }

        // Find the Animator component on child objects
        _animator = player.GetComponentInChildren<Animator>();

        if (_animator == null)
            Debug.LogWarning("Animator component not found. Ensure it is attached to a child object.");

        _lastZPosition = transform.position.z; // Initialize the last Z position
    }

    private void Update()
    {
        if (_canMove)
        {
            // Continuous forward movement
            transform.Translate(Vector3.forward * _forwardSpeed * _speedScale * Time.deltaTime);

            // Increase speed if player has traveled more than 100 units on the Z-axis
            if (transform.position.z >= _lastZPosition + _speedIncreaseInterval)
            {
                _speedScale += _speedIncreasePercentage; // Increase speed by 10%
                _lastZPosition = transform.position.z; // Update the last position
                _animator.speed = 1 + _speedScale / 10;
            }

            // Mobile touch controls
            DetectSwipe();

            // Smooth movement between lanes
            Vector3 desiredPosition = new Vector3(_targetPosition.x, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * _laneChangeSpeed);

            // Update score
            if (_scoreCounter.isActiveAndEnabled)
            {
                _scoreCounter.Score = (int)transform.position.z + 10;
            }
        }
    }

    private void DetectSwipe()
    {
        // Для сенсорных устройств
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                _startTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                _endTouchPosition = touch.position;
                CheckSwipe();
            }
        }
    }

    private void CheckSwipe()
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
                    Debug.Log("Swipe Right");
                    MoveRight();
                }
                else
                {
                    Debug.Log("Swipe Left");
                    MoveLeft();
                }
            }
            else
            {
                if (normalizedDirection.y > 0)
                {
                    Debug.Log("Swipe Up");
                    Jump();
                }
                else
                {
                    Debug.Log("Swipe Down");
                    Slide();
                }
            }
        }
    }

    private void Jump()
    {
        if (_isGrounded && _isFirst)
        {
            Debug.Log("Jumping...");
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z); // Reset vertical velocity
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);      // Apply upward impulse
            _animator.SetTrigger("Jump");                                 // Trigger jump animation
        }
        else
        {
            _isFirst = true;
            Debug.Log("Cannot jump, not grounded.");
        }
    }

    private void Slide()
    {
        if (!_isRolling )
        {

            _isRolling = true;
            _animator.SetTrigger("Roll");

            // Reduce Capsule Collider size for the roll
            AdjustCapsuleCollider(height: _originalHeight * 0.5f, center: new Vector3(_originalCenter.x, _originalCenter.y - 0.5f, _originalCenter.z));

            // Return to the original values after the roll
            Invoke(nameof(ResetCapsuleCollider), _rollDuration);
            _isRolling = false;
        }
    }

    private void FixedUpdate()
    {
        // Ground check setup
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f; // Slightly above the center
        float rayLength = (_capsuleCollider.height / 2f) + 0.5f;    // Ray length with a margin
        _isGrounded = Physics.Raycast(rayOrigin, Vector3.down, rayLength, _groundLayer);

        // Debug ray
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
        if (_currentLane < _lanes.Length - 1)
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
            StartCoroutine(_fireStore.UpdateHighScore(_scoreCounter.Score));
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
