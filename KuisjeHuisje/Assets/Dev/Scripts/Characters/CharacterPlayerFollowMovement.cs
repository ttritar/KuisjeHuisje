using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineAnimate))]
public class CharacterPlayerFollowMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField][Range(0f,0.1f)] private float _tOffset;
    [SerializeField][Range(1f, 10f)] private float _rotationSpeed = 5f;
    [SerializeField][Range(1f, 20f)] private float _followSpeed = 2f;
    [SerializeField] private GameObject _characterMesh;
    public PlayerSplineMovement Player { get; set; }
    public float NormalizedSplineTime => _splineAnimate.NormalizedTime;

    private SplineAnimate _splineAnimate;
    private SplineContainer _splineContainer;
    
    private float _nonZeroPlayerDirection;
    public float TargetT { get; private set; }
    public float TOffset => _tOffset;

    // START
    //--------------------------------------------------
    private void Awake()
    {
        _splineAnimate = GetComponent<SplineAnimate>();
        _splineContainer = _splineAnimate.Container;

        if (!_characterMesh) Debug.LogError("[CharacterPlayerFollowMovement] No Character Mesh assigned.", this);
    }

    // LOOP
    //--------------------------------------------------
    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    // MOVEMENT
    //--------------------------------------------------
    private void HandleMovement()
    {
        float currentT = _splineAnimate.NormalizedTime;
        TargetT = Player.NormalizedSplineTime - _tOffset * Player.NonZeroDirection;

        float delta = TargetT - currentT;
        if (delta > 0.5f) delta -= 1f;
        else if (delta < -0.5f) delta += 1f;

        currentT += delta * _followSpeed * Time.deltaTime;

        _splineAnimate.NormalizedTime = Mathf.Repeat(currentT, 1f);
    }
    private void HandleRotation()
    {
        float delta = Player.NormalizedSplineTime - _splineAnimate.NormalizedTime;
        if (delta > 0.5f) delta -= 1f;
        else if (delta < -0.5f) delta += 1f;

        float yRot = delta >= 0f ? 0f : -180f;

        Quaternion targetRotation = Quaternion.Euler(0f, yRot, 0f);
        _characterMesh.transform.localRotation = Quaternion.Slerp(
            _characterMesh.transform.localRotation,
            targetRotation,
            Time.deltaTime * _rotationSpeed
        );
    }

    // PLAYER FEEDBACK
    //--------------------------------------------------
}
