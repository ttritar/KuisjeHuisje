using System;
using System.Collections.Generic;
using UnityEngine;

public class StickerWiggleManager : ISingleton<StickerWiggleManager>
{
    [SerializeField] private List<GameObject> _stickersParents;
    private Dictionary<StickerWiggle, CharacterSticker> _stickers = new Dictionary<StickerWiggle, CharacterSticker>();

    [SerializeField] private StickerInputManager _stickerInputManager;

    [System.Flags]
    public enum State
    {
        None = 0,
        Relationship = 1 << 0,
        Animal = 1 << 1,
        Emotion = 1 << 2,
    }
    public State CurrentState { get; set; } = State.Relationship;


    // START
    //------------------------------------------------
    private void OnEnable()
    {
        foreach (GameObject parent in _stickersParents)
        {
            foreach (Transform child in parent.transform)
            {
                StickerWiggle[] stickerWiggleArray = child.GetComponents<StickerWiggle>();
                foreach (var s in stickerWiggleArray)
                    RegisterSticker(s);
            }
        }
    }

    private void OnDisable()
    {
        foreach (var pair in _stickers)
            if (_stickers.ContainsKey(pair.Key))
                _stickers[pair.Key]?.OnCharacterCompleted.RemoveListener(RemoveChildWigglers);
        _stickers.Clear();
    }

    // HELPER
    //------------------------------------------------
    public void RegisterSticker(StickerWiggle w)
    {
        _stickers.TryAdd(w, w.GetComponent<CharacterSticker>());
        _stickers[w]?.OnCharacterCompleted.AddListener(RemoveChildWigglers);
    }
    public void DeregisterSticker(StickerWiggle w)
    {
        if(_stickers.ContainsKey(w))
            _stickers[w]?.OnCharacterCompleted.RemoveListener(RemoveChildWigglers);
        _stickers.Remove(w);
    }
    private void RemoveChildWigglers(CharacterSticker s)
    {
        var comps = s.GetComponentsInChildren<StickerWiggle>();
        foreach (var c in comps)
            DeregisterSticker(c);
    }


    // UPDATE
    //------------------------------------------------
    private void Update()
    {
        if(_stickerInputManager.IsIdle)
        {
            TriggerWiggle();
        }
    }

    // WIGGLE 
    //------------------------------------------------
    private void TriggerWiggle()
    {
        foreach (var pair in _stickers)
        {
            var wiggle = pair.Key;
            var data = pair.Value;

            bool isComplete = !data || (data?.CharData?.IsComplete ?? true);    
            bool isRoot = data && data.IsRoot;
            bool isInState = (wiggle && wiggle.enabled && ((CurrentState & wiggle.Id) != 0));

            if ((!isComplete && !isRoot) || isInState)
                StartCoroutine(wiggle.Wiggle());
        }
    }


    //--- Set States
    private void SetState(State state)
    {
        CurrentState = state;
    }
    public void SetStateAll() => SetState(State.Emotion | State.Animal | State.Relationship);
    public void SetStateRelationship() => SetState(State.Relationship);
    public void SetStateAnimal() => SetState(State.Animal);
    public void SetStateEmotion() => SetState(State.Emotion);

}
