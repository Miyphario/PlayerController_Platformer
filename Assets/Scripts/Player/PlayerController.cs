using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Walk")]
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 7f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 700f;
    [SerializeField] private float _faceRightRotation = 0f;
    [SerializeField] private float _faceLeftRotation = 180f;

    [Header("Crouch")]
    [SerializeField] private float _crouchColliderHeight = 1.4f;
    [SerializeField] private float _crouchSpeed = 3f;

    [Header("Jump")]
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _jumpStrength = 10f;
    [SerializeField] private float _wallJumpStrength = 14f;

    [Header("Other")]
    [SerializeField] private Transform _model;

    private bool _isGrounded;
    private bool _isSprinting;
    private bool _isCrouching;
    private bool _isJumping;
    private bool _crouchKeyPressed;

    private float _defaultColliderHeight;
    private Vector3 _moveDirection;
    private bool _faceRight = true;

    private bool _onWall;
    private float _onWallTime;
    private float _maxOnWallTime = 0.35f;

    private Rigidbody _rb;
    private CapsuleCollider _collider;

    private float _groundCheckRadius = 0.2f;
    private Collider[] _overlapGround = new Collider[1];

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();

        _defaultColliderHeight = _collider.height;
    }

    private void Update()
    {
        CheckGroundUpdate();
        InputUpdate();
        RotationUpdate();
        CheckWallUpdate();
        WallTimeUpdate();
    }

    private void FixedUpdate()
    {
        MoveFixedUpdate();
        JumpFixedUpdate();
    }

    private void InputUpdate()
    {
        // Movement
        if (_onWallTime <= 0f)
            _moveDirection = new(Input.GetAxisRaw("Horizontal"), 0f, 0f);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _isJumping = true;
        }

        // Sprint
        if (!_isCrouching)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                _isSprinting = true;
            }
            else
            {
                _isSprinting = false;
            }
        }

        // Crouch
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _crouchKeyPressed = true;
            Crouch();
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            _crouchKeyPressed = false;
            Uncrouch();
        }

        if (!_crouchKeyPressed)
        {
            Uncrouch();
        }
    }

    private void RotationUpdate()
    {
        if (_moveDirection == Vector3.zero) return;

        Vector3 rotation = transform.rotation.eulerAngles;

        if (_moveDirection.x < 0f)
        {
            rotation.y = _faceLeftRotation;
            _faceRight = false;
        }
        else
        {
            rotation.y = _faceRightRotation;
            _faceRight = true;
        }

        Quaternion toRotation = Quaternion.Euler(rotation);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, _rotationSpeed * Time.deltaTime);
    }

    private void CheckGroundUpdate()
    {
        if (Physics.OverlapSphereNonAlloc(transform.position, _groundCheckRadius, _overlapGround, _groundMask) > 0)
            _isGrounded = true;
        else
            _isGrounded = false;
    }

    private void MoveFixedUpdate()
    {
        float currentSpeed;
        if (_isCrouching)
            currentSpeed = _crouchSpeed;
        else if (_isSprinting)
            currentSpeed = _sprintSpeed;
        else
            currentSpeed = _walkSpeed;

        Vector3 velocity = _rb.velocity;
        if (_onWallTime <= 0f)
        {
            velocity = new(_moveDirection.x * currentSpeed, _rb.velocity.y, _rb.velocity.z);

            if (_moveDirection != Vector3.zero)
            {
                if (_onWall)
                {
                    velocity.y = -1f;
                }
            }
        }

        _rb.velocity = velocity;
    }

    private void JumpFixedUpdate()
    {
        if (!_isJumping) return;

        Vector3 forceVelocity;
        if (_onWall)
        {
            Vector3 horizontalDirection;
            if (_faceRight)
            {
                horizontalDirection = -Vector3.right;
                _moveDirection.x = -1f;
            }
            else
            {
                horizontalDirection = Vector3.right;
                _moveDirection.x = 1f;
            }

            _onWallTime = _maxOnWallTime;
            _rb.velocity = Vector3.zero;
            forceVelocity = transform.up * _wallJumpStrength;
            forceVelocity += horizontalDirection * (_jumpStrength / 1.5f);
        }
        else
        {
            if (!_isGrounded)
            {
                _isJumping = false;
                return;
            }

            forceVelocity = transform.up * _jumpStrength;
        }

        _rb.AddForce(forceVelocity, ForceMode.Impulse);
        _isJumping = false;
    }

    private void WallTimeUpdate()
    {
        if (_onWallTime <= 0f) return;
        _onWallTime -= Time.deltaTime;
    }

    private void CheckWallUpdate()
    {
        if (_isGrounded)
        {
            _onWall = false;
            return;
        }

        Vector3 castPosCenter = _collider.height / 2f * Vector3.up + transform.position;
        Vector3 direction = _faceRight ? Vector3.right : -Vector3.right;
        float distance = _collider.radius + 0.25f;

        if (Physics.Raycast(castPosCenter, direction, distance, _groundMask))
        {
            Vector3 castPosUpper = _collider.height * Vector3.up + transform.position;
            if (Physics.Raycast(castPosUpper, direction, distance, _groundMask))
            {
                _onWall = true;
                return;
            }
        }

        _onWall = false;
    }

    private void Crouch()
    {
        _isCrouching = true;
        _isSprinting = false;

        _collider.height = _crouchColliderHeight;
        _collider.center = new Vector3(_collider.center.x, _collider.height / 2f, _collider.center.z);

        // Animation here
        _model.localScale = new Vector3(_model.localScale.x, _collider.center.y, _model.localScale.z);
    }

    private void Uncrouch()
    {
        if (!_isCrouching) return;

        Vector3 castPos = _collider.height * Vector3.up + transform.position;
        float maxDistance = _defaultColliderHeight - _crouchColliderHeight;
        if (Physics.Raycast(castPos, Vector3.up, maxDistance, _groundMask)) return;

        _isCrouching = false;
        _collider.height = _defaultColliderHeight;
        _collider.center = new Vector3(_collider.center.x, _collider.height / 2f, _collider.center.z);

        // Animation here
        _model.localScale = new Vector3(_model.localScale.x, _collider.center.y, _model.localScale.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if (!Application.isPlaying) return;

        // Wall
        Vector3 castPosCenter = _collider.height / 2f * Vector3.up + transform.position;
        Vector3 castPosUpper = _collider.height * Vector3.up + transform.position;
        Vector3 direction = _faceRight ? Vector3.right : -Vector3.right;
        float distance = _collider.radius + 0.25f;
        Gizmos.DrawRay(castPosCenter, direction * distance);
        Gizmos.DrawRay(castPosUpper, direction * distance);
    }
}
