using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemPlacementBehaviour : MonoBehaviour, IInteractable
{
    // Beh
    [SerializeField] private ItemBehaviour.Type _typeMask;
    [SerializeField] private Transform _spawnPos;
    [HideInInspector] public ItemBehaviour Item;

    [Header("Events")]
    [SerializeField] public UnityEvent OnItemPlaced = new();


    // INTERACTION
    //--------------------------------------------------
    public void Interact(GameObject interactor)
    {
        var itemInteractor = interactor.GetComponent<ItemInteractor>();
        if (!itemInteractor || !itemInteractor.Item)
            return;

        Item = itemInteractor.Item;
        itemInteractor.PlaceItem(_spawnPos);
        OnItemPlaced?.Invoke();
    }

    public bool CanInteract(GameObject interactor)
    {
        if (Item) return false;

        var itemInteractor = interactor.GetComponent<ItemInteractor>();
        if (!itemInteractor || !itemInteractor.Item)
            return false;

        return (_typeMask & itemInteractor.Item.ItemType) != 0;
    }
}
