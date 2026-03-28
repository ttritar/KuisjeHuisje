using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

[RequireComponent(typeof(Renderer))]
public class HouseCleaningBehaviour : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CinemachineCamera _houseCamera;
    [SerializeField] private LayerMask _occlusionMask;
    public LayerMask OcclusionMask => _occlusionMask;


    [Header("Cleaning Settings")]
    [SerializeField] private SpawnerManager _spawnerManager;
    [SerializeField] private int _circlesNeeded = 5;
    private int _circlesClicked = 0;
    public bool IsClean { get; private set; }
    private bool _isCleaning = false;
    private HouseCleaner _currentCleaner = null;

    [Header("Striking System")]
    public int MaxStrikeCount => _maxStrikeCount;
    [SerializeField] private int _maxStrikeCount = 3;
    private int _currentStrikes = 0;


    [Header("Player Feedback")]
    [SerializeField] private bool _resetScaleOnClean = true;
    [SerializeField] private Vector3 _scaleAxis = Vector3.up;
    [SerializeField][Range(0f, 1f)] private float _growScale = 0.02f;
    [SerializeField] private float _growTime = 0.05f;
    [SerializeField] private string _dirtinessProperty = "_Dirtiness";
    private Vector3 _originalScale;
    private bool _isPulsating = false;
    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    private bool _hasInitialized = false;


    [Header("Events")]
    public UnityEvent OnCleanStart = new();
    public UnityEvent OnCleanProgressed = new();
    public UnityEvent OnCleanRegressed = new();
    public UnityEvent OnCleanFailed = new();
    public UnityEvent OnCleanComplete = new();

    private Coroutine _spawnerRoutine;
    private HouseBehaviour _houseBehaviour;

    // START
    //--------------------------------------------------
    private void Awake()
    {
        if (!_hasInitialized)
            Initialize();
    }

    private void Initialize()
    {
        _houseBehaviour = GetComponent<HouseBehaviour>();
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
        _originalScale = transform.localScale;
        _renderer.SetPropertyBlock(_mpb);
        _mainCamera = Camera.main;
        _hasInitialized = true;
    }


    // CLEANING
    //--------------------------------------------------

    // MODES
    public void StartCleaningMode(HouseCleaner cleaner, float startSpawnDelay = 0f)
    {
        if (_isCleaning) 
            return;
        _isCleaning = true;
        _circlesClicked = 0;

        // set input
        _currentCleaner = cleaner;
        if (_currentCleaner)
        {
            _currentCleaner.SetInputCleaning();
            _currentCleaner.OnEnterCleaningMode.Invoke();
            if (!TutorialManager.Instance.HasExplainedTutorial(TutorialManager.TutorialType.Cleaning))
                _currentCleaner.OnEnterCleaningModeTutorial.Invoke();
        }

        _mainCamera.cullingMask &= ~_occlusionMask;
        _houseCamera.gameObject.SetActive(true);
        OnCleanStart.Invoke();
        _currentStrikes = 0;
        _currentCleaner?.OnStrikeReceived.Invoke(_currentStrikes, _maxStrikeCount);

        _spawnerRoutine = StartCoroutine(EnableSpawnerRoutine(cleaner, startSpawnDelay));
    }

    private IEnumerator EnableSpawnerRoutine(HouseCleaner cleaner, float spawnDelay)
    {
        if (spawnDelay > 0f)
            yield return new WaitForSeconds(spawnDelay);
        _spawnerManager.enabled = true;
        _spawnerManager.ClearAllSpawners();

        // subscribe to events
        HouseCleaningObject.OnAnyClicked.AddListener(HandleClick);
        HouseCleaningObject.OnAnyMissed.AddListener(HandleMissed);
    }

    public IEnumerator EndCleaningMode(bool success, float delay)
    {
        if (success)
        {
            IsClean = true;
            UpdateDirtiness();
            OnCleanComplete.Invoke();
            _currentCleaner?.PlayCleaningCompleteFeedback();
        }
        else
        {
            if(_spawnerRoutine != null)
                StopCoroutine(_spawnerRoutine);
            _spawnerManager.DestroyAllSpawnedObjects();
            _houseBehaviour.CountdownBehaviour.StopCountDown();
            if (!IsClean)
            {
                _circlesClicked = 0;
                UpdateDirtiness();
            }
        }

        _spawnerManager.enabled = false;
        _spawnerManager.ClearAllSpawners();

        _mainCamera.cullingMask |= _occlusionMask;
        yield return new WaitForSeconds(delay);

        if (!TutorialManager.Instance.HasExplainedTutorial(TutorialManager.TutorialType.Cleaning))
            yield break;

        _currentStrikes = 0;
        _currentCleaner?.OnStrikeReceived.Invoke(_currentStrikes, _maxStrikeCount);

        // unsubscribe from events
        HouseCleaningObject.OnAnyClicked.RemoveListener(HandleClick);
        HouseCleaningObject.OnAnyMissed.RemoveListener(HandleMissed);

        // reset input
        if (_currentCleaner)
        {
            _currentCleaner.OnExitCleaningMode.Invoke();
            _currentCleaner = null;
        }

        _houseCamera.gameObject.SetActive(false);
        _isCleaning = false;
    }

    public void SetClean()
    {
        if (!_hasInitialized)
            Initialize();
        IsClean = true;
        _circlesClicked = _circlesNeeded;
        if(gameObject.activeInHierarchy) StartCoroutine(EndCleaningMode(true, _currentCleaner?.ExitCleaningDelay ?? 0f));
        else UpdateDirtiness();
    }


    // HANDLE EVENTS
    private void HandleClick(HouseCleaningObject obj)
    {
        if(obj.IsDirty)
        {
            OnCleanRegressed.Invoke();
            HandleFailed();
        }
        else
            HandleCleaning();
        
    }
    private void HandleMissed(HouseCleaningObject obj)
    {
        if (!obj.IsDirty)
            HandleFailed();
    }


    // HANDLE CLEAN/FAIL
    private void HandleCleaning()
    {
        _circlesClicked++;
        TriggerPulse();

        OnCleanProgressed.Invoke();

        if (_circlesClicked >= _circlesNeeded) StartCoroutine(EndCleaningMode(true, _currentCleaner?.ExitCleaningDelay ?? 0f));
        else UpdateDirtiness();
    }

    private void HandleFailed()
    {
        // reset progress
        _circlesClicked = 0;
        _spawnerManager.ClearAllSpawners();
        OnCleanFailed.Invoke();
        if (!TutorialManager.Instance.IsTutorialRunning)
        {
            ++_currentStrikes;
            _currentCleaner?.OnStrikeReceived.Invoke(_currentStrikes, _maxStrikeCount);
        }

        if (_currentStrikes >= _maxStrikeCount)
        {
            StartCoroutine(EndCleaningMode(false, _currentCleaner?.ExitCleaningDelay ?? 0f));
            _currentCleaner?.PlayCleaningFailedFeedback();
        }
        else UpdateDirtiness();
    }

    // PLAYER FEEDBACK
    //--------------------------------------------------
    private void TriggerPulse()
    {
        if (_isPulsating && _resetScaleOnClean)
        {
            StopAllCoroutines();
            _isPulsating = false;
        }
        StartCoroutine(PulseScale());
    }

    private IEnumerator PulseScale()
    {
        _isPulsating = true;

        Vector3 start = _originalScale;

        Vector3 end = new Vector3(
            _originalScale.x * (1f + _growScale * _scaleAxis.x),
            _originalScale.y * (1f + _growScale * _scaleAxis.y),
            _originalScale.z * (1f + _growScale * _scaleAxis.z)
        );

        float elapsed = 0f;

        // grow
        while (elapsed < _growTime)
        {
            transform.localScale = Vector3.Lerp(start, end, elapsed / _growTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = end;

        // shrink
        elapsed = 0f;
        while (elapsed < _growTime)
        {
            transform.localScale = Vector3.Lerp(end, start, elapsed / _growTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = start;
        _isPulsating = false;
    }


    private void UpdateDirtiness()
    {
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(_dirtinessProperty, 1f - _circlesClicked / (float)_circlesNeeded);
        _renderer.SetPropertyBlock(_mpb);
    }
}
