using UnityEngine;
using UnityEngine.Events;

public class ItemBehaviour : MonoBehaviour, IInteractable
{
    [System.Flags]
    public enum Type
    {
        None = 0,
        Flower = 1 << 0,
        WiltedFlower = 1 << 1,
        DeadFish = 1 << 2,
    }

    public bool IsGoodGift => _isGoodGift;
    [SerializeField] private bool _isGoodGift = true;
    [SerializeField] private Type _type;
    public Type ItemType => _type;

    public bool IsPickedUp { get; set; } = false;


    [Header("Events")]
    [SerializeField] public UnityEvent OnPickedUp = new();
    [SerializeField] public UnityEvent OnDropped = new();

    // INTERACTION
    //--------------------------------------------------
    public void Interact(GameObject interactor)
    {
        if (!interactor.TryGetComponent(out ItemInteractor itemInteractor))
            return;
        itemInteractor.PickUpItem(this);

    }

    public bool CanInteract(GameObject interactor)
    {
        if(IsPickedUp) return false;
        if (!interactor.TryGetComponent(out ItemInteractor itemInteractor)) 
            return false;
        return true;
    }
}
