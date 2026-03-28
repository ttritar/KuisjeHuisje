using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class MapCharacterDisplay : MonoBehaviour
{
    [Header("Stickers")]
    [SerializeField] private GameObject _ellipsisSticker;
    [SerializeField] private GameObject[] _animalStickers = new GameObject[7];
    [SerializeField] private Sprite[] _emotionSprites = new Sprite[4];

    [Header("Spacing")]
    [SerializeField] private List<RectTransform> _basePos;
    [SerializeField] private float _horizontalSpacing = 125f;
    [SerializeField] private float _verticalSpacing = 125f;
    [SerializeField] private int _maxPerRow = 3;

    private Dictionary<Emotion, List<GameObject>> _stickers;
    private PlayerInput _input;
    private bool _hasInit = false;
    public static bool IsOpen { get; private set; } = false;

    // START
    //--------------------------------------------------
    private void Awake()
    {
        if(!_hasInit)
            Init();
    }
    private void Init()
    {
        _input = FindAnyObjectByType<PlayerInput>();
        _hasInit = true;
    }
    // FUNCTIONALITY
    //--------------------------------------------------
    private void SpawnIcons()
    {
        foreach (var w in WorldSwitchManager.Instance.WorldMap)
        {
            var copy = new List<CharacterData>(w.world.Characters);
            copy.Sort((a, b) =>
            {
                if (a.Relationship == "Stranger" && b.Relationship != "Stranger")
                    return 1;
                if (a.Relationship != "Stranger" && b.Relationship == "Stranger")
                    return -1;
                return 0;
            });

            var count = -1;
            if (copy.Count > _maxPerRow) count = _maxPerRow - 1;
            else count = Mathf.Min(_maxPerRow, copy.Count);

            for (int i = 0; i < count; i++)
            {
                var c = copy[i];
                var emotion = _emotionSprites[(int)c.Emotion];
                var obj = Instantiate(_animalStickers[(int)c.Animal], transform);

                var imgs = obj.GetComponentsInChildren<Image>();
                imgs[1].sprite = emotion;

                _stickers[w.emotion].Add(obj);
            }

            if (copy.Count > _maxPerRow)
            {
                var obj = Instantiate(_ellipsisSticker, transform);
                _stickers[w.emotion].Add(obj);
            }
        }
    }
    private void PositionStickers()
    {
        foreach (var sticker in _stickers)
        {
            var emotion = sticker.Key;
            var list = sticker.Value;
            var basePos = _basePos[(int)emotion];

            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i];

                int col = i % _maxPerRow;
                int row = i / _maxPerRow;

                RectTransform rt = obj.GetComponent<RectTransform>();
                float x = basePos.anchoredPosition.x + col * _horizontalSpacing;
                float y = basePos.anchoredPosition.y - row * _verticalSpacing;
                rt.anchoredPosition = new Vector2(x, y);
            }
        }
    }

    // EVENT
    //--------------------------------------------------
    public void CloseMap()
    {
        IsOpen = false;
        _input.ActivateInput();
        Time.timeScale = 1f;
    }
    public void OpenMap()
    {
        if (!_hasInit)
            Init();
        IsOpen = true;
        _input.DeactivateInput();
        Time.timeScale = 0f;

        if (_stickers == null)
            CreateLists();
        ClearLists();

        SpawnIcons();
        PositionStickers();
    }

    // HELPER
    //--------------------------------------------------
    private void ClearLists()
    {
        foreach (var s in _stickers)
        {
            foreach (var o in s.Value)
                Destroy(o.gameObject);
            s.Value.Clear();
        }
    }
    private void CreateLists()
    {
        _stickers = new Dictionary<Emotion, List<GameObject>>();
        foreach (Emotion e in Enum.GetValues(typeof(Emotion)))
            if (e != Emotion.COUNT)
                _stickers[e] = new List<GameObject>();
    }
}
