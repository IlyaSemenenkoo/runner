using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    private float _forwardSpeed = 5f; // Speed of forward movement
    private float _laneChangeSpeed = 7f; // Speed of lane changing

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 5f; // Jump force
    [SerializeField] private LayerMask _groundLayer; // Layer for ground detection
    private bool _isGrounded; // Is the player on the ground

    [Header("Slide Settings")]
    private bool _isSlide;

    [Header("Lane Settings")]
    private int _currentLane = 1; // Current lane: 0 = left, 1 = center, 2 = right
    private float[] _lanes = { LevelBoundary._leftSide, LevelBoundary._center, LevelBoundary._rightSide }; // Coordinates of the lanes on the X-axis
    private Vector3 _targetPosition; // Target position for lane changing

    [Header("Components")]
    private Rigidbody _rb;
    private Animator _animator;
    [SerializeField] private GameObject player;

    private void Start()
    {
        _targetPosition = transform.position;
        _rb = GetComponent<Rigidbody>();

        // Ищем компонент Animator на вложенных объектах
        _animator = player.GetComponentInChildren<Animator>();

        if (_animator == null)
            Debug.LogWarning("Animator component not found. Ensure it is attached to a child object.");
    }


    private void Update()
    {
        // Continuous forward movement
        transform.Translate(Vector3.forward * _forwardSpeed * Time.deltaTime);

        // Input handling
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Slide();
            _isSlide = false;
        }

        // Smooth movement between lanes
        Vector3 desiredPosition = new Vector3(_targetPosition.x, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * _laneChangeSpeed);
    }

    private void FixedUpdate()
    {
        // Ground check using a raycast
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, _groundLayer);
        _animator.SetBool("IsGrounded", _isGrounded);
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

    private void Jump()
    {
        if (_isGrounded)
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _animator.SetTrigger("Jump");
        }
    }

    private void Slide()
    {
        if (!_isSlide)
        {
            _animator.SetTrigger("Slide");
            _isSlide = true;
        }
    }
}
