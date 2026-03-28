using UnityEngine;

public class InteractablePlayer : IInteractable
{
    [SerializeField] private Animator _animator;
    [SerializeField] private int _numberOfInteractions = 3;
    public bool CanInteract(GameObject interactor)
    {
        return true;
    }
    public void Interact(GameObject interactor)
    {
        int x = Random.Range(1, _numberOfInteractions + 1);
        _animator.SetTrigger($"interactionTrigger{x}");
    }
}
