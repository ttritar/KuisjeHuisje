    using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using UnityEngine.UI;

public class WorldSwitchManager : ISingleton<WorldSwitchManager>
{
    [Serializable]
    public class EmotionWorldPair
    {
        public Emotion emotion;
        public WorldData world;
    }
    [Header("World Mapping")]
    [SerializeField] private Emotion _startWorld;
    [SerializeField] private List<EmotionWorldPair> _worldMap;
    public List<EmotionWorldPair> WorldMap => _worldMap;
    [SerializeField] private MapCharacterDisplay _characterMap;

    public EmotionWorldPair NextWorldPair => _nextWorldPair;
    private EmotionWorldPair _nextWorldPair;
    public EmotionWorldPair CurrentWorldPair => _currentWorldPair;
    private EmotionWorldPair _currentWorldPair;

    [Header("World Handling")]
    [SerializeField] private GameObject _rootToMove;
    public GameObject Moveables => _rootToMove;

    [Header("UI")]
    [SerializeField] private UIManager _uiManager;

    [Header("Camera")]
    public bool IsTransitioning { get; private set; }
    [SerializeField] private CameraSwitchManager _cameraManager;

    [Header("Events")]
    public UnityEvent OnWorldSwitchStart = new();
    public UnityEvent OnCharacterWorldSwitch = new();

    // START
    //--------------------------------------------------
    protected override void Awake()
    {
        base.Awake();

        foreach (var pair in _worldMap)
        {
            pair.world.gameObject.SetActive(false);
            pair.world.Initialize();
        }

        _currentWorldPair = GetPair(_startWorld);
        _currentWorldPair.world.gameObject.SetActive(true);
        _rootToMove.transform.position = _currentWorldPair.world.transform.position;
    }
    private void Start()
    {
        UpdateButtons(_currentWorldPair.emotion);
    }


    // MECHANICS
    //--------------------------------------------------
    public void SendCharacterToWorld(GameObject obj, Emotion world, bool respline, bool force = false)
    {
        if (world == Emotion.COUNT) return;
        if (!force && world == _currentWorldPair.emotion) return;

        var worldPair = GetPair(world);

        SplineContainer newDefault = worldPair.world.RandomCharacterSpline;
        var ai = obj.GetComponent<CharacterAIManager>();
        if (ai == null)
            return;

        var charData = obj.GetComponent<CharacterData>();
        if (charData != null)
        {
            if(!force)
                FileLogger.Instance.LogNPCWorldSwitch(_currentWorldPair.emotion, world, charData);
            _currentWorldPair.world.RemoveCharacter(charData);
            worldPair.world.AddCharacter(charData);
        }

        if (respline)
        {
            ai.EnableMovementAI(newDefault, SplineAnimate.LoopMode.Loop, !force);
            obj.transform.SetParent(worldPair.world.transform);

            var anim = obj.GetComponent<SplineAnimate>();
            anim.Container = newDefault;
        }

        ai.SetDefaults(newDefault, worldPair.world.transform);
        OnCharacterWorldSwitch.Invoke();
    }
    public void SendPlayerToWorld(GameObject player, Emotion newWorld)
    {
        if (newWorld == Emotion.COUNT)
            return;

        var intMan = player.GetComponent<CharacterInteractionManager>();
        intMan?.PlayTransitionAnimation();
        if (intMan.CurrentFollower != null)
            SendCharacterToWorld(intMan.CurrentFollower.gameObject, newWorld, false);

        FileLogger.Instance.LogWorldSwitch(_currentWorldPair.emotion, newWorld);
        SwitchWorld(newWorld);
    }
    public void ForceSetWorld(Emotion newWorld)
    {
        if (newWorld == Emotion.COUNT)
            return;

        _nextWorldPair = GetPair(newWorld);

        _rootToMove.transform.position = _nextWorldPair.world.transform.position;

        _nextWorldPair.world.gameObject.SetActive(true);
        HandleEvents(newWorld);

        if (_currentWorldPair.world != _nextWorldPair.world)
            _currentWorldPair.world.gameObject.SetActive(false);

        _currentWorldPair = _nextWorldPair;
        _nextWorldPair = null;
    }
    public void SwitchWorld(Emotion newWorld)
    {
        if (newWorld == _currentWorldPair.emotion || newWorld == Emotion.COUNT)
            return;

        _nextWorldPair = GetPair(newWorld);

        _nextWorldPair.world.gameObject.SetActive(true);
        HandleEvents(newWorld);

        StartCoroutine(SwitchWorldCoroutine(_nextWorldPair));
    }
    private IEnumerator SwitchWorldCoroutine(EmotionWorldPair newWorldPair)
    {
        OnWorldSwitchStart?.Invoke();
        IsTransitioning = true;

        var offset = newWorldPair.world.transform.position - _currentWorldPair.world.transform.position;
        var switchCoroutine = StartCoroutine(_cameraManager.TransitionToWorld(offset));
        yield return switchCoroutine;

        IsTransitioning = false;

        _rootToMove.transform.position = newWorldPair.world.transform.position;

        _currentWorldPair.world.gameObject.SetActive(false);
        _currentWorldPair = newWorldPair;
        _nextWorldPair = null;
    }

    // EVENTS
    //--------------------------------------------------
    private void UpdateButtons(Emotion newWorld)
    {
        var btns = _uiManager.GetUIGameObjects("PotionButtons");
        foreach (var btn in btns)
        {
            var btnComp = btn.GetComponent<Button>();
            btnComp.interactable = true;
        }
        btns[(int)newWorld].GetComponent<Button>().interactable = false;
    }
    private void HandleEvents(Emotion newWorld)
    {
        UpdateButtons(newWorld);

        var newPair = GetPair(newWorld);
        AudioManager.Instance.CrossFade(newPair.world.Music);

        if (_currentWorldPair.emotion == newWorld)
            return;
    }

    // HELPERS
    //--------------------------------------------------
    private EmotionWorldPair GetPair(Emotion emotion)
    {
        return _worldMap.FirstOrDefault(x => x.emotion == emotion);
    }
    public Emotion CurrentEmotion => _currentWorldPair.emotion;
}
