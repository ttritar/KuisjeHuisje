using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TutorialManager : ISingleton<TutorialManager>
{
    // Data
    public enum TutorialType
    {
        Cleaning,
        Assigning,
        Potion,
        Item,
        None
    }
    [System.Serializable]
    public class Tutorial
    {
        public TutorialType Type;
        public List<TutorialStep> Steps = new();
        public UnityEvent OnCompletion = new();
    }
    [Serializable]
    public class TutorialStep
    {
        public GameObject TextBox;
        public bool DisableInput = true;
    }

    // Variables
    [Header("Tutorial")]
    [SerializeField] private List<Tutorial> _tutorials = new();
    private Dictionary<TutorialType, bool> _explainedTutorials = new();
    [SerializeField] private bool _doTutorial = false;
    [SerializeField] List<HouseCleaningBehaviour> _playerHouses;

    [Header("Settings")]
    [SerializeField] private WizardInteractionManager _wizardInteractionManager;
    [SerializeField] private WizardInteractBehaviour _wizard;
    private PlayerInput _playerInput;

    [Header("Events")]
    public UnityEvent OnTutorialStarted = new();
    public UnityEvent OnTutorialStepStarted = new();
    public UnityEvent OnTextCompleted = new();
    public UnityEvent OnTutorialEnded = new();

    private int _currentStepIndex = -1;
    private Tutorial _currentTutorial = null;
    private TypeWriterText _currentTypeWriter = null;
    private int _lastRunIndex = 0;
    private bool _isRunning = false;
    private bool _isReady = false;
    public bool IsTutorialRunning => _isRunning;

    private void Start()
    {
        _explainedTutorials = new Dictionary<TutorialType, bool>();
        foreach (var t in _tutorials)
        {
            foreach (var s in t.Steps)
            {
                s.TextBox.SetActive(false);
            }

            if (_doTutorial) continue;
            _explainedTutorials.TryAdd(t.Type, true);
            t.OnCompletion.Invoke();
        }

        _playerInput = _wizardInteractionManager.GetComponent<PlayerInput>();

        StartInitialTutorial();
    }

    private void StartInitialTutorial()
    {
        Debug.Log("Starting Tutorial");
        _lastRunIndex = 0;
        if (_tutorials.Count > 0)
            StartTutorial(_tutorials[0].Type);
    }


    // START
    //--------------------------------------------------
    public void StartNextTutorial()
    {
        int nextIdx = _lastRunIndex + 1;
        if (nextIdx >= _tutorials.Count - 1) return;
        StartTutorial(_tutorials[_lastRunIndex + 1].Type);
    }
    public void StartTutorial(TutorialType tutorialName)
    {
        if (!_doTutorial)
        {
            _explainedTutorials.TryAdd(tutorialName, true);
            return;
        }

        int count = 0;
        _currentTutorial = _tutorials.Find(t =>
        {
            _lastRunIndex = count;
            count++;
            return t.Type == tutorialName;
        });
        if (_currentTutorial == null || _currentTutorial.Steps.Count == 0)
            return;

        _isRunning = true;
        _currentStepIndex = -1;

        _wizardInteractionManager.StartInteraction(_wizard, true);
        _wizardInteractionManager.OnStartInteractionLate.AddListener(InvokeStart);
        _wizardInteractionManager.OnStartInteractionLate.AddListener(NextStep);
    }

    // PROGRESS
    //--------------------------------------------------
    private void InvokeStart()
    {
        _wizardInteractionManager.OnStartInteractionLate.RemoveListener(InvokeStart);
        OnTutorialStarted.Invoke();
    }
    private void NextStep()
    {
        OnTutorialStepStarted.Invoke();
        _wizardInteractionManager.OnStartInteractionLate.RemoveListener(NextStep);
        if (!_isRunning || _currentTutorial == null)
            return;

        _currentStepIndex++;
        if (_currentStepIndex >= _currentTutorial.Steps.Count)
        {
            EndTutorial();
            return;
        }

        StartCoroutine(RunStep(_currentTutorial.Steps[_currentStepIndex]));
    }
    private IEnumerator RunStep(TutorialStep step)
    {
        // disable input
        if (step.DisableInput)
            _playerInput.DeactivateInput();

        // activate text
        step.TextBox.SetActive(true);
        yield return null;

        // play type effect
        _currentTypeWriter = step.TextBox.GetComponent<TypeWriterText>();
        if (_currentTypeWriter != null)
            _currentTypeWriter.Play();

        // wait for input
        _isReady = false;
        OnTextCompleted.Invoke();
        while (!_isReady)
            yield return null;

        // deactivate text
        step.TextBox.SetActive(false);

        // disable input
        if (step.DisableInput)
            _playerInput.ActivateInput();

        // next box
        NextStep();
    }

    // EVENTS
    //--------------------------------------------------
    public void OnReady()
    {
        if (_isReady) return;
        if (_currentTypeWriter)
        {
            if(_currentTypeWriter.IsTyping())
            {
                _currentTypeWriter.Skip();
                return;
            }
            _isReady = true;
            return;
        }
        _isReady = true;
    }

    // END
    //--------------------------------------------------
    private void EndTutorial()
    {
        _wizardInteractionManager.EndInteraction();
        OnTutorialEnded.Invoke();

        if (_currentTutorial.Type == TutorialType.Cleaning && !HasExplainedTutorial(TutorialType.Cleaning))
        {
            ForceCleaningPlayerHouse();
            var house = _playerInput.GetComponent<HouseCleaner>().SelectedHouse;
            house.OnCleanComplete.AddListener(EndTutorialContinuation);
            house.OnCleanComplete.AddListener(CleanAllPlayerHouses);
        }
        else
        {
            EndTutorialContinuation();
        }
    }

    private void EndTutorialContinuation()
    {
        _explainedTutorials.TryAdd(_currentTutorial.Type, true);
        _isRunning = false;
        _currentTutorial.OnCompletion.Invoke();
        _currentTutorial = null;
        _currentStepIndex = -1;
    }
    public bool HasExplainedTutorial(TutorialType type)
    {
        return !_doTutorial || _explainedTutorials.GetValueOrDefault(type, false);
    }

    // HELPER
    //--------------------------------------------------
    private void CleanAllPlayerHouses()
    {
        foreach (var house in _playerHouses)
        {
            if(!house.IsClean)
                house.SetClean();
        }
    }
    private void ForceCleaningPlayerHouse()
    {
        var houseCleaner = _playerInput.GetComponent<HouseCleaner>();
        houseCleaner.SelectedHouse = _playerHouses[(int)WorldSwitchManager.Instance.CurrentEmotion];
        houseCleaner.SelectedHouse.StartCleaningMode(houseCleaner);
    }
}
