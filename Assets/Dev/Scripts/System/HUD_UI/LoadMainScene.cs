using UnityEngine;
using UnityEngine.UI;

public class LoadMainScene : MonoBehaviour
{
    [SerializeField] private Button _button;
    private void OnEnable()
    {
        _button.onClick.AddListener(SceneLoader.Instance.LoadMainScene);
    }
    private void OnDisable()
    {
        _button.onClick.RemoveListener(SceneLoader.Instance.LoadMainScene);
    }
}
