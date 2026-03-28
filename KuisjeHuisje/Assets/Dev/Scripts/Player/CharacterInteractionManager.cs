using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.Splines;

[RequireComponent(typeof(PlayerSplineMovement))]
[RequireComponent(typeof(PlayerInput))]
public class CharacterInteractionManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _tOffset;
    [SerializeField] private float _rotationAngle = 45f;
    [SerializeField] private float _rotationDuration = .5f;

    [Header("Spline")]
    [SerializeField] private SplineContainer _followSpline;

    [Header("UI")]
    [SerializeField] private CinemachineCamera _interactionCamera;
    [SerializeField] private LayerMask _npcRenderLayer;
    private int _npcLayer;

    [Header("Player Feedback")]
    [SerializeField] private Animator _animator;
    [SerializeField] private float _giftSwapDelay = 1f;

    // interactors
    public CharacterInteractBehaviour CurrentFollower => _currentFollower;
    private CharacterInteractBehaviour _currentInteractor;
    private CharacterInteractBehaviour _currentFollower;
    
    // player
    private PlayerSplineMovement _playerSplineMovement;
    private ItemInteractor _itemInteractor;


    [Header("Events")]
    public UnityEvent OnAssigningCharacter = new ();
    [FormerlySerializedAs("OnStartFollowing")] public UnityEvent OnStartFollowUI = new ();
    [FormerlySerializedAs("OnStopFollowing")] public UnityEvent OnStopFollowUI = new ();
    public UnityEvent OnStartFollow = new();
    public UnityEvent OnStopFollow = new();
    public UnityEvent OnStartInteraction = new ();
    public UnityEvent OnStartInteractionLate = new ();
    public UnityEvent OnEndInteraction = new ();

    // AWAKE
    //--------------------------------------------------
    private void Awake()
    {
        _playerSplineMovement = GetComponent<PlayerSplineMovement>();
        _itemInteractor = GetComponent<ItemInteractor>();

        _npcLayer = LayerMask.NameToLayer("NPC");

        if (!_animator)
            _animator = GetComponentInChildren<Animator>();

        _itemInteractor.OnItemPickedUp.AddListener(OnFirstItemPickedUp);
        OnAssigningCharacter.AddListener(OnFirstCharacterAssigned);
    }

    // INPUT
    //--------------------------------------------------
    public void StopCharacterInteraction(InputAction.CallbackContext ctx)
    {
        EndInteraction();
    }

    // HELPERS
    //--------------------------------------------------

    //--- Follow
    private void SetFollowerLayer(int layer)
    {
        var renderers = _currentFollower.GetComponentsInChildren<Renderer>();
        foreach (var render in renderers)
            render.gameObject.layer = layer;
    }
    private void FollowInternal()
    {
        _currentFollower?.EnableMovementAI();
        _currentFollower = _currentInteractor;

        var charData = _currentFollower.GetComponent<CharacterData>();
        if (charData != null && _currentFollower.AssignedHouse != null)
            FileLogger.Instance.LogNPCUnassignment(charData);

        _currentFollower.AssignToHouse(null);
        _currentFollower.EnableFollowingAI(_followSpline, _playerSplineMovement);

        int layer = Mathf.RoundToInt(Mathf.Log(_npcRenderLayer.value, 2));
        SetFollowerLayer(layer);

        OnStartFollow?.Invoke();
    }
    private void UnfollowInternal()
    {
        SetFollowerLayer(_npcLayer);
        _currentFollower?.EnableMovementAI();
        _currentFollower = null;

        OnStopFollow?.Invoke();
    }
    private void UpdateFollowUI()
    {
        bool isFollowing = _currentFollower == _currentInteractor;

        if (!isFollowing)
            OnStopFollowUI.Invoke();
        else
            OnStartFollowUI.Invoke();
    }

    // INTERACTION
    //--------------------------------------------------
    public void EndInteraction()
    {
        if (_currentInteractor == null)
            return;

        _currentInteractor.GetComponent<CharacterAISplineMovement>().DoIdle = true;

        if(_currentFollower != _currentInteractor) _currentInteractor?.EnableMovementAI();
        else _currentFollower.EnableFollowingAI(_followSpline, _playerSplineMovement);
        

        OnEndInteraction.Invoke();
        _playerSplineMovement.AutoRotateY = true;
        _currentInteractor = null;
        _interactionCamera.gameObject.SetActive(false);
        GetComponent<PlayerInput>().SwitchCurrentActionMap("MainInput");
    }
    private IEnumerator DelayedEndInteraction(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndInteraction();
    }
    public void StartInteraction(CharacterInteractBehaviour other)
    {
        // setup
        _interactionCamera.gameObject.SetActive(true);
        _currentInteractor = other;
        _currentInteractor.GetComponent<CharacterAISplineMovement>().DoIdle = false;
        OnStartInteraction.Invoke();
        UpdateFollowUI();

        // disable player rot
        _playerSplineMovement.AutoRotateY = false;
        _playerSplineMovement.ForceSetDirection(-1);
        var playerSpline = GetComponent<SplineAnimate>();

        // transition
        if (!_currentInteractor.TryGetComponent(out SmoothTransition splineTransition))
            return;

        _playerSplineMovement.FullStop();
        float targetT = Mathf.Repeat(playerSpline.NormalizedTime + _tOffset, 1f);
        splineTransition.TransitionToSpline(playerSpline.Container, targetT);

        // rotation
        StartCoroutine(_playerSplineMovement.RotateMesh(Quaternion.Euler(0f, 180f - _rotationAngle, 0f), _rotationDuration));
        splineTransition.OnTransitionComplete.AddListener(RotateInteractors);

        StartCoroutine(WaitForTransitionThenInvoke(splineTransition));
    }

    private IEnumerator WaitForTransitionThenInvoke(SmoothTransition splineTransition)
    {
        while (splineTransition.IsTransitioning)
            yield return null;
        OnStartInteractionLate.Invoke();
    }
    private void RotateInteractors()
    {
        var splineTransition = _currentInteractor.GetComponent<SmoothTransition>();
        splineTransition?.OnTransitionComplete.RemoveListener(RotateInteractors);

        var interactorMesh = _currentInteractor.GetComponent<CharacterAISplineMovement>().CharacterMesh;
        StartCoroutine(RotateInteractorSmooth(interactorMesh.transform, _rotationAngle, _rotationDuration));
    }
    private IEnumerator RotateInteractorSmooth(Transform interactorT, float yOffset, float duration)
    {
        var localEuler = interactorT.localEulerAngles;
        while (localEuler.y > 180f) localEuler.y -= 180f;

        var startInteractorRot = Quaternion.Euler(localEuler);
        var targetInteractorRot = Quaternion.Euler(0f, yOffset, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float tLerp = elapsed / duration;

            interactorT.localRotation = Quaternion.Slerp(startInteractorRot, targetInteractorRot, tLerp);
            startInteractorRot = interactorT.localRotation;
            yield return null;
        }

        interactorT.localRotation = targetInteractorRot;
    }
    private void OnFirstItemPickedUp()
    {
        EndInteraction();
        TutorialManager.Instance.StartTutorial(TutorialManager.TutorialType.Item);
        _itemInteractor.OnItemPickedUp.RemoveListener(OnFirstItemPickedUp);
    }
    private void OnFirstCharacterAssigned()
    {
        if (!TutorialManager.Instance.HasExplainedTutorial(TutorialManager.TutorialType.Assigning))
            return;
        TutorialManager.Instance.StartTutorial(TutorialManager.TutorialType.Potion);
        OnAssigningCharacter.RemoveListener(OnFirstCharacterAssigned);
    }

    // INTERACTION OPTIONS
    //--------------------------------------------------

    //--- Emotions
    private void ExpressEmotion(Emotion emotion)
    {
        if (_currentInteractor == null)
            return;

        // LOGGING
        var charData = _currentInteractor.GetComponent<CharacterData>();
        if (charData != null)
            FileLogger.Instance.LogEmotion(charData, emotion);

        // PLAYER FEEDBACK
            // sound

            // animation
        PlayEmotionAnimation(emotion);

            // face
        if (TryGetComponent(out CharacterEmotionsSwap emotionSwap))
            emotionSwap.ApplyEmotion(emotion);

        // CHARACTER
        _currentInteractor.OnEmotionExpressed(emotion);
    }
    public void ExpressHappyEmotion() => ExpressEmotion(Emotion.Happy);
    public void ExpressSadEmotion() => ExpressEmotion(Emotion.Sad);
    public void ExpressAngryEmotion() => ExpressEmotion(Emotion.Angry);
    public void ExpressScaredEmotion() => ExpressEmotion(Emotion.Scared);


    //--- Follow
    public void ToggleFollow()
    {
        if (_currentInteractor == null)
            return;

        bool alreadyFollowing = _currentFollower == _currentInteractor;

        if (alreadyFollowing)
            UnfollowInternal();
        else
            FollowInternal();

        UpdateFollowUI();
        EndInteraction();
    }
    public bool AssignFollowerToHouse(HouseBehaviour house)
    {
        if (_currentFollower == null)
            return false;

        var transition = _currentFollower.GetComponent<SmoothTransition>();
        if (transition.IsTransitioning)
            return false;

        SetFollowerLayer(0);


        var charData = _currentFollower.GetComponent<CharacterData>();
        if (charData != null)
            FileLogger.Instance.LogNPCAssignment(charData, house);

        _currentFollower.AssignToHouse(house);
        _currentFollower = null;

        OnAssigningCharacter.Invoke();
        return true;
    }

    //--- Gift
    public void GiveItem()
    {
        if (_currentInteractor == null)
            return;
        StartCoroutine(DelayedGiveItemInteractor(_giftSwapDelay));

        //-- Player Feedback
        PlayGiftAnimation();
        _currentInteractor.PlayGiftAnimation();
    }
    private IEnumerator DelayedGiveItemInteractor(float delay)
    {
        yield return new WaitForSeconds(delay);
        _itemInteractor.GiveItem(_currentInteractor);
    }



    // PLAYER FEEDBACK
    //--------------------------------------------------

    //--- Animations
    private void PlayEmotionAnimation(Emotion emotion)
    {
        if (_currentInteractor == null)
            return;
        if (_animator == null)
            return;

        switch (emotion)
        {
            case Emotion.Happy:
                _animator.SetTrigger("happyTrigger");
                break;
            case Emotion.Sad:
                _animator.SetTrigger("sadTrigger");
                break;
            case Emotion.Angry:
                _animator.SetTrigger("angryTrigger");
                break;
            case Emotion.Scared:
                _animator.SetTrigger("scaredTrigger");
                break;
            default:
                break;
        }
    }

    private void PlayGiftAnimation()
    {
        if (_currentInteractor == null)
            return;
        if (_animator == null)
            return;
        _animator.SetTrigger("giftTrigger");
    }

    public void PlayTransitionAnimation()
    {
        if (_animator)
            _animator.SetTrigger("transitionTrigger");

        if (_currentFollower != null)
            _currentFollower.PlayTranstitionAnimation();
        
    }

}
