using UnityEngine;
using UnityEngine.Events;

public class HouseCleaningObject : MonoBehaviour, IInteractable
{
    public SpawnerBehaviour Spawner { get; set; }
    public bool IsDirty => _isDirty;
    [SerializeField] private bool _isDirty = false;
    [SerializeField] private float _lifetime = 2f;
    private float _timer = 0.0f;
    private bool _clicked = false;

    [Header("Player Feedback")]
    [SerializeField] bool _shrinkOverTime = true;
    [SerializeField] private GameObject _mesh;
    [SerializeField] GameObject _clickEffectPrefab;

    [Header("Events")]
    [SerializeField] public static UnityEvent<HouseCleaningObject> OnAnyClicked = new();
    [SerializeField] public static UnityEvent<HouseCleaningObject> OnAnyMissed = new();
    [SerializeField] public UnityEvent OnClicked = new();
    [SerializeField] public UnityEvent OnMissed = new();

    // START
    //--------------------------------------------------
    private void Start()
    {
        Spawner = GetComponentInParent<SpawnerBehaviour>();
    }

    // LOOP
    //--------------------------------------------------
    private void Update()
    {
        if (_clicked)
            return;

        _timer += Time.deltaTime;
        if (_shrinkOverTime)
            UpdateScaleBasedOnLifetime();

        if (_timer >= _lifetime)
        {
            HandleMissed();
        }
    }

    // INTERACTABLE
    //--------------------------------------------------
    public void Interact(GameObject interactor)
    {
        if (_clicked) return;

        _clicked = true;
        OnClicked.Invoke();
        OnAnyClicked.Invoke(this);

        if (Spawner != null)
            Spawner.ClearSpawner();

        //--- Player Feedback
        SpawnClickEffect();

        gameObject.SetActive(false);
        Destroy(gameObject,0.2f);
    }
    public bool CanInteract(GameObject interactor) => true;


    // FUNCTIONALITY
    //--------------------------------------------------
    private void HandleMissed()
    {
        if (_clicked) return;

        OnMissed.Invoke();
        OnAnyMissed.Invoke(this);

        if (Spawner != null)
            Spawner.ClearSpawner();

        Destroy(gameObject);
    }


    // PLAYER FEEDBACK
    //--------------------------------------------------
    private void UpdateScaleBasedOnLifetime()
    {
        float lifeProgress = _timer / _lifetime;
        float scale = Mathf.Lerp(1f, 0f, lifeProgress);
        _mesh.transform.localScale = new Vector3(scale, scale, _mesh.transform.localScale.z);
    }

    private void SpawnClickEffect()
    {
        if (_clickEffectPrefab != null)
        {
            var obj = Instantiate(_clickEffectPrefab, transform.position, Quaternion.identity);
            obj.transform.parent = null;
            Destroy(obj,5.0f);
        }
    }
}
