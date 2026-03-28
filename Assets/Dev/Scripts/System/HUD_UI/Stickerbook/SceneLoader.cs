using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneLoader : ISingleton<SceneLoader>
{
    enum CrossFadeType
    {
        Out, In, Both
    }

    private AsyncOperation _asyncOp = null;

    [Header("VFX")]
    [SerializeField] private Animator _crossFade;
    [SerializeField] private CrossFadeType _crossFadeType = CrossFadeType.In;
    [SerializeField] private Animator _vfxToMain;
    [SerializeField] private Animator _vfxToBook;
    [SerializeField] private float _minWaitDelay = 3f;
    [SerializeField] private float _minWaitDelayEffect = 1f;
    private string _animationName = "ANIM_StartTransition";

    [Header("Indices")]
    [SerializeField] private int _bookSceneBuildIndex = 0;
    [SerializeField] private int _gameSceneBuildIndex = 1;

    [Header("Events")]
    public UnityEvent OnBeginLoading = new();
    public UnityEvent OnMainSceneLoaded = new();
    public UnityEvent OnStickerbookLoaded = new();

    // START
    //--------------------------------------------------
    private void OnEnable()
    {
        if (_vfxToMain)
        {
            _vfxToMain.gameObject.SetActive(true);
            _vfxToMain.gameObject.SetActive(false);
        }

        if (_vfxToBook)
        {
            _vfxToBook.gameObject.SetActive(true);
            _vfxToBook.gameObject.SetActive(false);
        }
    }

    // SCENE LOADING
    //--------------------------------------------------
    private void LoadScene(int idx, Animator vfx)
    {
        if (_asyncOp != null && !_asyncOp.isDone)
            return;

        OnBeginLoading.Invoke();
        SceneManager.sceneLoaded += InvokeSceneEvent;

        StartCoroutine(LoadSceneRoutine(idx, vfx));
    }
    private IEnumerator LoadSceneRoutine(int idx, Animator vfx)
    {
        var playerInput = FindAnyObjectByType<PlayerInput>();
        playerInput.DeactivateInput();

        AnimatorStateInfo animInfo = default;
        bool hasAnim = false;

        // begin vfx
        if (vfx)
        {
            vfx.gameObject.SetActive(true);
            vfx.Play(_animationName, 0, 0f);

            yield return null;

            animInfo = vfx.GetCurrentAnimatorStateInfo(0);
            hasAnim = true;
        }

        // begin preload
        _asyncOp = SceneManager.LoadSceneAsync(idx);
        _asyncOp.allowSceneActivation = false;

        if (hasAnim)
            yield return new WaitForSecondsRealtime(animInfo.length + _minWaitDelayEffect);

        if (_crossFade && (_crossFadeType == CrossFadeType.Out || _crossFadeType == CrossFadeType.Both))
        {
            _crossFade.gameObject.SetActive(true);
            _crossFade.ResetTrigger("End");
            _crossFade.SetTrigger("Start");

            var cg = _crossFade.GetComponentInChildren<CanvasGroup>();
            while (!Mathf.Approximately(cg.alpha, 1f))
                yield return null;
        }

        yield return new WaitForSecondsRealtime(_minWaitDelay);

        while (_asyncOp.progress < 0.9f)
            yield return null;

        if(vfx) vfx.gameObject.SetActive(false);
        _asyncOp.allowSceneActivation = true;
    }
    public void LoadMainScene()
    {
        LoadScene(_gameSceneBuildIndex, _vfxToMain);
    }
    public void LoadStickerScene()
    {
        LoadScene(_bookSceneBuildIndex, _vfxToBook);
    }
    private void InvokeSceneEvent(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= InvokeSceneEvent;
        if (_crossFade && (_crossFadeType == CrossFadeType.Out || _crossFadeType == CrossFadeType.Both))
        {
            _crossFade.gameObject.SetActive(true);
            _crossFade.ResetTrigger("Start");
            _crossFade.SetTrigger("End");
        }

        if (scene.buildIndex == _gameSceneBuildIndex) OnMainSceneLoaded.Invoke();
        else if (scene.buildIndex == _bookSceneBuildIndex) OnStickerbookLoaded.Invoke();
    }
}
