using System.Collections.Generic;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    [SerializeField] private float _messageMinimumCooldown = 20.0f;
    [SerializeField] private float _messageMaximumCooldown = 40.0f;
    [SerializeField] private float _messageDisplayDuration = 3.0f;
    [SerializeField] private List<SlideInTransition> _messageFaders;

    private float _nextMessageTime = 0.0f;
    private int _lastMessageIndex = -1;

    private float _messageDisplayedTime = 0.0f;

    private SlideInTransition _activeMessage = null;

    // START
    //--------------------------------------------------
    private void Start()
    {
        ScheduleNextMessage();
    }


    // UPDATE
    //--------------------------------------------------
    private void Update()
    {
        if (Time.time >= _nextMessageTime)
        {
            if(!TutorialManager.Instance.IsTutorialRunning)
                ShowRandomMessage();
            ScheduleNextMessage();
        }

        _messageDisplayedTime += Time.deltaTime;
        if(_messageDisplayedTime >= _messageDisplayDuration)
        {
            HideActiveMessage();
            _messageDisplayedTime = 0.0f;
        }
    }


    // HELPERS
    //--------------------------------------------------
    private void ScheduleNextMessage()
    {
        float cooldown = Random.Range(_messageMinimumCooldown, _messageMaximumCooldown);
        _nextMessageTime = Time.time + cooldown + _messageDisplayDuration;
    }

    private void ShowRandomMessage()
    {
        if (_messageFaders.Count == 0) return;
        if (_activeMessage != null) return; 

        int randomIndex = Random.Range(0, _messageFaders.Count);
        // make sure it isnt the same as last time
        while (randomIndex == _lastMessageIndex && _messageFaders.Count > 1)
        {
            randomIndex = Random.Range(0, _messageFaders.Count);
        }

        SlideInTransition selectedSlider = _messageFaders[randomIndex];
        selectedSlider.SlideIn();

        _activeMessage = selectedSlider;
        _messageDisplayedTime = 0.0f; 
        _lastMessageIndex = randomIndex;
    }

    private void HideActiveMessage()
    {
        if (_activeMessage != null)
        {
            _activeMessage.SlideOut();
            _activeMessage = null;
        }
    }
}
