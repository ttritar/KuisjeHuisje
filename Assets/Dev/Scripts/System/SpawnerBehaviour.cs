using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[Serializable] public class SpawnPrefab
{
    [SerializeField] public GameObject prefab;
    [SerializeField] [Range(0f, 1f)] public float spawnChance = 1f;
    [SerializeField] public bool allowBackToBack = true;
}

public class SpawnerBehaviour : MonoBehaviour
{
    [SerializeField] private List<SpawnPrefab> _spawnPrefabs;
    private GameObject _spawnedObject;
    public bool IsOccupied => _isOccupied;
    private bool _isOccupied = false;

    [SerializeField] public UnityEvent OnCleared = new();

    [Header("PlayerFeedback")]
    // occupied sprite
    [SerializeField] private bool _useOccupiedSprite = false;
    [SerializeField] private GameObject _occupiedSprite;
    [SerializeField] private float _occupiedSpriteFloatFrequency = 2f;
    [SerializeField] private float _occupiedSpriteFloatAmplitude = 0.1f;
    private Vector3 _offsetPosition;


    // START
    //--------------------------------------------------
    private void Start()
    {
        _offsetPosition = _occupiedSprite != null ? _occupiedSprite.transform.localPosition - transform.localPosition : Vector3.zero;
    }

    // UPDATE
    //--------------------------------------------------
    private void Update()
    {
        if (_useOccupiedSprite)
            UpdateOccupiedSprite();
    }

    // FUNCTIONALITY
    //--------------------------------------------------
    public void Spawn(SpawnerManager spawner)
    {
        if (_isOccupied || _spawnPrefabs.Count == 0) return;

        List<(int index, float weight)> weighted = new();

        for (int i = 0; i < _spawnPrefabs.Count; i++)
        {
            var prefab = _spawnPrefabs[i];

            if (prefab.prefab == spawner.LastSpawnedObject && !prefab.allowBackToBack)
                continue;

            if (prefab.spawnChance > 0f)
                weighted.Add((i, prefab.spawnChance));
        }

        int selectedIndex = WeightedPick(weighted);

        var prefabToSpawn = _spawnPrefabs[selectedIndex].prefab;
        spawner.LastSpawnedObject = prefabToSpawn;

        _spawnedObject = Instantiate(prefabToSpawn, transform.position, transform.rotation);
        _spawnedObject.SetActive(true);
        _spawnedObject.transform.parent = this.transform;

        if (_spawnedObject.TryGetComponent(out BottleInteractBehaviour bib))
            bib.Spawner = this;

        _isOccupied = true;

        if (_useOccupiedSprite && _occupiedSprite != null)
            _occupiedSprite.SetActive(true);
    }
    private int WeightedPick(List<(int index, float weight)> list)
    {
        float total = 0f;
        foreach (var item in list)
            total += item.weight;

        float r = Random.value * total;

        foreach (var item in list)
        {
            if (r < item.weight)
                return item.index;
            r -= item.weight;
        }

        return list[^1].index;
    }

    public void ClearSpawner()
    {
        _spawnedObject = null;
        _isOccupied = false;
        OnCleared.Invoke();

        if (_useOccupiedSprite && _occupiedSprite != null)
            _occupiedSprite.SetActive(false);
    }
    public void DestroySpawnedObject()
    {
        Destroy(_spawnedObject);
        ClearSpawner();
    }

    public void SwapItem(GameObject obj)
    {
        var beh = obj.GetComponent<SpawnedBehaviour>();
        if (!_isOccupied || !beh)
            return;
        beh.SetSpawner(this);
        obj.transform.SetParent(_spawnedObject.transform.parent, false);
        _spawnedObject = obj;
        _spawnedObject.transform.position = transform.position;
        _spawnedObject.transform.rotation = transform.rotation;
    }

    // PLAYER FEEDBACK
    //--------------------------------------------------
    private void UpdateOccupiedSprite()
    {
        // sine wave
        if (_occupiedSprite != null && _isOccupied)
        {
            float newY = Mathf.Sin(Time.time * _occupiedSpriteFloatFrequency) * _occupiedSpriteFloatAmplitude;
            Vector3 newPosition = transform.localPosition + _offsetPosition + new Vector3(0f, newY, 0f);
            _occupiedSprite.transform.localPosition = newPosition;
        }
    }
}
