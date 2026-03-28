using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;

public class LocaleSelector : MonoBehaviour
{
    private bool _active = false;
    public int Locale => _locale;
    private int _locale = 0;

    [SerializeField] public UnityEvent<string> OnLocaleChanged = new();


    // FUNCTIONALITY
    //--------------------------------------------------
    public void ChangeLocale(int localeID)
    {
        if(_active)
            return;
        SetLocale(localeID);
    }
    private void SetLocale(int localeID)
    {
        _active = true;

        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (locales == null || locales.Count == 0)
        {
            Debug.LogWarning("[LocaleSelector] No available locales found!");
            _active = false;
            return;
        }


        // Modulo to wrap around available locales
        localeID = (localeID + locales.Count) % locales.Count;

        // Apply locale
        LocalizationSettings.SelectedLocale = locales[localeID];
        _locale = localeID;

        OnLocaleChanged.Invoke(LocalizationSettings.SelectedLocale.Identifier.CultureInfo.NativeName);

        _active = false;
    }
}
    