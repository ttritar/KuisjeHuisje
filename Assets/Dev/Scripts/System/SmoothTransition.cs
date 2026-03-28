using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class SmoothTransition : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private GameObject _effect;
    private ParticleSystem _effectSystem;

    [Header("Spline")]
    [SerializeField] private SplineAnimate _splineAnimate;
    [SerializeField] private GameObject _meshToRotate;

    public UnityEvent OnTransitionComplete = new();
    private bool _isTransitioning = false;
    public bool IsTransitioning => _isTransitioning;

    // START
    //--------------------------------------------------
    private void Awake()
    {
        if (_splineAnimate == null)
            _splineAnimate = GetComponent<SplineAnimate>();
        _effectSystem = _effect.GetComponentInChildren<ParticleSystem>();
    }

    // FUNCTIONALITY
    //--------------------------------------------------
    public void TransitionToPositionWalk(Transform dst, float dur = 1f, bool doEffect = false)
    {
        StopAllCoroutines();
        _isTransitioning = false;
        StartCoroutine(TransitionCoroutine(dst, dur, doEffect));
    }
    public void TransitionToSpline(SplineContainer targetSpline, float? desiredT = null, float duration = 1f)
    {
        TransitionToSpline(targetSpline, _splineAnimate.Loop, desiredT, duration, false);
    }
    public void TransitionToSpline(SplineContainer targetSpline, SplineAnimate.LoopMode mode, float? desiredT = null, float duration = 1f, bool doEffect = false)
    {
        StopAllCoroutines();
        _isTransitioning = false;

        StartCoroutine(_splineAnimate.Container
            ? TransitionCoroutineSpline(targetSpline, mode, desiredT, duration, doEffect)
            : TransitionCoroutineFromPos(targetSpline, mode, desiredT, duration, doEffect));
    }


    // HELPER
    //--------------------------------------------------
    private IEnumerator TransitionCoroutineFromPos(SplineContainer targetSpline, SplineAnimate.LoopMode mode, float? desiredT, float duration, bool doEffect)
    {
        if ((!_splineAnimate || !targetSpline) || (doEffect && !_effect))
        {
            _isTransitioning = false;
            OnTransitionComplete.Invoke();
            yield break;
        }

        _isTransitioning = true;

        float targetT = desiredT.HasValue ? Mathf.Clamp01(desiredT.Value) : 0f;

        Vector3 startPos = transform.position;
        Vector3 endPos = targetSpline.EvaluatePosition(targetT);

        Quaternion startRot = _meshToRotate.transform.rotation;

        Vector3 lastPos = transform.position;
        Quaternion lastRot = _meshToRotate.transform.rotation;

        if (doEffect)
        {
            var instanceEffect = InstantiateEffect(_effectSystem);
            instanceEffect.Play();
        }

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Clamp01(time / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, alpha);

            Vector3 newPos = Vector3.Lerp(startPos, endPos, smooth);
            transform.position = newPos;

            Vector3 forward = (newPos - lastPos).normalized;
            if (forward.sqrMagnitude > Mathf.Epsilon)
            {
                Quaternion targetRot = Quaternion.LookRotation(forward, transform.up);
                _meshToRotate.transform.rotation = Quaternion.Slerp(startRot, targetRot, smooth);
            }
            lastPos = newPos;

            yield return null;
        }

        _splineAnimate.Container = targetSpline;
        _splineAnimate.Loop = mode;
        _splineAnimate.NormalizedTime = targetT;

        if (doEffect)
        {
            var instanceEffect = InstantiateEffect(_effectSystem);
            instanceEffect.Play();
        }

        _isTransitioning = false;
        OnTransitionComplete.Invoke();
    }
    private IEnumerator TransitionCoroutineSpline(SplineContainer targetSpline, SplineAnimate.LoopMode mode, float? desiredT, float duration, bool doEffect)
    {
        if ((!_splineAnimate || !targetSpline) || (doEffect && !_effect))
        {
            _isTransitioning = false;
            OnTransitionComplete.Invoke();
            yield break;
        }

        _isTransitioning = true;

        SplineContainer startSpline = _splineAnimate.Container;
        float startT = _splineAnimate.NormalizedTime;
        float targetT = desiredT.HasValue ? Mathf.Clamp01(desiredT.Value) : startT;

        Vector3 lastPos = startSpline.EvaluatePosition(startT);
        Quaternion lastRot = _meshToRotate.transform.rotation;
        
        if (doEffect)
        {
            var instanceEffect = InstantiateEffect(_effectSystem);
            instanceEffect.Play();
        }

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Clamp01(time / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, alpha);

            Vector3 startPos = startSpline.EvaluatePosition(startT);
            Vector3 endPos = targetSpline.EvaluatePosition(targetT);
            Vector3 newPos = Vector3.Lerp(startPos, endPos, smooth);
            transform.position = newPos;

            Vector3 forward = (newPos - lastPos).normalized;
            Vector3 up = transform.up;
            if (forward.sqrMagnitude > Mathf.Epsilon)
            {
                Quaternion targetRot = Quaternion.LookRotation(forward, up);
                _meshToRotate.transform.rotation = Quaternion.Slerp(lastRot, targetRot, smooth);
                lastRot = _meshToRotate.transform.rotation;
            }

            lastPos = newPos;
            yield return null;
        }

        _splineAnimate.Container = targetSpline;
        _splineAnimate.Loop = mode;
        _splineAnimate.NormalizedTime = targetT;

        if (doEffect)
        {
            var instanceEffect = InstantiateEffect(_effectSystem);
            instanceEffect.Play();
        }

        _isTransitioning = false;
        OnTransitionComplete.Invoke();
    }
    private IEnumerator TransitionCoroutine(Transform target, float duration, bool doEffect)
    {
        if ((!_splineAnimate || !target) || (doEffect && !_effect))
        {
            _isTransitioning = false;
            OnTransitionComplete.Invoke();
            yield break;
        }

        _isTransitioning = true;

        SplineContainer startSpline = _splineAnimate.Container;
        float startT = _splineAnimate.NormalizedTime;

        Vector3 startPos = startSpline.EvaluatePosition(startT);
        Vector3 endPos = target.position;

        Vector3 lastPos = startPos;
        Quaternion lastRot = _meshToRotate.transform.rotation;

        if (doEffect)
        {
            var instanceEffect = InstantiateEffect(_effectSystem);
            instanceEffect.Play();
        }

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Clamp01(time / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, alpha);

            Vector3 newPos = Vector3.Lerp(startPos, endPos, smooth);
            transform.position = newPos;

            Vector3 forward = (newPos - lastPos).normalized;
            if (forward.sqrMagnitude > Mathf.Epsilon)
            {
                Quaternion targetRot = Quaternion.LookRotation(forward, transform.up);
                _meshToRotate.transform.rotation = Quaternion.Slerp(lastRot, targetRot, smooth);
                lastRot = _meshToRotate.transform.rotation;
            }

            lastPos = newPos;
            yield return null;
        }

        _splineAnimate.Container = null;
        _splineAnimate.NormalizedTime = 0f;

        if (doEffect)
        {
            var instanceEffect = InstantiateEffect(_effectSystem);
            instanceEffect.Play();
        }

        yield return StartCoroutine(ApplyRotation(target.rotation));

        _isTransitioning = false;
        OnTransitionComplete.Invoke();
    }
    private IEnumerator ApplyRotation(Quaternion rot, float duration = 1f)
    {
        var startRot = transform.rotation;
        var startRot2 = _meshToRotate.transform.localRotation;
        var targetRot = rot;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _meshToRotate.transform.localRotation = Quaternion.Slerp(startRot2, Quaternion.identity, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        transform.rotation = targetRot;
        _meshToRotate.transform.localRotation = Quaternion.identity;
    }
    private ParticleSystem InstantiateEffect(ParticleSystem effect)
    {
        var cam = Camera.main;
        var instance = Instantiate(effect, transform.position + transform.up - Vector3.forward, Quaternion.identity);
        instance.transform.localScale *= 2f;
        if (cam) instance.transform.LookAt(cam.transform.position);
        return instance;
    }
}
