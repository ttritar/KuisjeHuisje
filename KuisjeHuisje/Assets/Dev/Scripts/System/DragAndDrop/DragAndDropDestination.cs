using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class DragAndDropDestination : MonoBehaviour
{
    [SerializeField] private bool _acceptPayloads = false;
    public bool AcceptPayloads => _acceptPayloads;

    private bool _hasPayload = false;

    [Tooltip("Only accept payloads with this ID.")]
    [SerializeField] private string _acceptedPayloadId = "";
    public string AcceptedPayloadId => _acceptedPayloadId;

    [SerializeField] [CanBeNull] private DragAndDropAttractor _attractor; 
    private DragAndDropPayload _payloadInZone;

    [Header("Events")]
    [SerializeField] public UnityEvent<GameObject, string> OnPayloadReceived = new();


    // FUNCTIONALITY
    //--------------------------------------------------
    public void OnTriggerEnter(Collider other)
    {
        if (!_hasPayload && _acceptPayloads && other.CompareTag("Payload") && other.TryGetComponent(out DragAndDropPayload payload))
        {
            if (IsIdAccepted(payload.PayloadId))
                _payloadInZone = payload;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out DragAndDropPayload payload))
        {
            if (_payloadInZone == payload)
                _payloadInZone = null;
            
        }
    }

    private void Update()
    {
        if (_payloadInZone && !_payloadInZone.IsBeingDragged)
        {
            AcceptPayload(_payloadInZone);
            _payloadInZone = null;
        }
    }


    // HELPERS
    //--------------------------------------------------
    private bool IsIdAccepted(string id)
    {
        return id == _acceptedPayloadId || _acceptedPayloadId == String.Empty;
    }

    public void SetAcceptPayloads(bool value)
    {
        _acceptPayloads = value;
    }

    private void AcceptPayload(DragAndDropPayload payload)
    {
        OnPayloadReceived?.Invoke(payload.gameObject, payload.PayloadMessage);

        payload.transform.parent = transform.parent;
        payload.CurrentDestination = this;

        _acceptPayloads = false;
        _hasPayload = true;
    }


    public void ClearCurrentPayload(DragAndDropPayload payload)
    {
        // payload reset
        payload.CurrentDestination = null;
        payload.transform.parent = null;
        
        // destination reset
        _acceptPayloads = true;
        _hasPayload = false;

        // attraction reset
        if (_attractor)
        {
            _attractor.gameObject.SetActive(true);
            _attractor.enabled = true;
            _attractor.UseSnapping = true;
        }

    }
}
