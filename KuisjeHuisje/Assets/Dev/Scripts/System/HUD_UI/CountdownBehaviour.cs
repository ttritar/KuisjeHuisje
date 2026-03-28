using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountdownBehaviour : MonoBehaviour
{
    [SerializeField] private float _countdownTime = 4.0f;
    [SerializeField] private List<GameObject> _countdownObjects = new List<GameObject>();
    private Coroutine _counterRoutine;

    // COUNTDOWN
    //--------------------------------------------------
    public void StopCountDown()
    {
        if(_counterRoutine != null)
            StopCoroutine(_counterRoutine);
        foreach (var obj in _countdownObjects)
            obj.SetActive(false);
    }
    public void StartCountdown()
    {
        _countdownTime = _countdownObjects.Count;
        _counterRoutine = StartCoroutine(CountdownCoroutine());
    }
    public void OnDisable()
    {
        StopCountDown();
    }

    private IEnumerator CountdownCoroutine()
    {
        if (_countdownObjects.Count == 0)
        {
            Debug.LogWarning("[CountdownBehaviour] No countdown objects assigned.");
            yield break;
        }


        float timePerObject = _countdownTime / _countdownObjects.Count;
        for (int i = _countdownObjects.Count - 1; i >= 0; i--)
        {
            _countdownObjects[i].SetActive(true);
            yield return new WaitForSeconds(timePerObject);
            _countdownObjects[i].SetActive(false);
        }
    }
}
