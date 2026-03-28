using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class CounterLockedButton : MonoBehaviour
{
    [SerializeField] private int _requiredCount;
    private int _currentCount = 0;
    private bool _isUnlocked = false;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _counterTMP;

    [SerializeField] private Button _button;
    [SerializeField] private LocalizeStringEvent _buttonLocalizeStringEvent;
    [SerializeField] private string _readyStringKey = "Ready_NPC";
    [SerializeField] private string _unreadyStringKey = "Unready_NPC";


    // START
    //------------------------------
    private void Start()
    {
        UpdateCounterDisplay();
        _button.interactable = false;
    }
    // FUNCTIONALITY
    //------------------------------
    public void IncrementCount(int amount = 1)
    {
        _currentCount += amount;
        UpdateCounterDisplay();
    }

    private void UpdateCounterDisplay()
    {
        _counterTMP.text = $"{_currentCount} / {_requiredCount}";
        if (_currentCount >= _requiredCount)
        {
            UnlockButton();
        }
        else
        {
            if(_isUnlocked)
                UnlockButton(false);
        }
    }
    
    private void UnlockButton(bool isUnlocked = true)
    {
        if (!_button)
            return;
        _isUnlocked = isUnlocked;
        _button.interactable = isUnlocked;

        if (_buttonLocalizeStringEvent)
        {
            if(isUnlocked)
                _buttonLocalizeStringEvent.SetEntry(_readyStringKey);
            else
                _buttonLocalizeStringEvent.SetEntry(_unreadyStringKey);
        }
    }
}
