using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using static System.TimeZoneInfo;

[RequireComponent(typeof(SplineAnimate))]
public class PlayerSplineMovement : MonoBehaviour
{
    [Header("Splines")]
    private SplineAnimate _splineAnimate;
    private SplineContainer _splineContainer;

    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed = 0.05f;
    [SerializeField][Range(0f, 1f)] private float _screenPercentageDeadzone = 0.5f;
    [SerializeField] private float _idleCooldown = 2f;

    [SerializeField] private AnimationCurve _accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float _accelerationDuration = 0.5f;
    [SerializeField] private float _deccelerationDuration = 0.5f;
    private float _speedBlend = 0f;
    private float _transitionTimer = 0f;

    private float _currentSpeed = 0f;
    private float _idleTimer = 0f;

    [Header("Rotation Settings")] 
    [SerializeField] private GameObject _playerMesh;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private bool _autoRotateY = true;
    public bool AutoRotateY { get => _autoRotateY; set => _autoRotateY = value; }


    [Header("Animation")]
    [SerializeField] private Animator _animator;

    [Header("Events")]
    [SerializeField]  UnityEngine.Events.UnityEvent OnStepTaken = new();
    [SerializeField] private float _stepSpeedMultiplier = 50f;
    private float _elapsedTime = 0f;


    public bool CanMove { get; set; } = true;
    private bool _isAccelerating = false;
    private bool _isMoving = false;
    private float _t;
    public float NormalizedSplineTime => _t;

    // 1f = R, -1f = L
    public float Direction { get; private set; } = 1f;
    public float NonZeroDirection { get; private set; } = 1f;
    private float _previousDirection = 1f;

    // START
    //--------------------------------------------------
    private void Awake()
    {
        _splineAnimate = GetComponent<SplineAnimate>();
        if (!_splineAnimate) Debug.LogError("[PlayerSplineMovement] No SplineAnimate component found.", this);
        _splineContainer = _splineAnimate.Container;
        if (!_splineContainer)
            Debug.LogError("[PlayerSplineMovement] No SplineContainer found on SplineAnimate component.", this);

        if (!_playerMesh) _autoRotateY = false;
    }
    private void Start()
    {
        _splineAnimate.MaxSpeed = _movementSpeed;
        _splineAnimate.Pause();
        _t = _splineAnimate.NormalizedTime;
        _splineAnimate.NormalizedTime = _t;
    }

    // LOOP
    //--------------------------------------------------
    private void Update()
    {
        _previousDirection = Direction;

        HandleMovement();
        HandleRotation();

        // idle cooldown
        if (!_isMoving && _idleCooldown > 0f)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer >= _idleCooldown)
            {
                _idleTimer = 0f;
                Direction = 0f;
            }
        }

        //--- ANIMATION 
        UpdateAnimation();
    }


    // MOVEMENT
    //--------------------------------------------------
    public void FullStop()
    {
        _transitionTimer = 0f;
        _isMoving = false;
        _isAccelerating = false;
        _currentSpeed = 0f;
    }
    public void ForceSetDirection(int dir)
    {
        Direction = (dir <= -1) ? -1 : ((dir >= 1) ? 1 : 0);
        NonZeroDirection = (dir <= -1) ? -1 : ((dir >= 1) ? 1 : NonZeroDirection);
    }
    private void HandleMovement()
    {
        if (_isMoving)
        {
            _transitionTimer += Time.deltaTime;
            _isAccelerating = true;
        }
        else
        {
            _transitionTimer -= Time.deltaTime;
            _isAccelerating = false;
        }

        // full stop
        if (_transitionTimer <= 0)
        {
            _transitionTimer = 0f;
            _isMoving = false;
            _isAccelerating = false;
            return;
        }
        else
        {
            // idle cooldown
            _idleTimer = 0f;
        }

        _transitionTimer = Mathf.Clamp(_transitionTimer, 0f, _isAccelerating ? _accelerationDuration : _deccelerationDuration);

        float t = _transitionTimer / (_isAccelerating ? _accelerationDuration : _deccelerationDuration);
        _speedBlend = _accelerationCurve.Evaluate(t);

        // accelerate to movement speed
        _currentSpeed = _movementSpeed * _speedBlend;

        // update t
        _t += Direction * _currentSpeed * Time.deltaTime;
        _t = _splineAnimate.Loop == SplineAnimate.LoopMode.Loop
            ? Mathf.Repeat(_t, 1f)
            : Mathf.Clamp01(_t); // clamp between 0 and 1 if not looping
        _splineAnimate.NormalizedTime = _t;


        // trigger step event
        _elapsedTime += Time.deltaTime;

        if (_currentSpeed > 0f)
        {
            float stepInterval = 1.0f / (_currentSpeed * _stepSpeedMultiplier);
            if (_elapsedTime >= stepInterval)
            {
                OnStepTaken.Invoke();
                _elapsedTime -= stepInterval; // smoother for variable speed
            }
        }

    }
    private void HandleRotation()
    {
        if (!_autoRotateY) return;

        float yRot;
        if (Direction < 0f)
            yRot = -180f;
        else if (Direction > 0f)
            yRot = 0f;
            
        else
            yRot = 90f;
            

        Quaternion targetRotation = Quaternion.Euler(0f, yRot, 0f);
        _playerMesh.transform.localRotation = Quaternion.Slerp(_playerMesh.transform.localRotation, targetRotation,
            Time.deltaTime * _rotationSpeed);

    }
    public IEnumerator RotateMesh(Quaternion localRot, float duration)
    {
        var startPlayerRot = _playerMesh.transform.localRotation;
        var targetPlayerRot = localRot;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float tLerp = elapsed / duration;

            _playerMesh.transform.localRotation = Quaternion.Slerp(startPlayerRot, targetPlayerRot, tLerp);
            startPlayerRot = _playerMesh.transform.localRotation;

            yield return null;
        }
        _playerMesh.transform.localRotation = targetPlayerRot;
    }

    // INPUT
    //--------------------------------------------------
    public void OnMovementInput(InputAction.CallbackContext context)
    {
        if (!CanMove) return;

        Vector2 input = context.ReadValue<Vector2>();
        _isMoving = true;
        var deadzone = CalculateDeadZone();

        // in dead zone
        if (input.x > deadzone.x && input.x < deadzone.y ||
            (Mathf.Approximately(input.x, 0f) && Mathf.Approximately(input.y, 0f)))
        {
            _isMoving = false;
            return;
        }

        // on right side
        if (input.x > deadzone.y)
        {
            Direction = 1f;
            NonZeroDirection = 1f;
            return;
        }

        // on left side
        if (input.x < deadzone.x)
        {
            Direction = -1f;
            NonZeroDirection = -1f;
            return;
        }
    }
    private Vector2 CalculateDeadZone()
    {
        float width = Screen.width;
        float center = width * 0.5f;
        float radius = _screenPercentageDeadzone * width * 0.5f;

        float left = center - radius;
        float right = center + radius;

        return new Vector2(left, right);
    }


    // PLAYER FEEDBACK + ANIMATIONS
    //--------------------------------------------------
    private void UpdateAnimation()
    {
        if (!_animator) return;

        // WALKING
        bool walking = _isMoving && _currentSpeed < _movementSpeed * 0.5f;
        bool running = _currentSpeed >= _movementSpeed * 0.5f;

        _animator.SetBool("isWalking", walking || running);
        _animator.SetBool("isRunning", running);

        // TURNING
        float diff = Direction - _previousDirection;
        if (Mathf.Abs(diff) > 1.1f)
        {
            PlayFullTurnAnimation();
        }
        else if (Mathf.Abs(diff) > 0.1f)
        {
            PlaySmallTurnAnimation();
        }
    }
    private void PlaySmallTurnAnimation()
    {
        if (!_animator) return;
        _animator.SetTrigger("smallTurnTrigger");
    }
    private void PlayFullTurnAnimation()
    {
        if (!_animator) return;
        _animator.SetTrigger("fullTurnTrigger");
    }


    // DEBUG
    //--------------------------------------------------
#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool _showDeadzoneDebug = true;
    private void OnGUI()
    {
        if (!_showDeadzoneDebug)
            return;
        float width = Screen.width;
        float height = Screen.height;

        Vector2 deadzone = CalculateDeadZone();

        float rectX = deadzone.x;
        float rectWidth = deadzone.y - deadzone.x;

        Rect rect = new Rect(rectX, 0f, rectWidth, height);

        Color old = GUI.color;
        GUI.color = new Color(1f, 0f, 0f, 0.3f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = old;
    }
#endif
}
