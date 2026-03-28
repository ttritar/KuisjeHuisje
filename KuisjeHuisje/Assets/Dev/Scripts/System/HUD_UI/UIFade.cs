using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

[RequireComponent(typeof(CanvasGroup))]
public class UIFade : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 0.2f;
    [SerializeField] private bool _deactivateAfterFade = true;

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeRoutine;

    [SerializeField] private bool _fadeOnEnable = false;
    // START
    //--------------------------------------------------
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }
    private void OnEnable()
    {
        if (_fadeOnEnable)
        {
            _canvasGroup.alpha = 0f;
            FadeInAndEnable();
        }
    }

    // FADES
    //--------------------------------------------------
    public void FadeOutAndDisable()
    {
        if (!enabled || !this.gameObject.activeSelf) return;
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeOutRoutine());
    }
    private IEnumerator FadeOutRoutine()
    {
        float startAlpha = _canvasGroup.alpha;
        float time = 0f;

        while (time < _fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / _fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;

        if (_deactivateAfterFade)
            gameObject.SetActive(false);
    }

    public void FadeInAndEnable()
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);
        gameObject.SetActive(true);
        _fadeRoutine = StartCoroutine(FadeInRoutine());
    }
    private IEnumerator FadeInRoutine()
    {
        float startAlpha = _canvasGroup.alpha;
        float time = 0f;
        while (time < _fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, time / _fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = 1f;
    }

    public void Fade(bool active)
    {
        if (active)
            FadeInAndEnable();
        else
            FadeOutAndDisable();
    }
}

