using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class CharacterEmotionsSwap : MonoBehaviour
{
    public Emotion Emotion
    {
        get { return _emotion; }
        set { _emotion = value; }
    }
    private Emotion _emotion;
    private Emotion _previousEmotion;

    [SerializeField] private GameObject _face;
	private Material _instanceMat;
    private Renderer _renderer;


    // START
    //--------------------------------------------------
    private void Awake()
    {
        Initialize();
    }

    // TALKING
    //--------------------------------------------------
    public void StartTalking() => _instanceMat?.SetFloat("_ISTALKING", 1f);
    public void StopTalking() => _instanceMat?.SetFloat("_ISTALKING", 0f);



    // EMOTION
    //--------------------------------------------------
    public void ApplyEmotion(Emotion emotion)
    {
        if(_renderer == null || _instanceMat == null)
            Initialize();

        if (emotion == Emotion.COUNT)
            return;

        if (_instanceMat == null)
            return;

        StopTalking();

        _instanceMat.SetFloat("_UseExpression", 1f);
        _instanceMat.SetFloat("_ExpresionIndex", ConvertEmotion(emotion));

        _emotion = emotion;
    }
    public IEnumerator ApplyEmotionForTime(Emotion emotion, float time)
    {
        _previousEmotion = _emotion;

        ApplyEmotion(emotion);
        yield return new WaitForSeconds(time);

        ApplyEmotion(_previousEmotion);
    }
    public void ClearEmotion()
    {
        if (_instanceMat != null)
            _instanceMat.SetFloat("_UseExpression", 0f);
    }

    private void Initialize()
    {
        _renderer = _face.GetComponent<Renderer>();
        if (_renderer != null)
        {
            if (_renderer.sharedMaterial == null)
                return;
            _instanceMat = new Material(_renderer.sharedMaterial);
            _renderer.material = _instanceMat;
        }
    }

    private float ConvertEmotion(Emotion emotion)
    {
        return emotion switch
        {
            Emotion.Happy => 0f,
            Emotion.Sad => 3f,
            Emotion.Angry => 1f,
            Emotion.Scared => 2f,
            _ => 0f
        };
    }
}