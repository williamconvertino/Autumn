using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    
    #region Core
    
    [Header("Layer Masks")]
    [SerializeField] private LayerMask groundedMask;
    
    private Rigidbody2D _rb2d;
    private BoxCollider2D _boxCollider;
    private PlayerInput _playerMovementInput;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _playerMovementInput = GetComponentInChildren<PlayerInput>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Move();
        UpdateAnimation();
    }
    #endregion

    #region Movement
    
    [Header("Movement")]
    [SerializeField] private float walkVelocity = 6.0f;
    [SerializeField] private float jumpVelocity = 25.0f;
    
    private Vector2 _velocity = Vector2.zero;
    
    private void Move()
    {
        
        _velocity.x = _playerMovementInput.GetDirX() * walkVelocity;

        UpdateGravity();
        CheckGrounding();
        UpdateJump();

        _rb2d.velocity = new Vector3(_velocity.x, _velocity.y, 0);
    }
    #endregion
    
    #region Gravity
    
    [Header("Gravity")]
    [SerializeField] private float minGravity = 80f;
    [SerializeField] private float maxGravity = 120f;
    [SerializeField] private float maxFallSpeed = 20f;
    [SerializeField] private float jumpApexThreshold = 10f;
    
    private void UpdateGravity()
    {
        float apexPoint = Mathf.InverseLerp(jumpApexThreshold, 0, Mathf.Abs(_velocity.y));
        float fallSpeed = Mathf.Lerp(minGravity, maxGravity, apexPoint);
        _velocity.y -= fallSpeed * Time.deltaTime;

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = 0;
        }
        else
        {
            _velocity.y = Mathf.Max(_velocity.y, -maxFallSpeed);
        }
    }

    #endregion

    #region Grounding
    
    [Header("Grounding")]
    
    [SerializeField] private int numGroundingDetectors = 3;
    [SerializeField] private float groundingDetectorLength = 0.02f;

    private bool _isGrounded = false;
    private bool _wasGrounded = false;

    private void CheckGrounding()
    {
        _wasGrounded = _isGrounded;

        Bounds bounds = _boxCollider.bounds;
        float detectorStartX = bounds.min.x;
        float detectorEndX = bounds.max.x;
        float detectorYBottom = bounds.min.y;

        _isGrounded = GetDetectorPositions(detectorStartX, detectorEndX, detectorYBottom).Any(startingPoint =>
            {
                Collider2D collider = Physics2D.Raycast(
                    startingPoint, Vector2.down, groundingDetectorLength, groundedMask
                ).collider;

                if (collider == null) return false;
                
                transform.SetParent(collider.transform);
                return true;
            }
        );

        if (!_isGrounded) transform.SetParent(null);
        
    }

    private IEnumerable<Vector2> GetDetectorPositions(float detectorStartX, float detectorEndX, float detectorY)
    {
        float deltaT = (float)1 / (numGroundingDetectors - 1);
        float t = 0.0f;
        
        for (int i = 0; i < numGroundingDetectors - 1; i++)
        {
            yield return new Vector2(Mathf.Lerp(detectorStartX, detectorEndX, t), detectorY);
            t += deltaT;
        }
        yield return new Vector2(detectorEndX, detectorY);
    }

    #endregion
    
    #region Jump
    
    [Header("Double Jump")]
    
    public bool allowDoubleJump = false;
    public float doubleJumpVel = 20.0f;
    public float jumpMaxVel = 25.0f;
    private bool _doubleJumpActive = true;
    
    [Header("Coyote Jump")]
    
    [SerializeField] private float coyoteJumpTime = 0.15f;
    
    private float _coyoteJumpCurrentTimer = 0.0f;
    private bool _canJump = false;
    
    private void UpdateJump()
    {
        if (!_wasGrounded && _isGrounded)
        {
            _canJump = true;
        }

        if (_isGrounded)
        {
            _coyoteJumpCurrentTimer = coyoteJumpTime;
        }
        else
        {
            _coyoteJumpCurrentTimer -= Time.deltaTime;
        }
        
        if (_canJump && _playerMovementInput.GetJump() && _coyoteJumpCurrentTimer >= 0 && _velocity.y <= 0)
        {
            _canJump = false;
            _velocity.y = jumpVelocity;
        }
        
        if (allowDoubleJump && !_isGrounded && _doubleJumpActive && _playerMovementInput.GetJump())
        {
            _canJump = false;
            _doubleJumpActive = false;
            if (_velocity.y < 0)
            {
                _velocity.y = doubleJumpVel;
            }
            else
            {
                _velocity.y = Mathf.Min(doubleJumpVel + _velocity.y, jumpMaxVel);
            }
        }

    }
    #endregion
    
    #region Animation
    
    [Header("Animation")]
    
    [SerializeField] private bool invertX = false;

    private void UpdateAnimation()
    {
        if (_velocity.x > 0) _spriteRenderer.flipX = invertX;
        if (_velocity.x < 0) _spriteRenderer.flipX = !invertX;

        if (_velocity.y > 0)
        {
            _animator.Play("Jump");
        } else if (!_isGrounded && _velocity.y < 0)
        {
            _animator.Play("Fall");
        } else if (_velocity.x == 0)
        {
            _animator.Play("Idle");
        }
        else
        {
            _animator.Play("Run");
        }
    }

    #endregion
    
    #region Gizmos
    private void OnDrawGizmos()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        Bounds bounds = _boxCollider.bounds;
        float detectorStartX = bounds.min.x;
        float detectorEndX = bounds.max.x;
        float detectorYTop = bounds.max.y;
        float detectorYBottom = bounds.min.y;
        
        Gizmos.color = Color.blue;
        foreach (Vector2 startingPoint in GetDetectorPositions(detectorStartX, detectorEndX, detectorYBottom))
        {
            Gizmos.DrawRay(startingPoint, Vector2.down * groundingDetectorLength);    
        }
        Gizmos.color = Color.yellow;
        foreach (Vector2 startingPoint in GetDetectorPositions(detectorStartX, detectorEndX, detectorYTop))
        {
            Gizmos.DrawRay(startingPoint, Vector2.up * groundingDetectorLength);    
        }
    }
    #endregion
}