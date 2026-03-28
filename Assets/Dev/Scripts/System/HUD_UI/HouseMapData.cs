using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HouseMapData : MonoBehaviour
{
    [SerializeField] private List<GameObject> _peopleStickers;
    public int StickerCount => _peopleStickers.Count(obj => obj.activeSelf);

    // HELPER
    //--------------------------------------------------
    public void UpdateStickerDisplay(int count)
    {
        foreach (var sticker in _peopleStickers)
            sticker.SetActive(false);
        for (int i = 0; i < Mathf.Min(count, _peopleStickers.Count); i++)
            _peopleStickers[i].SetActive(true);
    }
}
