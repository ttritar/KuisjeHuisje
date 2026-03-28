using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSaveState : MonoBehaviour
{
    private Button _btn;
    private bool _previousInteractState;

    // START
    // --------------------------------------------------------------
    private void Awake()
    {
        _btn = GetComponent<Button>();
        _previousInteractState = _btn.interactable;
    }

    // Functionality
    // --------------------------------------------------------------
    public void SetPreviousStateToCurrent()
    {
        _previousInteractState = _btn.interactable;
    }
    public void SetToPreviousState()
    {
        _btn.interactable = _previousInteractState;
    }
}
