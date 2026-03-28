using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class DragAndDropAttractor : MonoBehaviour
{
    public string AttractorId => _attractorId;
    [SerializeField] private string _attractorId = "";

    [Header("Snapping")]
    public bool UseSnapping
    {
        get => _useSnapping;
        set => _useSnapping = value;
    }

    [SerializeField] private bool _useSnapping = true;
    [SerializeField] private float _snapDuration = 0.25f;
    private float _snapTimer = 0f;
    private Vector3 _snapStartPos;
    private bool _isSnapping = false;

    [SerializeField] private GameObject _stickerRoot;
    private DragAndDropPayload _thisPayload = null;


    private DragAndDropPayload _currentSnappingPayload = null;
    private Collider[] _allPayloadColliders = null;

    [SerializeField] private bool _disableColliderAfter = false;

    [SerializeField] public UnityEvent OnSnapEnd = new();


    // START
    //--------------------------------------------------
    private void Start()
    {
        if(_stickerRoot)
        {
            _thisPayload = _stickerRoot.GetComponent<DragAndDropPayload>();
            if (!_thisPayload)
                Debug.LogError($"[DragAndDropAttractor] No DragAndDropPayload found in {_stickerRoot.name}");
        }
    }

    // LOOP
    //--------------------------------------------------
    private void LateUpdate()
    {
        UpdateSnapping();
    }

    // SNAPPING
    //--------------------------------------------------
    public void BeginSnapping(DragAndDropPayload payload)
    {
        // early exits
        if (!_useSnapping) return;
        if (!payload) return;

        if(_thisPayload)
            _thisPayload.CanMove = false;

        _isSnapping = true;

        _currentSnappingPayload = payload;
        _currentSnappingPayload.LastSnappedAttractor = this;
        _currentSnappingPayload.IsSnapping = true;
        _currentSnappingPayload.transform.SetParent(transform);
        _currentSnappingPayload.CanMove = false;

        //_allPayloadColliders = _currentSnappingPayload.GetComponentsInChildren<Collider>(true);
        //foreach (var col in _allPayloadColliders)
        //    col.enabled = false;


        // movement
        _snapTimer = 0f;
        _snapStartPos = _currentSnappingPayload.transform.position;


        //-- player feedback?
    }
    private void UpdateSnapping()
    {
        if (!_useSnapping || !_isSnapping || !_currentSnappingPayload) return;


        _snapTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_snapTimer / _snapDuration);

        float eased = Mathf.SmoothStep(0f, 1f, t);

        _currentSnappingPayload.transform.position = Vector3.Lerp(
            _snapStartPos,
            transform.position,
            eased
        );

        if (t >= 1f)
        {
            EndSnapping();
        }
    }
    private void EndSnapping()
    {
        if (!_useSnapping) return;

        _isSnapping = false;
        _useSnapping = false;

        _currentSnappingPayload.IsSnapping = false;

        if(_thisPayload)
            _thisPayload.CanMove = true;
        if(!_disableColliderAfter)
            _currentSnappingPayload.CanMove = true;

        //StartCoroutine(FinalizeSnapNextFrame());


        // player feedback?
    }
    private IEnumerator FinalizeSnapNextFrame()
    {
        yield return new WaitForEndOfFrame();

        foreach (var col in _allPayloadColliders)
            col.enabled = !_disableColliderAfter;
    }

    private IEnumerator DelayedDisableCollider()
    {
        yield return new WaitForEndOfFrame();

        if (_allPayloadColliders.Length > 0)
        {
            if (_disableColliderAfter)
                foreach (var col in _allPayloadColliders)
                    col.enabled = false;
        }
    }
    public void ResetAttractor()
    {
        gameObject.SetActive(true);
        enabled = true;

        _useSnapping = true;
        _allPayloadColliders = null;
        _currentSnappingPayload = null;

        GetComponent<Collider>().enabled = true;
    }
    
}
