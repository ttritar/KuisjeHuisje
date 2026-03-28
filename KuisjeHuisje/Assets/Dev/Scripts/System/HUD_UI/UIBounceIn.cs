using System.Collections;
using UnityEngine;

public class UIBounceIn : MonoBehaviour
{
    [SerializeField] private float _bounceDuration = 0.5f;
    [SerializeField] private float _bounceScale = 1.2f;
    [SerializeField] private bool _useUnscaled = false;
    private Vector3 _originalScale;
    private float _elapsedTime = 0f;


    private void OnEnable()
    {
        _originalScale = transform.localScale;
        _elapsedTime = 0f;
        transform.localScale = Vector3.zero;
        StartCoroutine(BounceIn());
	}

    public IEnumerator BounceIn()
    {
        while (_elapsedTime < _bounceDuration)
        {
            _elapsedTime += _useUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = _elapsedTime / _bounceDuration;
            float scale = Mathf.Lerp(0f, _bounceScale, t);
            transform.localScale = _originalScale * scale;
            yield return null;
        }
        transform.localScale = _originalScale;
    }
}
