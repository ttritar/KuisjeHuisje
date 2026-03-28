using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionUICounter : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _number;

    // BEHAVIOUR
    //--------------------------------------------------
    public void OnNumberChanged(int number)
    {
        if (_button != null)
        {
            _button.interactable = true;
            if (number <= 0)
                _button.interactable = false;
        }

        if (_number != null)
        {
            _number.text = number.ToString();
        }
    }
}
