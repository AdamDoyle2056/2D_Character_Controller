using System;
using UnityEngine;
using CharacterController;

// Automatically adds rb and capcollider IF none on player
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private ScriptableStats _stats;
    [SerializeField] private PlayerInput _input;

    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private Vector2 _frameVelocity;
    private float _time;
    private bool _cachedQueryStartInColliders;

    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    private bool _grounded;
    private float _frameLeftGrounded = float.MinValue;

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();

        // Bool for ignoring raycast hits inside a collider
        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Update()
    {   
        // Time calculations run every frame
        _time += Time.deltaTime;

        // If the player presses the jump button this frame
        if (_input.Current.JumpDown)
        {
            _jumpToConsume = true;          // Mark that a jump input occurred 
            _timeJumpWasPressed = _time;    // Record the exact time the jump button was pressed 
        }
    }

    private void FixedUpdate()
    {   
        // Important physics checking occurs at a fixed time step
        CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleGravity();

        ApplyMovement();
    }

    private void CheckCollisions()
    {   
        // Enabling raycast to start in colliders and not trigger
        Physics2D.queriesStartInColliders = false;

        // Both raycast are determined by the size of the collider
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

        // If the player hits the ceiling stop vertical velocity
        if (ceilingHit)
            _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        // If the player was not grounded before but is now touching the ground
        if (!_grounded && groundHit)
        {
            _grounded = true;                             // Mark the player as grounded
            _coyoteUsable = true;                         // Reset coyote time availability
            _bufferedJumpUsable = true;                   // Allowed jump
            _endedJumpEarly = false;                      // Reset jump state (no early release)
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y)); // Trigger grounded event with landing speed
        }

        // If the player was grounded before but has now left the ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;                            // Mark the player as airborne
            _frameLeftGrounded = _time;                   // Record the time they left the ground (for coyote time)
            GroundedChanged?.Invoke(false, 0);            // Trigger event for leaving the ground
        }

        // Disabling raycast to start in colliders and not trigger
        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    // Checks if player can jump given there is a buffered jump avaiable and jump input occured recently
    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;

    // Checks if player can use coyote jump given they arent grounded and within the coyote time 
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

    private void HandleJump()
    {
        // If the player released the jump button mid-air while still moving upward, mark jump as ended early
        if (!_endedJumpEarly && !_grounded && !_input.Current.JumpHeld && _rb.velocity.y > 0)
            _endedJumpEarly = true;

        // If there’s no jump input to process and no buffered jump, stop here
        if (!_jumpToConsume && !HasBufferedJump) return;

        // If the player is grounded or within coyote time, perform the jump
        if (_grounded || CanUseCoyote)
            ExecuteJump();

        // Consume the jump input so it doesn’t trigger again next frame
        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _endedJumpEarly = false;                // Reset flag so the jump isn't marked as ended early
        _timeJumpWasPressed = 0;                // Reset jump input timer (the jump has now been used)
        _bufferedJumpUsable = false;            // Consume the buffered jump so it can’t trigger again
        _coyoteUsable = false;                  // Consume coyote time — it can’t be reused after jumping
        _frameVelocity.y = _stats.JumpPower;    // Apply upward velocity to make the player jump
        Jumped?.Invoke();                       // Fire the Jumped event for other systems 
    }

    private void HandleDirection()
    {
        float inputX = _input.Current.Move.x;

        // If the player has no horizontal input, gradually reduce horizontal velocity
        if (inputX == 0)
        {
            float deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        // If there is horizontal input, accelerate player toward target speed
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, inputX * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleGravity()
    {
        // If the player is grounded and not moving upward, apply a small downward force to keep them grounded
        if (_grounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = _stats.GroundingForce;
        }
        else
        {
            float gravity = _stats.FallAcceleration;

            // If the player released the jump early while moving upward, increase gravity to shorten the jump
            if (_endedJumpEarly && _frameVelocity.y > 0)
                gravity *= _stats.JumpEndEarlyGravityModifier;

            // Apply gravity toward maximum fall speed smoothly
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, gravity * Time.fixedDeltaTime);
        }
    }

    private void ApplyMovement()
    {   
        // Apply calcualted movement to player rigidbody
        _rb.velocity = _frameVelocity;
    }
}