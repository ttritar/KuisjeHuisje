using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

[RequireComponent(typeof(CharacterInteractionManager))]
[RequireComponent(typeof(PlayerSplineMovement))]
[RequireComponent(typeof(PlayerInput))]
public class WizardInteractionManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _tOffset;
    [SerializeField] private float _rotationAngle = 45f;
    [SerializeField] private float _rotationDuration = .5f;

    [Header("UI")]
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private CinemachineCamera _interactionCamera;

    private WizardInteractBehaviour _wizard;
    private PlayerSplineMovement _playerSplineMovement;

    [Header("Events")]
    public UnityEvent OnStartInteraction = new();
    public UnityEvent OnStartInteractionLate = new();
    public UnityEvent OnEndInteraction = new();

    // START
    //--------------------------------------------------
    private void Awake()
    {
        _playerSplineMovement = GetComponent<PlayerSplineMovement>();
    }

    // INPUT
    //--------------------------------------------------
    public void StopCharacterInteraction(InputAction.CallbackContext ctx)
    {
        EndInteraction();
    }

    // INTERACTION
    //--------------------------------------------------
    public void EndInteraction()
    {
        var splineTransition = _wizard?.GetComponent<SmoothTransition>();
        if (_wizard == null ||
            (splineTransition?.IsTransitioning ?? false))
            return;
        splineTransition.TransitionToPositionWalk(_wizard.Spawn, 0.01f, true);

        _uiManager.EnableUI("Potion_UI");
        _uiManager.DisableUI("WizardInteraction_UI");
        OnEndInteraction.Invoke();
        _playerSplineMovement.AutoRotateY = true;
        _wizard = null;
        _interactionCamera.gameObject.SetActive(false);
        GetComponent<PlayerInput>().SwitchCurrentActionMap("MainInput");
    }

    public void StartInteraction(WizardInteractBehaviour other, bool isTutorial = false)
    {
        _interactionCamera.gameObject.SetActive(true);
        _wizard = other;
        _wizard.GetComponent<CharacterAISplineMovement>().DoIdle = false;

        if (isTutorial)
        {
            GetComponent<PlayerInput>()?.SwitchCurrentActionMap("CharacterInteraction");
            var ai = _wizard.GetComponent<CharacterAIManager>();
            if (ai != null)
                ai.FreezeAI();
        }

        _uiManager.DisableUI("Potion_UI");
        OnStartInteraction.Invoke();

        var splineTransition = _wizard.GetComponent<SmoothTransition>();
        if (!splineTransition)
            return;

        _playerSplineMovement.AutoRotateY = false;
        _playerSplineMovement.ForceSetDirection(-1);
        _playerSplineMovement.FullStop();
        var playerSpline = GetComponent<SplineAnimate>();
        float targetT = Mathf.Repeat(playerSpline.NormalizedTime + _tOffset, 1f);
        splineTransition.TransitionToSpline(playerSpline.Container, SplineAnimate.LoopMode.PingPong, targetT, 0.01f, true);

        StartCoroutine(_playerSplineMovement.RotateMesh(Quaternion.Euler(0f, 180f - _rotationAngle, 0f), _rotationDuration));
        splineTransition.OnTransitionComplete.AddListener(RotateInteractors);

        StartCoroutine(WaitForTransitionThenInvoke(splineTransition, isTutorial));
    }
    private IEnumerator WaitForTransitionThenInvoke(SmoothTransition splineTransition, bool isTutorial)
    {
        while (splineTransition.IsTransitioning)
            yield return null;

        if (!isTutorial)
            _uiManager.EnableUI("WizardInteraction_UI");
        OnStartInteractionLate.Invoke();
    }
    private void RotateInteractors()
    {
        var splineTransition = _wizard.GetComponent<SmoothTransition>();
        splineTransition?.OnTransitionComplete.RemoveListener(RotateInteractors);

        var interactorMesh = _wizard.GetComponent<CharacterAISplineMovement>().CharacterMesh;
        StartCoroutine(RotateInteractorSmooth(interactorMesh.transform, _rotationAngle, _rotationDuration));
    }
    private IEnumerator RotateInteractorSmooth(Transform interactorT, float yOffset, float duration)
    {
        var localEuler = interactorT.localEulerAngles;
        if (localEuler.y > 180f) localEuler.y -= 180f;

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
}
