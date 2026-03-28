using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using UnityEngine.TextCore.Text;

public class HouseBehaviour : MonoBehaviour, IInteractable
{
    [Header("Cleaning")]
    [SerializeField] private HouseCleaningBehaviour _houseCleaningBehaviour;
    [SerializeField] private float _cleaningDelay = 4.0f;
    [SerializeField] private CountdownBehaviour _countdownBehaviour;
    public CountdownBehaviour CountdownBehaviour => _countdownBehaviour;

    [Header("Housing")]
    public int AssignedCharacterCount => _assignedCharacterCount;
    private int _assignedCharacterCount = 0;
    [SerializeField] private int _maxAssignedCharacters = 3;
    [SerializeField] private List<Transform> _spawnPositions = new List<Transform>();
    private Dictionary<CharacterData, Transform> _spawnOccupied = new Dictionary<CharacterData, Transform>();
    public List<CharacterData> AssignedCharacters => _spawnOccupied.Keys.ToList();

    [SerializeField] private GameObject _rootUI;
    [SerializeField] private GameObject _iconUIParent;
    [SerializeField] private float _freeSpotAlpha = 0.25f;

    [Header("Player Feedback")]
    [SerializeField] private GameObject _smokeGameObject;
    [SerializeField] private Material _material;
    private const string LAMP_ON = "_LIGHTS_ON";

    [Header("Events")]
    [SerializeField] public UnityEvent OnCharacterAssigned = new();
    [SerializeField] public UnityEvent OnMaxCharacterAssigned = new();
    [SerializeField] public UnityEvent OnCharacterRemoved = new();


    // START
    //--------------------------------------------------
    private void Start()
    {
        _rootUI.SetActive(false);
    }


    // INTERACT
    //--------------------------------------------------
    public bool CanInteract(GameObject interactor)
    {
        //if (!TutorialManager.Instance.PlayerHouseCleaner.HasCleanedOneHouse)
        //    return this == TutorialManager.Instance.PlayerHouse;
        return true;
    }
    public void Interact(GameObject interactor)
    {
        // Set Selected House
        var charInteractionMgrComp = interactor.GetComponent<CharacterInteractionManager>();
        if (_houseCleaningBehaviour != null && !_houseCleaningBehaviour.IsClean)
        {
            var houseCleanerComp = interactor.GetComponent<HouseCleaner>();
            houseCleanerComp.SelectedHouse = _houseCleaningBehaviour;
            _houseCleaningBehaviour.StartCleaningMode(houseCleanerComp, _cleaningDelay);
            _countdownBehaviour.StartCountdown();
        }

        // Assign
        else if (charInteractionMgrComp != null)
        {
            if (_assignedCharacterCount < _maxAssignedCharacters)
            {
                bool succeeded = charInteractionMgrComp.AssignFollowerToHouse(this);
                if(succeeded) PlayOnCharacterAssignedFeedback();
            }
        }
    }


    // ASSIGNING
    //--------------------------------------------------
    public Transform AddAssignee(CharacterData character)
    {
        if (_spawnOccupied.Count >= _maxAssignedCharacters || _spawnOccupied.ContainsKey(character))
            return null;

        Transform freePos = GetAvailableSpawnPos();
        if (freePos == null)
            return null;

        _spawnOccupied.Add(character, freePos);
        ++_assignedCharacterCount;
        UpdateUIIndicator();

        // Events
        OnCharacterAssigned?.Invoke();
        if (_assignedCharacterCount >= _maxAssignedCharacters)
            OnMaxCharacterAssigned?.Invoke();

        return freePos;
    }

    public void RemoveAssignee(CharacterData character)
    {
        if (!_spawnOccupied.Remove(character))
            return;

        --_assignedCharacterCount;
        UpdateUIIndicator();

        OnCharacterRemoved?.Invoke();
    }
    private void UpdateUIIndicator()
    {
        if (!_rootUI || !_iconUIParent)
            return;

        _rootUI.SetActive(_spawnOccupied.Count > 0);

        for (int i = 0; i < _iconUIParent.transform.childCount; i++)
            _iconUIParent.transform.GetChild(i).GetComponent<CanvasGroup>().alpha = (i < _assignedCharacterCount) ? 1f : _freeSpotAlpha;
    }
    private Transform GetAvailableSpawnPos()
    {
        foreach (var pos in _spawnPositions)
        {
            if (!_spawnOccupied.ContainsValue(pos))
                return pos;
        }
        return null;
    }

    // PLAYER FEEDBACK
    //--------------------------------------------------
    private void PlayOnCharacterAssignedFeedback()
    {
        if (!_smokeGameObject || !_material)
            return;

        //-- ONLY FIRST CHAR
        if (_assignedCharacterCount > 1)
            return;

        // chimney smoke
        _smokeGameObject.SetActive(true);

        // material
        _material = new Material(_material);

        GetComponent<Renderer>().material = _material;
        _material.EnableKeyword(LAMP_ON);

    }

}
