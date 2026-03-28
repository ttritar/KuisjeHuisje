using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : ISingleton<AudioManager>
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer _mixer;

    private const string MIXER_MUSIC = "MusicVolume";
    private const string MIXER_SFX = "SFXVolume";

    public float MusicVolume { get; private set; }
    public float EffectVolume { get; private set; }


    // START
    //--------------------------------------------------
    private void Start()
    {
        MusicVolume = PlayerPrefs.GetFloat(MIXER_MUSIC, 0.8f);
        EffectVolume = PlayerPrefs.GetFloat(MIXER_SFX, 0.8f);

        ApplyVolumes();
    }

    // APPLY VOLUMES
    //--------------------------------------------------
    private void ApplyVolumes()
    {
        _mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(Mathf.Max(MusicVolume, 0.0001f)) * 20f);
        _mixer.SetFloat(MIXER_SFX, Mathf.Log10(Mathf.Max(EffectVolume, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat(MIXER_MUSIC, MusicVolume);
        PlayerPrefs.SetFloat(MIXER_SFX, EffectVolume);
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = value;
        PlayerPrefs.SetFloat(MIXER_MUSIC, value);

        _mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
    }

    public void SetEffectVolume(float value)
    {
        EffectVolume = value;
        PlayerPrefs.SetFloat(MIXER_SFX, value);

        _mixer.SetFloat(MIXER_SFX, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
    }

    // CROSS-FADE
    //--------------------------------------------------
    private AudioSource _currentAudioSource;
    public void CrossFade(AudioSource newSource, float duration = 1f)
    {
        if (newSource == _currentAudioSource)
            return;
        StartCoroutine(CrossFadeRoutine(_currentAudioSource, newSource, duration));
    }
    private IEnumerator CrossFadeRoutine(AudioSource original, AudioSource newSource, float duration)
    {
        if (newSource == null)
            yield break;

        if (!newSource.isPlaying)
            newSource.Play();

        float time = 0f;

        float originalStartVolume = 1f;
        float newStartVolume = 1f;
        newSource.volume = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            if (original)
                original.volume = Mathf.Lerp(originalStartVolume, 0f, t);

            newSource.volume = Mathf.Lerp(0f, newStartVolume, t);

            yield return null;
        }

        if (original)
            original.volume = 0;
        newSource.volume = newStartVolume;
        _currentAudioSource = newSource;
    }
}