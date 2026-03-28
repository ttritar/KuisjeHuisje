using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    private PlayerInput _input;

    // START
    //--------------------------------------------------
    private void Awake()
    {
        _input = FindAnyObjectByType<PlayerInput>();
    }
    // BTN  EVENTS
    //--------------------------------------------------
    public void OnPauseButton()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        _input?.DeactivateInput();
    }

    public void OnResumeButton()
    {
        if(!MapCharacterDisplay.IsOpen && !MapHouseDisplay.IsOpen)
        {
            _input?.ActivateInput();
            Time.timeScale = 1f;
        }    
        gameObject.SetActive(false);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    public void OnMainMenuButton()
    {
        _input?.ActivateInput();
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadStickerScene();
    }
}
