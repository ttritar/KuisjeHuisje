using UnityEngine;

[System.Serializable]
public class PlayerData : MonoBehaviour
{
    // SELECTION DATA
    public Hair Hair
    {
        get => _hair;
        set => _hair = value;
    }
    [SerializeField] private Hair _hair = Hair.Bald;
    public HairColor HairColor
    {
        get => _hairColor;
        set => _hairColor = value;
    }
    [SerializeField] private HairColor _hairColor = HairColor.Red;
    public SkinColor SkinColor
    {
        get => _skinColor;
        set => _skinColor = value;
    }
    [SerializeField] private SkinColor _skinColor = SkinColor.White;
    public Emotion Emotion
    {
        get => _emotion;
        set => _emotion = value;
    }
    [SerializeField] private Emotion _emotion = Emotion.COUNT;
    public Top Top
    {
        get => _top;
        set => _top = value;
    }
    [SerializeField] private Top _top = Top.COUNT;
    public Bottom Bottom
    {
        get => _bottom;
        set => _bottom = value;
    }
    [SerializeField] private Bottom _bottom = Bottom.COUNT;

    // APPLY NEW
    //--------------------------------------------------
    public void ApplyNewData()
    {
        var hairColor = GetComponent<HairUVMover>();
        var skinColor = GetComponent<PlayerUVMover>();
        var clothing = GetComponent<ClothingSelection>();
        var emotionsSwap = GetComponent<CharacterEmotionsSwap>();

        // clothes + hair type
        if (clothing)
        {
            clothing.ApplyAll(Top, Bottom, Hair);
        }
        else Debug.LogWarning("[PlayerSticker] ClothingSelection component not found on PlayerPrefab.", this);

        // emotion
        if (emotionsSwap)
        {
            emotionsSwap.ApplyEmotion(Emotion);
        }
        else
            Debug.LogWarning("[PlayerSticker] CharacterEmotionsSwap component not found on PlayerPrefab.", this);

        // skin color
        if (skinColor)
        {
            skinColor.ApplySkinColor(SkinColor);
        }
        else
            Debug.LogWarning("[PlayerSticker] PlayerUVMover component not found on PlayerPrefab.", this);

        // hair color
        if (hairColor)
        {
            hairColor.ApplyHairColor(HairColor, clothing.InstantiatedHair?.GetComponentInChildren<MeshFilter>());
        }
        else
            Debug.LogWarning("[PlayerSticker] HairUVMover component not found on PlayerPrefab.", this);

    }

    // FUNCTIONALITY
    //--------------------------------------------------
    public bool IsStringInEnum<T>(string str, out T enumValue) where T : struct, System.Enum
    {
        if (System.Enum.TryParse<T>(str, out enumValue))
        {
            return true;
        }
        enumValue = default;
        return false;
    }
}


public enum Hair
{
    Bald,
    Short,
    Ponytail,
    WizardHat,

	COUNT
}
public enum HairColor
{
    Black,
    Brown,
    Blonde,
    Red,

    COUNT
}

public enum Top
{
    TShirt,
    Longsleeve,
    WizardCloak,

    COUNT
}
public enum Bottom
{
    Shorts,
    Pants,
    SkirtRuffles,
    SkirtStraight,

	COUNT
}
public enum SkinColor
{
    White,
    Medium,
    Brown,
    DarkBrown,

    COUNT
}
