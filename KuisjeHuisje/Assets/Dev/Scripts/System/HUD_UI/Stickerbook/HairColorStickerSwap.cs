using System;
using UnityEngine;

public class HairColorStickerSwap: MonoBehaviour
{
    [SerializeField] private Renderer _hairRenderer;
     
    [SerializeField] private Texture _blackHair;
    [SerializeField] private Texture _brownHair;
    [SerializeField] private Texture _blondeHair;
    [SerializeField] private Texture _redHair;

    private const string HAIR_TEXTURE_PROPERTY = "_texture";

    // START
    //--------------------------------------------------
    private void Start()
    {
        _hairRenderer.material = new Material(_hairRenderer.material);
    }

    // FUNCTIONALITY
    //--------------------------------------------------
    public void SetColor(HairColor color)
    {
        switch (color)
        {
            case HairColor.Black:
                _hairRenderer.material.SetTexture(HAIR_TEXTURE_PROPERTY,_blackHair);
                break;
            case HairColor.Brown:
                _hairRenderer.material.SetTexture(HAIR_TEXTURE_PROPERTY, _brownHair);
                break;
            case HairColor.Blonde:
                _hairRenderer.material.SetTexture(HAIR_TEXTURE_PROPERTY, _blondeHair);
                break;
            case HairColor.Red:
                _hairRenderer.material.SetTexture(HAIR_TEXTURE_PROPERTY, _redHair);
                break;
        }
    }

    public void SetColorBlack()
    {
        SetColor(HairColor.Black);
    }
    public void SetColorBrown()
    {
        SetColor(HairColor.Brown);
    }
    public void SetColorBlonde()
    {
        SetColor(HairColor.Blonde);
    }
    public void SetColorRed()
    {
        SetColor(HairColor.Red);
    }

}
