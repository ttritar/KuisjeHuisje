using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnerManager : MonoBehaviour
{
    public List<SpawnerBehaviour> Spawners => _spawners;
    private List<SpawnerBehaviour> _spawners;

    [SerializeField] private int _spawnLimit = 5;
    private int _currentSpawned = 0;

    [SerializeField] private float _spawnInterval = 0f; 
    private float _spawnTimer = 0f;

    public GameObject LastSpawnedObject { get; set; } = null;


    // START
    //--------------------------------------------------
    private void Awake()
    {
        _spawners = new List<SpawnerBehaviour>(GetComponentsInChildren<SpawnerBehaviour>());

        if (_spawners == null)
            return;
        foreach (var spawner in _spawners)
        {
            spawner.OnCleared.AddListener(DecreaseSpawnedCount);
        }
    }
    private void OnDestroy()
    {
        if (_spawners == null)
            return;

        foreach (var spawner in _spawners)
        {
            if (spawner != null)
                spawner.OnCleared.RemoveListener(DecreaseSpawnedCount);
        }
    }

    // LOOP
    //--------------------------------------------------
    private void Update()
    {
        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0f)
        {
            TrySpawn();
            _spawnTimer = _spawnInterval;
        }
    }

    // FUNCTIONALITY
    //--------------------------------------------------
    public void TrySpawn()
    {
        if (_spawners == null || _spawners.Count == 0)
            return;
        if (_currentSpawned >= _spawnLimit) 
            return;

        int tries = _spawners.Count;
        while (tries-- > 0)
        {
            int randomIndex = Random.Range(0, _spawners.Count);
            SpawnerBehaviour spawner = _spawners[randomIndex];

            if (!spawner.IsOccupied)
            {
                spawner.Spawn(this);
                _currentSpawned++;
                return;
            }
        }
    }

    private void DecreaseSpawnedCount()
    {
        _currentSpawned = Mathf.Max(0, _currentSpawned - 1);
    }

    public void ClearAllSpawners()
    {
        if (_spawners == null) return;

        foreach (var spawner in _spawners)
        {
            if (spawner != null)
                spawner.ClearSpawner();
        }

        _currentSpawned = 0;
    }

    public void DestroyAllSpawnedObjects()
    {
        if (_spawners == null) return;

        foreach (var spawner in _spawners)
        {
            if (spawner == null) continue;
            spawner.DestroySpawnedObject();
        }
        _currentSpawned = 0;
    }
}
