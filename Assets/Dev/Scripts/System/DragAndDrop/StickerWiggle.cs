using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickerWiggle : MonoBehaviour
{
    public StickerWiggleManager.State Id => _id;
    [SerializeField] private StickerWiggleManager.State _id;

    [Header("Wiggle Settings")]
    [SerializeField] private float _wiggleIntensity = 5f;
    [SerializeField] private float _wiggleDuration = 0.35f;

    private bool _isWiggling;
    private Coroutine _wiggleRoutine;
    private Quaternion _originalRotation;

    // START
    //------------------------------------------------
    private void Start()
    {
        StickerWiggleManager.Instance.RegisterSticker(this);
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
        StickerWiggleManager.Instance.DeregisterSticker(this);
    }

    // FUNCTIONALITY
    //------------------------------------------------
    public IEnumerator Wiggle()
    {
        if (_isWiggling)
            yield break;

        _isWiggling = true;
        _originalRotation = transform.rotation;

        float half = _wiggleDuration * 0.5f;

        yield return Sway(_originalRotation,  _wiggleIntensity, half);
        yield return Sway(_originalRotation, -_wiggleIntensity, half);

        transform.rotation = _originalRotation;
        _isWiggling = false;
    }

    private IEnumerator Sway(Quaternion baseRot, float targetAngle, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            float normalized = t / duration;
            float ease = Mathf.Sin(normalized * Mathf.PI);
            float angle = targetAngle * ease;

            transform.rotation = baseRot * Quaternion.Euler(0f, angle, 0f);

            t += Time.deltaTime;
            yield return null;
        }
    }
}
