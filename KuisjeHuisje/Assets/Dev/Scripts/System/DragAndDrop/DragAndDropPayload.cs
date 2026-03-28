using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider))]
public class DragAndDropPayload : MonoBehaviour
{
    [Header("Sticker Settings")]
    public bool DuplicateOnDrag
    {
        get => _duplicateOnDrag;
        set => _duplicateOnDrag = value;
    }
    [SerializeField] private bool _duplicateOnDrag = true;
    [Tooltip("Null if false")][SerializeField] private Transform _reparent;

    public bool CanMove { get; set; } = true;

    [Header("Payload Data")]
    public string PayloadId => _payloadId;
    [SerializeField] private string _payloadId = "";
    public string PayloadMessage
    {
        get => _payloadMessage;
        set => _payloadMessage = value;
    }
    [SerializeField] private string _payloadMessage = "";

    [SerializeField] private bool _discardOnWrongDrop = true;
    public bool IsBeingDragged => _isBeingDragged;
    private bool _isBeingDragged = false;
    private Vector3 _dragOffset;
    private Plane _dragPlane;
    public DragAndDropDestination CurrentDestination { get; set; } = null;

    [Header("Attraction Settings")]
    public bool IsSnapping { get; set; } = false;
    [SerializeField] private bool _enableAttraction = true;
    [SerializeField] private string _attractorTag = "PayloadAttractor";
    private List<DragAndDropAttractor> _currentAttractors = new();
    private DragAndDropAttractor _closestAttractor = null;
    public DragAndDropAttractor LastSnappedAttractor { get => _lastSnappedAttractor; set => _lastSnappedAttractor = value; }
    private DragAndDropAttractor _lastSnappedAttractor = null;


    [Header("Delete Settings")] 
    private bool _canBeDeleted = false;
    [SerializeField] private string _deletorTag = "PayloadDeletor";
    private bool _isToBeDeleted = false;

    [Header("Scale Settings")]
    [SerializeField] private bool _scaleOnDrag = true;
    [SerializeField] private float _dragScale = 2.0f;
    [SerializeField] private float _scaleSpeed = 10f;
    private float _pickUpOffsetY = 0.1f;
    [SerializeField] private Vector3 _scaleAxis = new Vector3(1.0f, 0.0f, 1.0f);
    private Vector3 _originalScale;
    [SerializeField] private bool _scaleOnDrop = true;
    [SerializeField] private float _dropScale = 1.0f;
    private Coroutine _scaleRoutine;



    [Header("Rotation Settings")]
    [SerializeField] private bool _rotateOnDrag = false;
    [SerializeField] private Vector3 _rotationEulerOnDrag = new Vector3(0f, 0f, 0f);
    [SerializeField] private bool _randomRotationOnDrag = false;
    [SerializeField] private Vector3 _randomRotationAxis = new Vector3(0f, 1f, 0f);
    [SerializeField] private float _randomRotationMinAngle = -5f;
    [SerializeField] private float _randomRotationMaxAngle = 5f;



    [Header("Player Feedback")]
    // peel anim
    private Coroutine _peelCoroutine;
    [SerializeField] private Renderer _stickerRenderer;
    [SerializeField] private float _maxPeeling = 1.0f;
    [SerializeField] private float _peelTime = 0.2f;
    // to-be-deleted feedback
    [SerializeField] private GameObject _vfxToBeDeletedGameObject;
    private Coroutine _deleteVfxCoroutine;
    private Material _vfxToBeDeletedMaterial;
    [SerializeField] private float _vfxToBeDeletedDuration = 0.5f;
    private const string PROGRESS = "_Progress";
    [SerializeField] private RandomEffect _toBeDeletedSFX;
    [SerializeField] private GameObject _deleteFeedback;
    // wiggle
    [SerializeField] private StickerWiggle _stickerWiggleRelationship;
    [SerializeField] private StickerWiggle _stickerWiggleCharacter;
    // place VFX
    [SerializeField] private ParticleSystem _stickerPlaceVFX;


    [Header("Events")]
    private bool _hasBeenPickedUp = false;
    [SerializeField] public UnityEvent OnFirstPickUp = new();
    [SerializeField] public UnityEvent OnPickUp = new();
    [SerializeField] public UnityEvent OnDragHold = new();
    [SerializeField] public UnityEvent OnDrop = new();
    [SerializeField] public UnityEvent OnFirstDrop = new();
    private bool _hasBeenDropped = false;
    [SerializeField]public static event Action<DragAndDropPayload> OnPayloadCreated;
    [SerializeField] public UnityEvent OnDeleted = new();   

    private Collider _payloadCollider;
    private Camera _mainCamera;

	// INITIALIZATION
	//--------------------------------------------------
	private void InitializeReferences()
    {
        if (_payloadCollider == null)
            _payloadCollider = GetComponent<Collider>();

        if (_mainCamera == null)
            _mainCamera = Camera.main;

    }
    private void Start()
    {
        InitializeReferences();

        if (_duplicateOnDrag)
            OnPickUp.AddListener(this.DuplicateObject);


        _originalScale = transform.localScale;

        OnPayloadCreated?.Invoke(this);


        // Setup VFX events
        if (_stickerPlaceVFX != null)
        {
            OnDrop.AddListener(PlayPlaceVFX);
        }
    }
    private void OnDestroy()
    {
        if (_duplicateOnDrag)
            OnPickUp.RemoveListener(this.DuplicateObject);

	}


    // STICKER MANAGEMENT
    //--------------------------------------------------
    private void DuplicateObject()
    {
        GameObject duplicate = Instantiate(this.gameObject, this.transform.parent);
        if(_reparent) transform.SetParent(_reparent, true);
        DragAndDropPayload payload = duplicate.GetComponent<DragAndDropPayload>();
        if (payload != null)
        {
            payload._hasBeenPickedUp = true;

            payload._isBeingDragged = false;
            payload.enabled = true;

            payload.InitializeReferences();
            OnPayloadCreated?.Invoke(payload);
            payload._payloadCollider.enabled = true;

            payload._originalScale = this._originalScale;
            payload.transform.localScale = this._originalScale;
            payload.transform.rotation = this.transform.rotation;

            if(_stickerWiggleRelationship)
                payload._stickerWiggleRelationship.enabled = true;
            if(_stickerWiggleCharacter)
                payload._stickerWiggleCharacter.enabled = false;
        }


        // own stuff
        if (_duplicateOnDrag)
            OnPickUp.RemoveListener(this.DuplicateObject);

        _canBeDeleted = true;
    }
    private void DeletePayload()
    {
        if (IsSnapping) return;

        _deleteFeedback.transform.parent = null;
        Destroy(_deleteFeedback, 2.0f);
        OnDeleted.Invoke();
        this.gameObject.SetActive(false);

        //--- Player Feedback

        if (CurrentDestination)
        {
            CurrentDestination.gameObject.SetActive(true);

            if (CurrentDestination.TryGetComponent(out CharacterSticker sticker))
                sticker.OnPayloadDeleted(_payloadId); 

            CurrentDestination.ClearCurrentPayload(this);
        }

		// discard
		Destroy(this.gameObject);
    }


    // FUNCTIONALITY
    //--------------------------------------------------

    public void SetNewCollider(Collider newCollider)
    {
        _payloadCollider = newCollider;
    }

    //--- Snapping
    private void TrySnap()
    {
        if (!_enableAttraction)
        {
            if (_discardOnWrongDrop)
                DeletePayload();
            return;
        }

        UpdateClosestAttractor();

        if (_lastSnappedAttractor != null)
        {
            _lastSnappedAttractor.ResetAttractor();
            _lastSnappedAttractor.BeginSnapping(this);
        }
        else if (_closestAttractor != null){

            _closestAttractor.BeginSnapping(this);
        }
        else if (_discardOnWrongDrop)
        {
            DeletePayload();
        }
    }
    private IEnumerator DelayedTrySnap()
    {
        yield return new WaitForFixedUpdate();
        TrySnap();
    }

    //--- Scaling
    private IEnumerator ScaleTo(float targetScale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.Scale(_originalScale * targetScale, _scaleAxis)
                           + Vector3.Scale(_originalScale, Vector3.one - _scaleAxis);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * _scaleSpeed;
            elapsed = Mathf.Clamp01(elapsed);

            Vector3 lerped = Vector3.Lerp(startScale, endScale, elapsed);

            transform.localScale = Vector3.Scale(lerped, _scaleAxis)
                                   + Vector3.Scale(_originalScale, Vector3.one - _scaleAxis);

            yield return null;
        }

        transform.localScale = endScale;
    }

    //--- Rotation
    private void ApplyRotationOnDrag()
    {
        if (_rotateOnDrag)
        {
            transform.rotation = Quaternion.Euler(_rotationEulerOnDrag);
        }

        if (_randomRotationOnDrag)
        {
            float randomAngle = UnityEngine.Random.Range(_randomRotationMinAngle, _randomRotationMaxAngle);
            Vector3 randomRotation = _randomRotationAxis.normalized * randomAngle;
            transform.rotation = Quaternion.Euler(randomRotation) * transform.rotation;
        }
    }


    // INPUT
    //--------------------------------------------------
    public void BeginLeftDrag(Vector2 screenPos)
    {
        if (IsSnapping) return;
        if (!CanMove || EventSystem.current.IsPointerOverGameObject()) return; 
        _isBeingDragged = true; 
        //_payloadCollider.enabled = false;

        // find hit location
        _dragPlane = new Plane(_mainCamera.transform.forward * -1f, transform.position);
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        if (_dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter); 
            _dragOffset = transform.position - hitPoint;
            transform.position = hitPoint + _dragOffset;
        }


        //--- Events
        if (!_hasBeenPickedUp)
            OnFirstPickUp.Invoke();
        _hasBeenPickedUp = true;
        OnPickUp.Invoke();

        transform.position += new Vector3(0, _pickUpOffsetY, 0);
        _dragOffset += new Vector3(0, _pickUpOffsetY, 0);


        //--- Player Feedback
        // Scaling
        if (_scaleOnDrag)
        {
            if (_scaleRoutine != null)
                StopCoroutine(_scaleRoutine);
            StartCoroutine(ScaleTo(_dragScale));
        }

        // Rotation
        ApplyRotationOnDrag();

        // Peel
        if (_stickerRenderer != null)
        {
            if (_peelCoroutine != null)
                StopCoroutine(_peelCoroutine);
            _peelCoroutine = StartCoroutine(PlayPeel());
        }
    }
    public void UpdateLeftDrag(Vector2 screenPos)
    {
        if (IsSnapping) return;
        if (!CanMove || !_isBeingDragged) return;

        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        if (_dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            transform.position = hitPoint + _dragOffset;
        }

        OnDragHold.Invoke();


        //--- Player Feedback
    }
    public void EndLeftDrag()
    {
        if (IsSnapping) return;
        // delete if in deletor
        if (_isToBeDeleted)
        {
            DeletePayload();
            return; 
        }

        
        if (!_isBeingDragged) return;
        _isBeingDragged = false;

        _payloadCollider.enabled = true;

        // Snap to closest attractor if any
        if(enabled && gameObject.activeInHierarchy)
            StartCoroutine(DelayedTrySnap());


        if (!_hasBeenDropped)
            OnFirstDrop.Invoke();
        _hasBeenDropped = true;
        OnDrop.Invoke();

        transform.position -= new Vector3(0, _pickUpOffsetY, 0);
        _dragOffset -= new Vector3(0, _pickUpOffsetY, 0);

        //--- Player Feedback
        // Scaling
        if (_scaleOnDrop)
        {
            if (_scaleRoutine != null)
                StopCoroutine(_scaleRoutine);
            StartCoroutine(ScaleTo(_dropScale));
        }

        // Unpeel
        if (_stickerRenderer != null)
        {
            if (_peelCoroutine != null)
                StopCoroutine(_peelCoroutine);
            _peelCoroutine = StartCoroutine(PlayUnpeel());
        }
    }


    // COLLISION
    //--------------------------------------------------
    //--- Attractor
    private void UpdateClosestAttractor()
    {
        // early outs
        if (_currentAttractors.Count == 0)
        {
            _closestAttractor = null;
            return;
        }
        else if (_currentAttractors.Count == 1)
        {
            _closestAttractor = _currentAttractors[0];
            return;
        }

        foreach (var attractor in _currentAttractors)
        {
            // if the attractor has snapping disabled, skip
            if (!attractor.UseSnapping)
                continue;

            // if no closest yet, set and continue
            if (_closestAttractor == null)
            {
                _closestAttractor = attractor;
                continue;
            }

            // if its the same attractor obv also skip bc why would u not
            if (_closestAttractor == attractor)
                continue;

            // compare distances
            float currentDistance = Vector3.Distance(this.transform.position, _closestAttractor.transform.position);
            float newDistance = Vector3.Distance(this.transform.position, attractor.transform.position);
            if (newDistance < currentDistance)
            {
                _closestAttractor = attractor;
            }
        }

    }
    private void ClearAttractors()
    {
        _closestAttractor = null;
        _currentAttractors.Clear();
    }

    //--- Trigger
    private int _deletorOverlapCount = 0;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_deletorTag))
        {
            if (_canBeDeleted)
            {
                _deletorOverlapCount++;
                if (_deletorOverlapCount == 1)
                {
                    _isToBeDeleted = true;
                    _enableAttraction = false;
                    if (_isBeingDragged)
                        PlayOnToBeDeletedFeedback();
                }
            }
        }
        else if (other.CompareTag(_attractorTag))
        {
            if (!_enableAttraction) return;

            DragAndDropAttractor attractor = other.GetComponent<DragAndDropAttractor>();
            if (attractor == null) return;

            if (!attractor.UseSnapping) return;

            if (attractor.AttractorId == _payloadId || attractor.AttractorId == "")
            {
                _currentAttractors.Add(attractor);
            }
        }


    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_deletorTag))
        {
            if (_canBeDeleted)
            {
                _deletorOverlapCount--;
                if (_deletorOverlapCount <= 0)
                {
                    _deletorOverlapCount = 0;
                    _isToBeDeleted = false;
                    _enableAttraction = true;
                    ResetToBeDeletedProgress();
                }
            }
        }
        else if (other.CompareTag(_attractorTag))
        {
            if (!_enableAttraction) return;

            DragAndDropAttractor attractor = other.GetComponent<DragAndDropAttractor>();
            if (attractor == null) return;

            if (attractor.AttractorId == _payloadId || attractor.AttractorId == "")
            {
                _currentAttractors.Remove(attractor);
                if (_closestAttractor == attractor)
                    _closestAttractor = null;
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(_deletorTag)) return;
        if (_canBeDeleted) _isToBeDeleted = true;
    }

    // PLAYER FEEDBACK
    //--------------------------------------------------
    private void PlayOnToBeDeletedFeedback()
    {
        if (!_isToBeDeleted)
            return;

        _toBeDeletedSFX.Play();


        // effect  
        if (_deleteVfxCoroutine != null)
            StopCoroutine(_deleteVfxCoroutine);
        _deleteVfxCoroutine = StartCoroutine(AnimateToBeDeletedProgress());
    }
    private IEnumerator AnimateToBeDeletedProgress()
    {
        if (_vfxToBeDeletedGameObject == null)
        {
            Debug.LogWarning("[DragAndDropPayload] _vfxToBeDeletedGameObject is null.");
            yield break;
        }

        _vfxToBeDeletedGameObject.SetActive(true);

        // get renderer
        var rend = _vfxToBeDeletedGameObject.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning("[DragAndDropPayload] No Renderer found on _vfxToBeDeletedGameObject.");
            yield break;
        }

        _vfxToBeDeletedMaterial = new Material(rend.material);
        rend.material = _vfxToBeDeletedMaterial;

        // update
        _vfxToBeDeletedMaterial.SetFloat(PROGRESS, 0f);

        float t = 0f;
        while (t < Mathf.Max(0.0001f, _vfxToBeDeletedDuration))
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / _vfxToBeDeletedDuration);

            _vfxToBeDeletedMaterial.SetFloat(PROGRESS, normalized);

            yield return null;
        }

        _vfxToBeDeletedMaterial.SetFloat(PROGRESS, 1f);
    }
    private void ResetToBeDeletedProgress()
    {
        {
            if (_deleteVfxCoroutine != null)
            {
                StopCoroutine(_deleteVfxCoroutine);
                _deleteVfxCoroutine = null;
                _toBeDeletedSFX.Stop();
            }

            if (_vfxToBeDeletedMaterial != null)
            {
                _vfxToBeDeletedMaterial.SetFloat(PROGRESS, 0f);
            }
        }
    }

    private void PlayPlaceVFX()
    {
        if (_stickerPlaceVFX != null)
        {
            _stickerPlaceVFX.Play();
        }
    }

    private IEnumerator PlayPeel()
    {
        float t = 0f;
        while (t < Mathf.Max(0.0001f, _peelTime))
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / _peelTime);
            float peelAmount = Mathf.Lerp(0f, _maxPeeling, normalized);
            if (_stickerRenderer != null)
            {
                _stickerRenderer.material.SetFloat("_Peel", peelAmount);
            }
            yield return null;
        }
        if (_stickerRenderer != null)
        {
            _stickerRenderer.material.SetFloat("_Peel", _maxPeeling);
        }
    }
    private IEnumerator PlayUnpeel()
    {
        float t = 0f;
        while (t < Mathf.Max(0.0001f, _peelTime))
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / _peelTime);
            float peelAmount = Mathf.Lerp(_maxPeeling, 0f, normalized);
            if (_stickerRenderer != null)
            {
                _stickerRenderer.material.SetFloat("_Peel", peelAmount);
            }
            yield return null;
        }
        if (_stickerRenderer != null)
        {
            _stickerRenderer.material.SetFloat("_Peel", 0f);
        }
    }
}
