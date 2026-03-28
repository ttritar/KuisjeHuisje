using UnityEngine;
using UnityEngine.Events;

public class ItemInteractor : MonoBehaviour
{
    [SerializeField] private Transform _holdSocket;
    public ItemBehaviour Item => _heldItem;
    private ItemBehaviour _heldItem;

    public UnityEvent OnItemPickedUp = new();
    public UnityEvent OnItemPickedUpOutline = new();
    public UnityEvent OnItemGiven = new();

	ItemPlacementBehaviour[] _itemPlacementBehaviour;


	private void Start()
    {
        _itemPlacementBehaviour = FindObjectsByType<ItemPlacementBehaviour>(FindObjectsSortMode.InstanceID);
	}
    // BEHAVIOUR
    //--------------------------------------------------
    public void PlaceItem(Transform placement)
    {
        if (!_heldItem)
            return;
        _heldItem.IsPickedUp = true;

        _heldItem.transform.SetParent(placement, false);
        ApplyLayer("Item", _heldItem.gameObject);

        DisableColliders(_heldItem.gameObject);

        _heldItem = null;
        OnItemGiven.Invoke();
	}
    public void PickUpItem(ItemBehaviour value)
    {
        var spawner = value.GetComponent<SpawnedBehaviour>().Spawner;
        if (_heldItem)
        {
            _heldItem.OnDropped?.Invoke();
            spawner.SwapItem(_heldItem.gameObject);
            _heldItem.IsPickedUp = false;
            ApplyLayer("Item", _heldItem.gameObject);
        }
        else spawner.ClearSpawner();

        _heldItem = value;
        _heldItem.IsPickedUp = true;
        if (_heldItem == null)
            return;
        _heldItem.transform.SetParent(_holdSocket, false);
        _heldItem.transform.localPosition = Vector3.zero;
        _heldItem.transform.localRotation = Quaternion.identity;
        FileLogger.Instance.LogItemPickup(_heldItem);

        ApplyLayer("Player", _heldItem.gameObject);

        _heldItem.OnPickedUp?.Invoke();
		OnItemPickedUp.Invoke();

		if (_heldItem.ItemType != ItemBehaviour.Type.DeadFish)
            OnItemPickedUpOutline.Invoke();
		Debug.Log("Outline");
	}

    public void GiveItem(CharacterInteractBehaviour character)
    {
        if(!_heldItem)
            return;
        _heldItem.IsPickedUp = true;
        FileLogger.Instance.LogItemGifting(_heldItem, character.GetComponent<CharacterData>());

        DisableColliders(_heldItem.gameObject);

        character.ReceiveItem(_heldItem);

        OnItemGiven.Invoke();

        _heldItem = null;
	}

    // HELPER
    //--------------------------------------------------
    private void DisableColliders(GameObject obj)
    {
        var colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var coll in colliders)
            coll.enabled = false;
    }
    private void ApplyLayer(string layerName, GameObject obj)
    {
        int layer = LayerMask.GetMask(layerName);
        layer = Mathf.RoundToInt(Mathf.Log(layer, 2));
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var render in renderers)
            render.gameObject.layer = layer;
    }
}
