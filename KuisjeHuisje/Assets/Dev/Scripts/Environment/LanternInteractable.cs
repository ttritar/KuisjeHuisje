using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

public class LanternInteractable : MonoBehaviour, IInteractable
{
    [Header("Player Feedback")]
    [SerializeField] private RandomEffect _randomEffect;
    [SerializeField] private Animator _animator;

    [Header("Events")]
    [SerializeField] private UnityEvent OnInteracted;


    // INTERACT
    //--------------------------------
    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        // feedback
        _randomEffect?.Play();
        _animator?.SetTrigger("swingTrigger");

        OnInteracted?.Invoke();
    }

    public bool CanInteract(GameObject interactor)
    {
        return true;
    }

}