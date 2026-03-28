using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideInTransition : MonoBehaviour
{
    [SerializeField] private Vector3 _direction = new Vector3 (1,0,0);
    [SerializeField] private float _distance = 100f;
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private bool _playOnEnable = false;

    private Vector3 _startPosition;
    private Vector3 _hiddenPosition;
    private Coroutine _currentCoroutine;

    private void Awake()
    {
        _startPosition = transform.localPosition;
        _hiddenPosition = _startPosition + (-_direction.normalized) * _distance;
        transform.localPosition = _hiddenPosition;
    }
    private void OnEnable()
    {
        if (!_playOnEnable) return;
        SlideIn();
    }
    private void OnDisable()
    {
        if (!_playOnEnable) return;
        SlideOut();
    }

    public void SlideIn()
    {
        gameObject.SetActive(true);
        if (_currentCoroutine != null) 
            StopCoroutine(_currentCoroutine);
        if( this.gameObject.activeInHierarchy)
            _currentCoroutine = StartCoroutine(Slide(_hiddenPosition, _startPosition));
    }

    public void SlideOut()
    {
        // if active
        if (_currentCoroutine != null) 
            StopCoroutine(_currentCoroutine);
        if (this.gameObject.activeInHierarchy)
            _currentCoroutine = StartCoroutine(Slide(_startPosition, _hiddenPosition, true));
    }

    private IEnumerator Slide(Vector3 from, Vector3 to, bool disableAfter = false)
    {
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            transform.localPosition = Vector3.Lerp(from, to, elapsed / _duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = to;

        if (disableAfter)
            gameObject.SetActive(false);
    }

}
