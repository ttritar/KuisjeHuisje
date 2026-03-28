using UnityEngine;

public class SkinColorStickerSwap : MonoBehaviour
{
    [SerializeField] private Renderer _skinRenderer;

    private static readonly string KW_WHITE = "_SKINTYPE_WHITE";
    private static readonly string KW_MEDIUM = "_SKINTYPE_MEDIUM";
    private static readonly string KW_BROWN = "_SKINTYPE_BROWN";
    private static readonly string KW_DARK = "_SKINTYPE_DARKBROWN";

    private void Start()
    {
        _skinRenderer.material = Instantiate(_skinRenderer.material);
    }

    public void SetSkinColor(int skinType)
    {
        Material mat = _skinRenderer.material;
        
        // disable
        mat.DisableKeyword(KW_WHITE);
        mat.DisableKeyword(KW_MEDIUM);
        mat.DisableKeyword(KW_BROWN);
        mat.DisableKeyword(KW_DARK);

        // enable the selected one
        switch (skinType)
        {
            case 0: mat.EnableKeyword(KW_WHITE); break;
            case 1: mat.EnableKeyword(KW_MEDIUM); break;
            case 2: mat.EnableKeyword(KW_BROWN); break;
            case 3: mat.EnableKeyword(KW_DARK); break;
        }
    }
}