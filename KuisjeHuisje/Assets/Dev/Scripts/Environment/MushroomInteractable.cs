using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

public class MushroomInteractable : MonoBehaviour, IInteractable
{
    [Header("Scale Settings")]
    [SerializeField] private float _scaleMultiplier = 1.5f;
    [SerializeField] private float _scaleDuration = 0.3f;

    private Coroutine _scalingCoroutine;
    private Vector3 _originalScale;
    private bool _isScaling;

    [Header("Player Feedback")]
    [SerializeField] private RandomEffect _randomEffect;
    private int _currentPitchIndex = 0;

    [SerializeField] private float[] _pitchModifierArray = new float[]
        { 1.0f, 1.4f, 1.8f, 2.2f, 2.6f, 3.0f };

    [SerializeField] private GameObject _deathEffectPrefab;

    [Header("Events")]
    [SerializeField] private UnityEvent OnInteracted;

    // START
    //--------------------------------
    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    // DESTROY
    //--------------------------------
    private void OnKilled()
    {
        if (_deathEffectPrefab != null)
        {
            var obj = Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
            obj.transform.parent = null;
            obj.SetActive(true);
            Destroy(obj, 1.5f);
        }
    }

    // INTERACT
    //--------------------------------
    public void Interact(GameObject interactor)
    {
        if(!CanInteract(interactor))
            return;

        if (_isScaling)
            return;

        if (_scalingCoroutine != null)
        {
            StopCoroutine(_scalingCoroutine);
            _scalingCoroutine = null;
        }

        _scalingCoroutine = StartCoroutine(ScaleMushroomRoutine(_scaleDuration));


        // sound
        if (_randomEffect != null)
        {
            _randomEffect.PitchModifier = _pitchModifierArray[_currentPitchIndex];
            _randomEffect.Play();
            _currentPitchIndex++;
            if (_currentPitchIndex >= _pitchModifierArray.Length)
            {
                OnKilled();
                Destroy(gameObject);
            }
        }

        OnInteracted?.Invoke();
    }

    public bool CanInteract(GameObject interactor)
    {
        return true;
    }


    // Scale
    //--------------------------------
    private IEnumerator ScaleMushroomRoutine(float duration)
    {
        _isScaling = true;

        Vector3 target = _originalScale * _scaleMultiplier;

        // scale up
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float ease = t * t * (3f - 2f * t);
            transform.localScale = Vector3.LerpUnclamped(_originalScale, target, ease);
            yield return null;
        }

        transform.localScale = target;

        // scale back
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float ease = t * t * (3f - 2f * t);
            transform.localScale = Vector3.LerpUnclamped(target, _originalScale, ease);
            yield return null;
        }

        transform.localScale = _originalScale;

        _isScaling = false;
        _scalingCoroutine = null;
    }

}
