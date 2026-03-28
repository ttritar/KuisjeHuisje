using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class MapHouseDisplay : MonoBehaviour
{
    [Header("Stickers")]
    [SerializeField] private GameObject _houseStickerPrefab;

    [Header("Spacing")]
    [SerializeField] private List<RectTransform> _basePos;
    [SerializeField] private float _horizontalSpacing = 125f;
    [SerializeField] private float _verticalSpacing = 125f;
    [SerializeField] private int _maxPerRow = 5;

    private Dictionary<Emotion, List<HouseMapData>> _stickers;
    private bool _hasSpawnedStickers = false;
    public static bool IsOpen { get; private set; } = false;

    // AWAKE
    //--------------------------------------------------
    private void Awake()
    {
        if(_stickers == null)
            CreateLists();
        SpawnIcons();
        PositionStickers();
        UpdateHouseCounter();
    }

    // FUNCTIONALITY
    //--------------------------------------------------
    private void SpawnIcons()
    {
        if (_hasSpawnedStickers)
            return;
        foreach (var w in WorldSwitchManager.Instance.WorldMap)
        {
            foreach (var house in w.world.Houses)
            {
                var obj = Instantiate(_houseStickerPrefab, transform);
                var comp = obj.GetComponentInChildren<HouseMapData>();
                if (comp) _stickers[w.emotion].Add(comp);
                else Destroy(obj);
            }
        }

        _hasSpawnedStickers = true;
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

                RectTransform rt = obj.GetComponentInParent<RectTransform>();
                float x = basePos.anchoredPosition.x + col * _horizontalSpacing;
                float y = basePos.anchoredPosition.y - row * _verticalSpacing;
                rt.anchoredPosition = new Vector2(x, y);
            }
        }
    }

    private void UpdateHouseCounter()
    {
        foreach (var w in WorldSwitchManager.Instance.WorldMap)
        {
            for (int i = 0; i < w.world.Houses.Count; i++)
            {
                var house = w.world.Houses[i];
                var sticker = _stickers[w.emotion][i];
                sticker.UpdateStickerDisplay(house.AssignedCharacterCount);
            }
        }
    }

    // EVENT
    //--------------------------------------------------
    public void CloseMap()
    {
        IsOpen = false;
        Time.timeScale = 1f;
    }
    public void OpenMap()
    {
        IsOpen = true;
        Time.timeScale = 0f;
        if (_stickers == null)
            CreateLists();
        SpawnIcons();
        PositionStickers();
        UpdateHouseCounter();
    }

    // HELPER
    //--------------------------------------------------
    private void CreateLists()
    {
        _stickers = new Dictionary<Emotion, List<HouseMapData>>();
        foreach (Emotion e in Enum.GetValues(typeof(Emotion)))
            if (e != Emotion.COUNT)
                _stickers[e] = new List<HouseMapData>();
    }

}
