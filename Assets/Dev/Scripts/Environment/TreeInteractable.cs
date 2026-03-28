using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

public class TreeInteractable : MonoBehaviour, IInteractable
{
    [Header("Player Feedback")]
    [SerializeField] private RandomEffect _randomEffect;
    [SerializeField] private ParticleSystem _particleSystem;

    [Header("Events")]
    [SerializeField] private UnityEvent OnInteracted;


    // INTERACT
    //--------------------------------
    public void Interact(GameObject interactor)
    {
        if(!CanInteract(interactor))
            return;

        // feedback
        _randomEffect?.Play();
        _particleSystem?.Play();

        OnInteracted?.Invoke();
    }

    public bool CanInteract(GameObject interactor)
    {
        return true;
    }

}
