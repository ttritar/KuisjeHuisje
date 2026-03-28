using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ScreenTransition : MonoBehaviour
{
    [SerializeField] private Camera _secondaryCamera;
    [SerializeField] private float _resolutionScale = 1f;
    [SerializeField] private string _ref = "_Texture";

    private RawImage _image;
    private Material _mat;
    private RenderTexture _rt;

    private int _lastWidth;
    private int _lastHeight;

    // FUNCTIONALITY
    //--------------------------------------------------
    private void Awake()
    {
        _image = GetComponent<RawImage>();
        _mat = _image.material;

        UpdateRenderTexture();
    }

    private void Update()
    {
        if (Screen.width != _lastWidth || Screen.height != _lastHeight)
        {
            UpdateRenderTexture();
        }
    }

    // HELPER
    //--------------------------------------------------
    private void UpdateRenderTexture()
    {
        if (_rt != null)
        {
            _secondaryCamera.targetTexture = null;
            _rt.Release();
            Destroy(_rt);
        }

        _lastWidth = Screen.width;
        _lastHeight = Screen.height;

        _rt = new RenderTexture((int)(_lastWidth * _resolutionScale), (int)(_lastHeight * _resolutionScale), 16);
        _secondaryCamera.targetTexture = _rt;

        _mat.SetTexture(_ref, _rt);
    }


    // CLEAN UP
    //--------------------------------------------------
    private void OnDestroy()
    {
        if (_rt != null)
        {
            _rt.Release();
            Destroy(_rt);
        }
    }
}