using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private float _interactRange = 100f;
    [SerializeField] private LayerMask _layerMask;

    public UnityEvent OnInteractionStarted = new();


    // INTERACTION
    //--------------------------------------------------
    public bool CanInteract()
    {
        if (EventSystem.current == null /*&& EventSystem.current.IsPointerOverGameObject()*/)
            return false;
        if (WorldSwitchManager.Instance.IsTransitioning || !(GetComponent<PotionThrower>()?.CanThrowPotion ?? true))
            return false;
        if (TutorialManager.Instance.IsTutorialRunning)
            return false;
        return true;
    }
    public void TryInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || ctx.canceled)
            return;
        if (!CanInteract())
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
        interactable.Interact(gameObject);
        OnInteractionStarted.Invoke();
        Debug.Log("Interacted with: " + interactable);
    }
}