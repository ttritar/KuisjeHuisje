using JetBrains.Annotations;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("Language")]
    [SerializeField] [CanBeNull] private LocaleSelector _localeSelector;

    [Header("Volume")]
    [SerializeField] private Slider _effectSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private AudioSource _testSoundEffect;
    [SerializeField] private AudioSource _testSoundMusic;

    [Header("File")]
    [SerializeField] private TMP_InputField _pathInput;


    // START
    //--------------------------------------------------
    private void OnEnable()
    {
        _musicSlider.value = AudioManager.Instance.MusicVolume;
        _effectSlider.value = AudioManager.Instance.EffectVolume;
    }
    private void Start()
    {
        // File
        if (_pathInput != null)
            _pathInput.text = FileLogger.Instance.LogDirectory;
    }
    public void PlayTestSound(BaseEventData data)
    {
        var selectedSlider = data.selectedObject.GetComponent<Slider>();
        if (selectedSlider == _effectSlider)
        {
            _testSoundEffect.volume = AudioManager.Instance.EffectVolume;
            _testSoundEffect.Play();
        }
        else if (selectedSlider == _musicSlider)
        {
            _testSoundMusic.volume = AudioManager.Instance.MusicVolume;
            _testSoundMusic.Play();
        }
    }

    // LANGUAGE
    //--------------------------------------------------
    private void SwitchLanguage(bool previous = false)
    {
        if(!_localeSelector) 
            return;

        int current = _localeSelector.Locale; 
        int localeID = current;
        if (previous)
            --localeID;
        else
            ++localeID;

        _localeSelector.ChangeLocale(localeID);
    }
    public void SelectNextLanguage()
    {
        SwitchLanguage(false);
    }
    public void SelectPreviousLanguage()
    {
        SwitchLanguage(true);
    }


    // VOLUME
    //--------------------------------------------------
    public void SetMusicVolume(Single volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
    }
    public void SetSfxVolume(Single volume)
    {
        AudioManager.Instance.SetEffectVolume(volume);
    }


    // FILE
    //--------------------------------------------------
    public void OpenFile()
    {
        string path = FileLogger.Instance.LogDirectory;
        if (System.IO.Directory.Exists(path))
            System.Diagnostics.Process.Start("explorer.exe", path);
    }
    public void PickFileDialog()
    {
        FileLogger.Instance.PickLogDirectory();
    }
}
