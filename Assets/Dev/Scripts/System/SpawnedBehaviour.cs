using System;
using UnityEngine;

public class SpawnedBehaviour : MonoBehaviour
{   
    public SpawnerBehaviour Spawner => _spawner;
    private SpawnerBehaviour _spawner;

    // START
    //--------------------------------------------------
    private void Start()
    {
        SetSpawner(GetComponentInParent<SpawnerBehaviour>());
    }
    public void SetSpawner(SpawnerBehaviour spawner)
    {
        _spawner = spawner;
    }
}
