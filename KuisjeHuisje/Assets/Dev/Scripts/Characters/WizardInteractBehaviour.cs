using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class WizardInteractBehaviour : CharacterInteractBehaviour
{
    public Transform Spawn =>_spawn;
    [SerializeField] private Transform _spawn;

    // AWAKE
    //--------------------------------------------------
    protected override void Awake()
    {
        base.Awake();
        transform.position = _spawn.position;
        transform.rotation = _spawn.rotation;
    }


    // INTERACTION
    //--------------------------------------------------
    public override void Interact(GameObject interactor)
    {
        interactor.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("CharacterInteraction");

        if (interactor.TryGetComponent(out WizardInteractionManager component))
            component.StartInteraction(this);

        _aiManager.FreezeAI();
    }

    public override bool CanInteract(GameObject interactor)
    {
        bool isTransitioning = GetComponent<SmoothTransition>()?.IsTransitioning ?? false;
        return !isTransitioning;
    }
}
