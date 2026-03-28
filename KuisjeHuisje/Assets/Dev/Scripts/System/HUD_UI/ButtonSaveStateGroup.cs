using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSaveStateGroup : MonoBehaviour
{
    private List<Button> _btn;
    private List<bool> _previousInteractState;
    // START
    // --------------------------------------------------------------
    private void Awake()
    {
        _btn = GetComponentsInChildren<Button>(true).ToList();
        _previousInteractState = new List<bool>(_btn.Count);
        foreach (var b in _btn)
        {
            _previousInteractState.Add(b.interactable);
        }

        SetPreviousStateToCurrent();
    }

    // Functionality
    // --------------------------------------------------------------
    public void SetPreviousStateToCurrent()
    {
        int i = 0;
        foreach (var b in _btn)
        {
            _previousInteractState[i] = b.interactable;
            ++i;
        }
    }

    public void SetToPreviousState()
    {
        int i = 0;
        foreach (var b in _btn)
        {
            b.interactable = _previousInteractState[i];
            ++i;
        }
    }
}
