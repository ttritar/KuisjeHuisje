using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TypeWriterText : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private float _charDelay = 0.03f;

    private string _fullText;
    private Coroutine _typingRoutine;

    [Header("Events")]
    [SerializeField] public UnityEvent OnTypingStarted;
    [SerializeField] public UnityEvent OnCharacterTyped;
    [SerializeField] public UnityEvent OnEveryOtherCharacterTyped;
    [SerializeField] public UnityEvent OnTypingSkipped;
    [SerializeField] public UnityEvent OnTypingComplete;


    // START
    //------------------------------
    private void Start()
    {
        if (_text == null)
            _text = GetComponentInChildren<TMP_Text>(true);
    }

    private void OnEnable()
    {
        _fullText = _text.text;
        _text.maxVisibleCharacters = 0;
    }

    // FUNCTIONALITY
    //------------------------------
    public void Play(System.Action onComplete = null)
    {
        if (_typingRoutine != null)
            StopCoroutine(_typingRoutine);

        _typingRoutine = StartCoroutine(TypeRoutine(onComplete));
    }

    private IEnumerator TypeRoutine(System.Action onComplete)
    {
        OnTypingStarted?.Invoke();

        _fullText = _text.text;
        int len = _fullText.Length;

        _text.maxVisibleCharacters = 0;

        for (int i = 0; i <= len; i++)
        {
            _text.maxVisibleCharacters = i;
            OnCharacterTyped?.Invoke();
            if (i % 2 == 0)
                OnEveryOtherCharacterTyped?.Invoke();
            yield return new WaitForSeconds(_charDelay);
        }

        onComplete?.Invoke();
        OnTypingComplete?.Invoke();
    }

    public void Skip()
    {
        if (_typingRoutine != null)
            StopCoroutine(_typingRoutine);

        _text.maxVisibleCharacters = _text.text.Length;

        OnTypingSkipped?.Invoke();
    }

    public bool IsTyping()
    {
        return _text.maxVisibleCharacters < _fullText.Length;
    }
}
