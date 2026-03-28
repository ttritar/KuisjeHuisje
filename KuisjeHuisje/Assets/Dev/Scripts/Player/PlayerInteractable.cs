using UnityEngine;

public class PlayerInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _animationTriggerName = "interact";
    [SerializeField] private int _animationCount = 3;


    public void Interact(GameObject interactor)
    {
        if (_animator != null && !string.IsNullOrEmpty(_animationTriggerName))
        {
            int randomAnimationIndex = Random.Range(1, _animationCount + 1);
            _animator.SetTrigger(_animationTriggerName + randomAnimationIndex);
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return true;
    }
}
