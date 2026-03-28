using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonCooldownBehaviour : MonoBehaviour
{
    private bool _cooldownActive = false;

    [SerializeField] private float _cooldownDuration = 3f;
    private float _cooldownTimer = 0f;

    [System.Serializable]
    class ButtonBoolPair
    {
        public Button Key;
        public bool Value;
    }
    [SerializeField] private List<ButtonBoolPair> _buttonsToCooldown;
    private List<bool> _buttonsInitialStates;

    [SerializeField] private bool _resetOnEnable = false;
    public UnityEvent OnCooldownStart = new();
    public UnityEvent OnCooldownEnd = new();

    // START
    // --------------------------------------------------------------
    private void Start()
    {
        if (_buttonsToCooldown == null || _buttonsToCooldown.Count == 0)
        {
            Debug.LogWarning("[ButtonCooldownBehaviour] No buttons assigned for cooldown.");
            return;
        }

        _buttonsInitialStates = new List<bool>();
        foreach (var button in _buttonsToCooldown)
        {
            if (button.Value)
                button.Key.onClick.AddListener(StartCooldown);
            _buttonsInitialStates.Add(button.Key.interactable);
        }
    }

    private void OnEnable()
    {
        if (_resetOnEnable)
        {
            ResetButtonsToInitialStates();
        }
    }

    // UPDATE
    // --------------------------------------------------------------
    private void Update()
    {
        if(!_cooldownActive)
            return;

        if(_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer <= 0f)
        {
            ResetButtonsToInitialStates();
            OnCooldownEnd.Invoke();
        }
    }


    // FUNCTIONALITY
    // --------------------------------------------------------------
    public void StartCooldown()
    {
        OnCooldownStart.Invoke();
        for (int i = 0; i < _buttonsToCooldown.Count; i++)
            _buttonsInitialStates[i] = _buttonsToCooldown[i].Key.interactable;
        _cooldownTimer = _cooldownDuration;
        SetButtonsInteractable(false);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (var button in _buttonsToCooldown)
        {
            if(button.Key != null)
                button.Key.interactable = interactable;
        }
        _cooldownActive = !interactable;
    }

    private void ResetButtonsToInitialStates()
    {
        for (int i = 0; i < _buttonsToCooldown.Count; i++)
        {
            if (_buttonsToCooldown[i].Key != null && _buttonsInitialStates[i] != null)
                _buttonsToCooldown[i].Key.interactable = _buttonsInitialStates[i];
        }
        _cooldownActive = false;
        _cooldownTimer = 0f;
    }
}
