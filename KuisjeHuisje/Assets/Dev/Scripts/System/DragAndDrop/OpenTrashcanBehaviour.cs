using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class OpenTrashcanBehaviour : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    [Header("Settings")]
    [SerializeField] private float _timeout = 0.1f;
    private float _lastSeenTime = -1f;
    private bool _isOpen = false;

    [Header("Events")]
    [SerializeField] private UnityEvent OnTrashcanOpened;
    [SerializeField] private UnityEvent OnTrashcanClosed;

    private readonly HashSet<DragAndDropPayload> _payloadsInside = new HashSet<DragAndDropPayload>(); 
    private readonly Dictionary<DragAndDropPayload, UnityAction> _payloadDeleteListeners = new Dictionary<DragAndDropPayload, UnityAction>();


    // START & UPDATE
    //--------------------------------------------------
    private void Start()
    {
        if (!_animator) Debug.LogError("[OpenTrashcanBehaviour] No Animator assigned.", this);

        var col = GetComponent<Collider>();
        if (!col.isTrigger) Debug.LogWarning("[OpenTrashcanBehaviour] Collider should be set to 'isTrigger' for trigger callbacks to work.", this);
    }

    private void Update()
    {
        if (_payloadsInside.Count > 0)
        {
            var toRemove = new List<DragAndDropPayload>();
            bool anyDragging = false;

            foreach (var p in _payloadsInside)
            {
                if (p == null)
                {
                    toRemove.Add(p);
                    _lastSeenTime = Time.time;
                    continue;
                }
                if (p.IsBeingDragged)
                {
                    anyDragging = true;
                    break;
                }
            }
            foreach (var n in toRemove)
                _payloadsInside.Remove(n);

            if (anyDragging)
            {
                _lastSeenTime = Time.time;
                if (!_isOpen) OpenTrashcan();
                return;
            }
        }

        if (_isOpen && Time.time - _lastSeenTime > _timeout)
            CloseTrashcan();
    }


    // TRASHCAN STATE
    //--------------------------------------------------
    private void OpenTrashcan()
    {
        if (_isOpen) return;
        _isOpen = true;
        OnTrashcanOpened?.Invoke();
        if (_animator) _animator.SetBool("isOpen", true);
    }

    private void CloseTrashcan()
    {
        if (!_isOpen) return;
        _isOpen = false;
        OnTrashcanClosed?.Invoke();
        if (_animator) _animator.SetBool("isOpen", false);
    }


    // COLLISION
    //--------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Payload")) return;
        if (other.TryGetComponent<DragAndDropPayload>(out var payload))
        {
            _payloadsInside.Add(payload);

            if (!_payloadDeleteListeners.ContainsKey(payload))
            {
                UnityAction listener = () => OnPayloadDeletedBySelf(payload);
                payload.OnDeleted.AddListener(listener);
                _payloadDeleteListeners[payload] = listener;
            }

            if (payload.IsBeingDragged)
            {
                _lastSeenTime = Time.time;
                OpenTrashcan();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Payload")) return;
        if (other.TryGetComponent<DragAndDropPayload>(out var payload))
        {
            _payloadsInside.Remove(payload);
            if (_payloadDeleteListeners.TryGetValue(payload, out var listener))
            {
                payload.OnDeleted.RemoveListener(listener);
                _payloadDeleteListeners.Remove(payload);
            }

            if (_payloadsInside.Count == 0)
                _lastSeenTime = Time.time;
        }
    }

    private void OnPayloadDeletedBySelf(DragAndDropPayload payload)
    {
        if (payload != null && _payloadsInside.Contains(payload))
            _payloadsInside.Remove(payload);

        if (_payloadDeleteListeners.TryGetValue(payload, out var listener))
            _payloadDeleteListeners.Remove(payload);

        _lastSeenTime = Time.time;

        if (_payloadsInside.Count == 0 && _isOpen)
            CloseTrashcan();
    }

}
