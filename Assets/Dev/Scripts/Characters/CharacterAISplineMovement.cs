using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using Random = UnityEngine.Random;


[RequireComponent(typeof(SplineAnimate))]
public class CharacterAISplineMovement : MonoBehaviour
{
    [Header("Components")]
    private SplineAnimate _splineAnimate;
    [SerializeField] private BoxCollider _avoidCollider;

    [Header("Movement Settings")]
    private bool _isMoving = true;
    public bool IsMoving => _isMoving;
    [SerializeField] private float _minSpeed = 0.007f;
    [SerializeField] private float _maxSpeed = 0.012f;
    public float MovementSpeed => _movementSpeed;
    private float _movementSpeed;
    public bool DoIdle { get; set; } = true;

    [SerializeField] private float _acceleration = 1.5f;
    private float _currentSpeed;

    [SerializeField] private float _randomIdleChance = 0.1f;
    [SerializeField] private float _minIdleDuration = 0.5f;
    [SerializeField] private float _maxIdleDuration = 1.0f;
    private float _idleDuration = 0.0f;
    [SerializeField] private float _idleDirectionSwitchChance = 0.4f;

    [SerializeField] private float _slowDownDuration = 0.2f;


    [Header("Rotation Settings")]
    public GameObject CharacterMesh => _characterMesh;
    [SerializeField] private GameObject _characterMesh;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private bool _autoRotateY = true;

    public float NormalizedSplineTime => _splineAnimate.NormalizedTime;
    private float _t; // 0 to 1
    private float _direction = 1f; // 1f = R, -1f = L
    public float Direction => _direction;
    private float _previousDirection = 1f;
    public float PreviousDirection => _previousDirection;


    [Header("Player Feedback")]
    [SerializeField] private Animator _animator;
    [SerializeField] private float _animationBaseSpeed = 0.01f;
    private float _animationSpeedMultiplier = 1f;
    public float AnimationSpeedMultiplier => _animationSpeedMultiplier;
    // idle
    [SerializeField] private int _idleAnimationsCount = 7;

    [SerializeField] private float _minAnimationTime = 3.5f;
    // talk
    [SerializeField] private ParticleSystem _textBubble;


    [Header("Events")] 
    [SerializeField] private UnityEvent OnTalk;
    [SerializeField] private UnityEvent OnIdle;



    // START
    //--------------------------------------------------
    private void Awake()
    {
        _splineAnimate = GetComponent<SplineAnimate>();
        if (!_splineAnimate) Debug.LogError("[CharacterAISplineMovement] No SplineAnimate component found.", this);

        if (!_characterMesh) _autoRotateY = false;

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }
    private void Start()
    {
        //-- RANDOM X --------------------------
        // start position
        _t = Random.Range(0f, 1f);
        _splineAnimate.NormalizedTime = _t;

        // direction
        if (Random.value < 0.5f) _direction = -1f;
        else _direction = 1f;

        // speed
        _movementSpeed = Random.Range(_minSpeed, _maxSpeed);
        _animationSpeedMultiplier = _movementSpeed / _animationBaseSpeed;


        //-- APPLY --------------------------
        _splineAnimate.MaxSpeed = _movementSpeed;
        _splineAnimate.Pause();
    }
    private void OnEnable()
    {
        _t = _splineAnimate.NormalizedTime;
        _isMoving = true;
    }

    // LOOP
    //--------------------------------------------------
    private void Update()
    {
        _previousDirection = _direction;
        if (_isMoving)
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, _movementSpeed, Time.deltaTime * _acceleration);
            HandleMovement();

            // random idle
            if (Random.value < _randomIdleChance * Time.deltaTime)
            {
                StartCoroutine(HandleIdle(false));
                PlayOnIdleFeedback();
            }
        }
        else
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, 0f, Time.deltaTime * _acceleration);
        }

        HandleRotation();
    }

    // MOVEMENT
    //------------------------------
    private void HandleMovement()
    {
        // update t
        _t += _direction * _currentSpeed * Time.deltaTime;
        _t = _splineAnimate.Loop == SplineAnimate.LoopMode.Loop ? Mathf.Repeat(_t, 1f) : Mathf.Clamp01(_t); // clamp between 0 and 1 if not looping
        _splineAnimate.NormalizedTime = _t;
    }
    private void HandleRotation()
    {
        // y rotation (direction)
        if (_autoRotateY)
        {
            float yRot;
            if (_direction < 0f) yRot = -180f;
            else if (_direction > 0f) yRot = 0f;
            else yRot = 90f;

            // slerp to new rotation local space 
            Quaternion targetRotation = Quaternion.Euler(0f, yRot, 0f);
            _characterMesh.transform.localRotation = Quaternion.Slerp(_characterMesh.transform.localRotation, targetRotation, Time.deltaTime * _rotationSpeed);
        }

    }
    private IEnumerator HandleIdle(bool ignoreTurnChance)
    {
        if (!DoIdle) yield break;
        _isMoving = false;

        float approachTimer = 0f;

        while (approachTimer < _slowDownDuration)
        {
            if (!DoIdle) yield break;
            _currentSpeed = Mathf.Lerp(_currentSpeed, 0f, Time.deltaTime * (_acceleration * 0.5f)); // slow down gradually
            _t += _direction * _currentSpeed * Time.deltaTime;
            _t = _splineAnimate.Loop == SplineAnimate.LoopMode.Loop
                ? Mathf.Repeat(_t, 1f)
                : Mathf.Clamp01(_t);

            _splineAnimate.NormalizedTime = _t;

            approachTimer += Time.deltaTime;
            yield return null;
        }
        while (_currentSpeed > 0.001f)
        {
            if (!DoIdle) yield break;
            _currentSpeed = Mathf.Lerp(_currentSpeed, 0f, Time.deltaTime * _acceleration);
            _t += _direction * _currentSpeed * Time.deltaTime;
            _t = _splineAnimate.Loop == SplineAnimate.LoopMode.Loop
                ? Mathf.Repeat(_t, 1f)
                : Mathf.Clamp01(_t);

            _splineAnimate.NormalizedTime = _t;
            yield return null;
        }

        // wait
        _idleDuration = Random.Range(_minIdleDuration, _maxIdleDuration);
        yield return new WaitForSeconds(_idleDuration);

        // maybe switch direction
        if (Random.value < _idleDirectionSwitchChance || ignoreTurnChance)
            _direction *= -1f;


        _isMoving = true;
    }

    // AVOIDING
    //--------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other == _avoidCollider) return;
        if (!other.CompareTag("NPC")) return;
        if (!_isMoving) return;

        // handle idle
        StartCoroutine(HandleIdle(true));
        PlayOnTalkFeedback();
    }



    // PLAYER FEEDBACK
    //--------------------------------------------------
    private void PlayOnIdleFeedback()
    {
        if (_isMoving) return;
        OnIdle?.Invoke();

        // ANIMATION
        if (_animator)
        {
            if(_idleDuration >= _minAnimationTime)
            {
                int idleIndex = Random.Range(1, _idleAnimationsCount + 1);
                _animator.SetTrigger($"idleTrigger{idleIndex}");
            }
        }
    }

    private void PlayOnTalkFeedback()
    {
        if (_isMoving) return;
        OnTalk?.Invoke();

        PlayTextBubble();
    }

    // VFX
    private void PlayTextBubble()
    {
        if (_textBubble)
        {
            _textBubble.Play();
        }
    }
}
