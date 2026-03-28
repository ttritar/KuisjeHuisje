using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomEffect : MonoBehaviour
{
    [SerializeField] private bool _playOnStart = false;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] _clips;
    private AudioSource _source;

    [SerializeField] private bool _usePitchModifier = false;
    public float PitchModifier   { get; set; } = 1f;

    [Header("Random Pitch")]
    [SerializeField] private bool _useRandomPitch = false;
    [SerializeField] private float _minPitch = 0.9f;
    [SerializeField] private float _maxPitch = 1.1f;

    [Header("Random Delay")]
    [SerializeField] private bool _useRandomDelay = false;
    [SerializeField] private float _minDelay = 0.1f;
    [SerializeField] private float _maxDelay = 0.5f;


    // START
    //--------------------------------------------------
    private void Start()
    {
        _source = GetComponent<AudioSource>();
        if (_playOnStart)
        {
            Play();
        }
    }

    // PLAY
    //--------------------------------------------------
    public void Play()
    {
        if (_clips == null || _clips.Length == 0 || _source == null)
            return;

        // delayed
        if (_useRandomDelay)
        {
            StartCoroutine(DelayPlay());
            return;
        }


        AudioClip clip = _clips[Random.Range(0, _clips.Length)];

        _source.pitch = _useRandomPitch ? Random.Range(_minPitch, _maxPitch) : 1f;
        if (_usePitchModifier)
            _source.pitch *= PitchModifier;

        _source.PlayOneShot(clip);
    }

    private IEnumerator DelayPlay()
    {
        float delay = Random.Range(_minDelay, _maxDelay);
        yield return new WaitForSeconds(delay);


        AudioClip clip = _clips[Random.Range(0, _clips.Length)];

        _source.pitch = _useRandomPitch ? Random.Range(_minPitch, _maxPitch) : 1f;
        _source.PlayOneShot(clip);
    }

    // STOP
    //--------------------------------------------------
    public void Stop()
    {
        if (_source != null)
            _source.Stop();
    }
}
