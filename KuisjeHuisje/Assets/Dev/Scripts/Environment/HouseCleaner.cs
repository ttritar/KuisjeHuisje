using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class HouseCleaner : MonoBehaviour
{
    public HouseCleaningBehaviour SelectedHouse { get; set; }

    [SerializeField] private Camera _camera;
    [SerializeField] private float _interactRange = 100f;
    [SerializeField] private LayerMask _layerMask;
    private bool _hasCleanedOneHouse = false;

    [SerializeField] private float _exitCleaningDelay = 1.0f;
    public float ExitCleaningDelay => _exitCleaningDelay;
    [SerializeField] private float _delayedEventTime = 2.0f;

    [Header("PlayerFeedback")]
    [SerializeField] private Animator _animator;

    [Header("Events")]
    public UnityEvent OnInteractionStarted = new();
    public UnityEvent OnEnterCleaningMode = new();
    public UnityEvent OnExitCleaningMode = new();
    public UnityEvent OnDelayedExitEvent = new();
    public UnityEvent OnEnterCleaningModeTutorial = new();
    public UnityEvent<int, int> OnStrikeReceived = new();

    public UnityEvent OnCleaningComplete = new();
    public UnityEvent OnCleaningFailed  = new();

    // START
    //--------------------------------------------------
    private void Start()
    {
        if (_camera == null)
            _camera = Camera.main;

        OnExitCleaningMode.AddListener(() => StartCoroutine(TriggerDelayedExitEvent()));
    }

    private void OnDestroy()
    {
        OnExitCleaningMode.RemoveAllListeners();
    }

    // INTERACTION
    //--------------------------------------------------
    public void TryInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || ctx.canceled)
            return;
        if (EventSystem.current == null /*&& EventSystem.current.IsPointerOverGameObject()*/)
            return;

        var pos = Pointer.current.position.ReadValue();
        Ray ray = _camera.ScreenPointToRay(pos);

        if (!Physics.Raycast(ray, out RaycastHit hit, _interactRange, _layerMask))
            return;
        var interactable = hit.collider.GetComponent<IInteractable>();
        if (interactable != null && interactable.CanInteract(gameObject))
            InteractWith(interactable);
    }

    private void InteractWith(IInteractable interactable)
    {
        OnInteractionStarted.Invoke();
        interactable.Interact(gameObject);
        SelectedHouse.OnCleanComplete.AddListener(SetCleanedOneHouse);
    }

    public void OnExit(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || TutorialManager.Instance.IsTutorialRunning)
            return;
        ExitCleaningMode();

    }
    private IEnumerator TriggerDelayedExitEvent()
    {
        yield return new WaitForSeconds(_delayedEventTime);

        // dont when tutorial running
        if (TutorialManager.Instance.IsTutorialRunning) yield break;

        OnDelayedExitEvent.Invoke();

        // input 
        SetInputMain();
    }

    // CLEANING
    //--------------------------------------------------
    public void ExitCleaningMode()
    {
        if (SelectedHouse != null)
        {
            SelectedHouse.OnCleanComplete.RemoveListener(SetCleanedOneHouse);
            StartCoroutine(SelectedHouse.EndCleaningMode(false, 0f));
            SelectedHouse = null;
        }
        GetComponent<PlayerInput>().DeactivateInput();
    }

    private void SetCleanedOneHouse()
    {
        if (_hasCleanedOneHouse || !TutorialManager.Instance.HasExplainedTutorial(TutorialManager.TutorialType.Cleaning))
            return;
        TutorialManager.Instance.StartTutorial(TutorialManager.TutorialType.Assigning);
        _hasCleanedOneHouse = true;
    }

    // INPUT
    //--------------------------------------------------
    private void SetInputMain()
    {
        var input = GetComponent<PlayerInput>();
        input.ActivateInput();
        input.SwitchCurrentActionMap("MainInput");
    }
    public void SetInputCleaning()
    {
        var input = GetComponent<PlayerInput>();
        input.ActivateInput();
        input.SwitchCurrentActionMap("HouseCleaning");
    }


    // PLAYER FEEDBACK
    public void PlayCleaningCompleteFeedback()
    {
        OnCleaningComplete?.Invoke();
        if (_animator != null)
        {
            _animator.SetTrigger("cheerTrigger");
        }
    }

    public void PlayCleaningFailedFeedback()
    {
        OnCleaningFailed?.Invoke();
        if (_animator != null)
        {
            _animator.SetTrigger("failTrigger");
        }
    }
}