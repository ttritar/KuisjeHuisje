using UnityEngine;
using UnityEngine.UI;

public class QuitHookup : MonoBehaviour
{
    [SerializeField] private Button _quitButton;
    void Start()
    {
        _quitButton.onClick.AddListener(GameManager.Instance.QuitGame);
    }

}
