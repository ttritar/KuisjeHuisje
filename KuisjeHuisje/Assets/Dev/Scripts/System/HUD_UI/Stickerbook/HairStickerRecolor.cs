using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HairStickerRecolor : MonoBehaviour
{
    private Button _button;
    [SerializeField] private HairColor _color;

    [SerializeField] private List<GameObject> _hairStickersParentGameObjects;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        List<HairColorStickerSwap> hairStickers = new List<HairColorStickerSwap>();
        foreach (var obj in _hairStickersParentGameObjects)
        {
            foreach (var sticker in obj.GetComponentsInChildren<HairColorStickerSwap>())
            {
                hairStickers.Add(sticker);
            }
        }
        if (hairStickers.Count != 0)
        {
            foreach (var hair in hairStickers)
            {
                hair.SetColor(_color);
            }
        }
    }
}
