using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class CharacterInteractBehaviour : MonoBehaviour, IInteractable
{
    // House
    private HouseBehaviour _assignedHouse;
    public HouseBehaviour AssignedHouse =>_assignedHouse;
    private Transform _housePos;
    private CharacterData _data;

    // Item
    [SerializeField] protected Transform _holdSocket;
    [SerializeField] private float _itemDeletionTime = 10f;
    protected ItemBehaviour _heldItem;

    // AI
    protected CharacterAIManager _aiManager;
    protected SmoothTransition _splineTransition;

    [Header("Player Feedback")]
    [SerializeField] protected Animator _animator;

    [Header("Sound")]
    [SerializeField] protected AudioSource _interactAudioSource;
    [SerializeField] protected ParticleSystem _textBubble;
    [SerializeField] protected RandomEffect _reactSFX;

    [Header("Events")]
    [SerializeField] public UnityEvent OnGiftReceived = new ();
    [SerializeField] public UnityEvent OnGoodGiftReceived = new();
    [SerializeField] public UnityEvent OnBadGiftReceived = new();



    // START
    //--------------------------------------------------
    protected virtual void Awake()
    {
        _aiManager = GetComponent<CharacterAIManager>();

        if(!_animator)
            _animator = GetComponentInChildren<Animator>();
        _splineTransition = GetComponent<SmoothTransition>();

        _data = GetComponent<CharacterData>();
    }

    // FUNCTIONALITY
    //--------------------------------------------------
    public void EnableFollowingAI(SplineContainer followSpline, PlayerSplineMovement player)
    {
        _aiManager.EnableFollowingAI(followSpline, player);
    }

    public void EnableMovementAI([CanBeNull] SplineContainer newSpline = null)
    {
        if (_assignedHouse)
        {
            GetComponent<CharacterAISplineMovement>().DoIdle = false;
            _aiManager.FreezeAI();
            _aiManager.ReparentDefault();
            _splineTransition.TransitionToPositionWalk(_housePos);
            return;
        }

        if (newSpline) _aiManager.EnableMovementAI(newSpline, _aiManager.Spline.Loop);
        else _aiManager.EnableMovementAI(_aiManager.Spline.Loop);
    }
    public void EnableMovementAI(SplineAnimate.LoopMode mode, [CanBeNull] SplineContainer newSpline = null)
    {
        if(newSpline) _aiManager.EnableMovementAI(newSpline, mode);
        else _aiManager.EnableMovementAI(mode);
    }
    public void AssignToHouse(HouseBehaviour house)
    {
        if (house == null)
        {
            if(_assignedHouse) 
                _assignedHouse.RemoveAssignee(_data);
            _assignedHouse = null;
            _housePos = null;
            return;
        }

        _assignedHouse = house;
        if (!_assignedHouse)
            return;

        _housePos = _assignedHouse.AddAssignee(_data);

        _aiManager.FreezeAI();
        _aiManager.ReparentDefault();
        _splineTransition.TransitionToPositionWalk(_housePos);
    }

    // INTERACTION
    //--------------------------------------------------
    public virtual bool CanInteract(GameObject interactor)
    {
        bool isFrozen = _aiManager.IsFrozen && !_assignedHouse;
        bool isTransitioning = GetComponent<SmoothTransition>()?.IsTransitioning ?? false;
        return (!isFrozen && !isTransitioning);
    }
    public virtual void Interact(GameObject interactor)
    {
        interactor.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("CharacterInteraction");
        if (interactor.TryGetComponent(out CharacterInteractionManager component))
            component.StartInteraction(this);
        _aiManager.FreezeAI();

        // Text Bubble
        if(_textBubble)
            _textBubble.Stop();

        // Audio
        float randomPitch = Random.Range(0.9f, 1.1f);
        _interactAudioSource?.Play();
    }

    //--- Items
    public virtual void ReceiveItem(ItemBehaviour item)
    {
        if (_heldItem != null)
            Destroy(_heldItem.gameObject);
        _heldItem = item;
        if (_heldItem == null)
            return;
        _heldItem.transform.SetParent(_holdSocket, false);
        _heldItem.transform.localPosition = Vector3.zero;
        _heldItem.transform.localRotation = Quaternion.identity;

        int layer = LayerMask.GetMask("NPCRender");
        layer = Mathf.RoundToInt(Mathf.Log(layer, 2));
        var renderers = _heldItem.GetComponentsInChildren<Renderer>();
        foreach (var render in renderers)
            render.gameObject.layer = layer;

        
        OnGiftReceived?.Invoke();
        if (_heldItem.IsGoodGift)
            OnGoodGiftReceived?.Invoke();
        else
            OnBadGiftReceived?.Invoke();

        StartCoroutine(DestroyItem());
    }

    private IEnumerator DestroyItem()
    {
        float elapsed = 0f;
        while (elapsed < _itemDeletionTime)
        {
            if (_aiManager.IsWandering)
                elapsed += Time.deltaTime;
            else elapsed = 0f;
            yield return null;
        }

        Destroy(_heldItem.gameObject);
    }

    //--- Emotion
    public void OnEmotionExpressed(Emotion emotion)
    {
        _reactSFX?.Play();

        // animation
        PlayComfortAnimation(emotion);
    }



    // PLAYER FEEDBACK
    //--------------------------------------------------

    //--- Animation
    public void PlayGiftAnimation()
    {
        if(!_animator)
            return;

        _animator.SetTrigger("giftTrigger");
    }

    public void PlayComfortAnimation(Emotion emotion)
    {
        if (!_animator)
            return;

        switch (emotion)
        {
            case Emotion.Happy:
                _animator.SetTrigger("reactionHappyTrigger");
                break;
            case Emotion.Sad:
                _animator.SetTrigger("reactionSadTrigger");
                break;
            case Emotion.Angry:
                _animator.SetTrigger("reactionAngryTrigger");
                break;
            case Emotion.Scared:
                _animator.SetTrigger("reactionScaredTrigger");
                break;
            default:
                break;

        }
    }

    public void PlayTranstitionAnimation()
    {
        if (_animator)
            _animator.SetTrigger("transitionTrigger");
    }
}
