using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LightningSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _lightningSphere;

    private float _spawnInterval = 10f;
    [SerializeField] private float _spawnIntervalMinimum = 5f;
    [SerializeField] private float _spawnIntervalMaximum = 15f;
    private float _duration = 0.5f;
    [SerializeField] private float _durationMinimum = 0.5f;
    [SerializeField] private float _durationMaximum = 1.5f;

    [Header("SFX")]
    [SerializeField] private RandomEffect _thunderSFX;
    [SerializeField] private float _thunderDelayMinimum = 0.1f;
    [SerializeField] private float _thunderDelayMaximum = 2.0f;
    private float _thunderDelay = 1.0f;


    // UPDATE
    //--------------------------------------------------
    private void Update()
    {
        _spawnInterval -= Time.deltaTime;
        if (_spawnInterval <= 0f)
        {
            StartCoroutine(SpawnLightning(_duration, _thunderDelay));
            _spawnInterval = UnityEngine.Random.Range(_spawnIntervalMinimum, _spawnIntervalMaximum);
            _duration = UnityEngine.Random.Range(_durationMinimum, _durationMaximum);
            _thunderDelay = UnityEngine.Random.Range(_thunderDelayMinimum, _thunderDelayMaximum);
        }
    }

    // LIGHTNING
    //--------------------------------------------------
    private IEnumerator SpawnLightning(float duration, float sfxDelay)
    {
        _lightningSphere.SetActive(true);
        yield return new WaitForSeconds(duration);
        _lightningSphere.SetActive(false);

        yield return new WaitForSeconds(sfxDelay);
        PlaySFX();
    }
    private void PlaySFX()
    {
        if (_thunderSFX != null)
        {
            _thunderSFX.Play();
        }
    }
}
