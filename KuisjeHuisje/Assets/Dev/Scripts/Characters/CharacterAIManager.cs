using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SmoothTransition))]
public class CharacterAIManager : MonoBehaviour
{
    [SerializeField] private CharacterAISplineMovement _ai;
    [SerializeField] private CharacterPlayerFollowMovement _follow;
    [SerializeField] private SmoothTransition _splineTransition;
    public bool IsFrozen => !_follow.enabled && !_ai.enabled;
    public bool IsWandering => _ai.enabled;
    public bool IsFollowing => _follow.enabled;
    private Transform _parent;
    private SplineContainer _originalContainer;
    public SplineAnimate Spline => _splineAnimate;
    private SplineAnimate _splineAnimate;

    [SerializeField] private Animator _animator;

    // START
    //--------------------------------------------------
    private void Awake()
    {
        _splineTransition = GetComponent<SmoothTransition>();
        _splineAnimate = GetComponent<SplineAnimate>();
        _originalContainer = _splineAnimate.Container;
        _parent = transform.parent;
        _animator = GetComponentInChildren<Animator>();
    }

    // HELPER
    //--------------------------------------------------
    public void ReparentDefault()
    {
        transform.SetParent(_parent);
    }
    public void SetDefaults(SplineContainer spline, Transform parent)
    {
        _originalContainer = spline;
        _parent = parent;
    }


    // FUNCTIONALITY
    //--------------------------------------------------
    public void EnableMovementAI(SplineAnimate.LoopMode mode)
    {
        EnableMovementAI(_originalContainer, mode);
    }
    public void EnableMovementAI(SplineContainer newSpline, SplineAnimate.LoopMode mode, bool transition = true)
    {
        FreezeAI();
        transform.SetParent(_parent);
        if (transition)
        {
            _splineTransition.OnTransitionComplete.AddListener(OnTransitionCompletedAI);
            _splineTransition.TransitionToSpline(newSpline, mode);
        }
        else
        {
            GetComponent<SplineAnimate>().Container = newSpline;
            UnfreezeMovementAI();
        }
        SetDefaults(newSpline, _parent);
    }
    public void EnableFollowingAI(SplineContainer followSpline, PlayerSplineMovement player)
    {
        // setup
        FreezeAI();
        transform.SetParent(WorldSwitchManager.Instance.Moveables.transform);
        _follow.Player = player;

        // hookup event
        _splineTransition.OnTransitionComplete.AddListener(OnTransitionCompletedFollow);

        // transition
        float targetT = player.NormalizedSplineTime - _follow.TOffset * player.NonZeroDirection;
        _splineTransition.TransitionToSpline(followSpline, SplineAnimate.LoopMode.Loop, targetT);
        SetDefaults(WorldSwitchManager.Instance.CurrentWorldPair.world.RandomCharacterSpline, _parent);
    }

    private void UnfreezeMovementAI()
    {
        _ai.enabled = true;
        _ai.DoIdle = true;
    }
    private void UnfreezeFollowingAI()
    {
        _follow.enabled = true;
    }
    public void FreezeAI()
    {
        _ai.enabled = false;
        _follow.enabled = false;
        _ai.DoIdle = false;
    }


    // EVENTS
    //--------------------------------------------------
    private void OnTransitionCompletedFollow()
    {
        _ai.DoIdle = false;
        _ai.enabled = false;
        _follow.enabled = true;

        UnfreezeFollowingAI();
        _splineTransition.OnTransitionComplete.RemoveListener(OnTransitionCompletedFollow);
    }
    private void OnTransitionCompletedAI()
    {
        _ai.enabled = true;
        _follow.enabled = false;

        UnfreezeMovementAI();
        _splineTransition.OnTransitionComplete.RemoveListener(OnTransitionCompletedAI);
    }

    // ANIMATION
    //--------------------------------------------------
    private void Update()
    {
        if (!_animator) return;

        if (IsFollowing)
        {
            float diff = Mathf.Abs(_follow.TargetT - _splineAnimate.NormalizedTime);
            diff = Mathf.Min(diff, 1f - diff);
            bool isMoving = diff < 0.001f;
            _animator.SetBool("isWalking", !isMoving);
            _animator.SetBool("isRunning", !isMoving);
            _animator.SetFloat("speedMultiplier", 1.0f);
        }
        else if (IsWandering)
        {
            // WALKING
            _animator.SetBool("isRunning", false);
            _animator.SetBool("isWalking", _ai.IsMoving);
            _animator.SetFloat("speedMultiplier", _ai.AnimationSpeedMultiplier);

            // TURNING
            float diff = _ai.Direction - _ai.PreviousDirection;
            if (Mathf.Abs(diff) > 1.1f)
            {
                _animator.SetTrigger("fullTurnTrigger");
            }
            else if (Mathf.Abs(diff) > 0.1f)
            {
                _animator.SetTrigger("smallTurnTrigger");
            }
        }
        else
        {
            _animator.SetBool("isRunning", _splineTransition.IsTransitioning);
            _animator.SetBool("isWalking", _splineTransition.IsTransitioning);
        }
    }
}
