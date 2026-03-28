using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class UIElement
{
    public string Key;
    public List<GameObject> Element;
    public bool Active;
    public TransitionType transitionType = TransitionType.None;
}

public enum TransitionType
{
    None,
    Fade,
    Slide
}

public class UIManager : MonoBehaviour
{
    [SerializeField] private List<UIElement> _uiList;
    private Dictionary<string, UIElement> _uiMap;

    // START
    //--------------------------------------------------
    private void Awake()
    {
        _uiMap = new Dictionary<string, UIElement>();
        foreach (var ui in _uiList)
        {
            foreach (var el in ui.Element)
                el.gameObject.SetActive(ui.Active);
            _uiMap[ui.Key] = ui;
        }
    }

    // LOGIC
    //--------------------------------------------------
    public void ToggleUI(string key)
    {
        var ui = FindUI(key);
        if (ui == null)
            return;

        SetUIActive(key, !ui.Active);
    }
    public void EnableUI(string key) => SetUIActive(key, true);
    public void DisableUI(string key) => SetUIActive(key, false);
    public void SetUIActive(string key, bool active)
    {
        var ui = FindUI(key);
        if (ui == null)
            return;

        switch (ui.transitionType)
        {
            case TransitionType.None:
            {
                foreach (var el in ui.Element)
                {
                    el.SetActive(active);
                }
                break;
            }
            case TransitionType.Fade:
            {
                foreach (var el in ui.Element)
                {
                    if (el.TryGetComponent(out UIFade fader))
                        fader.Fade(active);
                }
                break;
            }
            case TransitionType.Slide:
            {
                foreach (var el in ui.Element)
                {
                    if (el.TryGetComponent(out SlideInTransition slider))
                    {
                        if (active)
                            slider.SlideIn();
                        else
                            slider.SlideOut();
                    }
                    else
                        el.SetActive(active);

                    foreach (var childSlider in el.GetComponentsInChildren<SlideInTransition>())
                    {
                        if (active)
                            childSlider.SlideIn();
                        else
                            childSlider.SlideOut();
                    }
                }
                break;
            }
        }

        ui.Active = active;
    }
    public void EnableUIAlone(string key)
    {
        var ui = FindUI(key);
        if (ui == null)
            return;

        foreach (var element in _uiList)
        {
            if (element == ui)
                continue;
            SetUIActive(element.Key, false);
        }

        SetUIActive(key, true);
    }


    // HELPER
    //--------------------------------------------------
    public List<GameObject> GetUIGameObjects(string key)
    {
        return FindUI(key)?.Element ?? null;
    }
    private UIElement FindUI(string key)
    {
        _uiMap.TryGetValue(key, out var ui);
        return ui;
    }

}
