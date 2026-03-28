using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] private Camera _secondaryCamera;
    [SerializeField] private Material _mat;
    [SerializeField] private string _ref = "_PlayerTexture";

    private Camera _cam;
    private RenderTexture _playerColorRT;

    private int _lastWidth;
    private int _lastHeight;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        UpdateRenderTexture();
    }

    private void Update()
    {
        if (Screen.width != _lastWidth || Screen.height != _lastHeight)
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            UpdateRenderTexture();
        }
    }

    private void UpdateRenderTexture()
    {
        if (_playerColorRT != null)
        {
            _cam.targetTexture = null;
            _playerColorRT.Release();
            DestroyImmediate(_playerColorRT);
        }

        _lastWidth = Screen.width;
        _lastHeight = Screen.height;

        _playerColorRT = new RenderTexture(_lastWidth, _lastHeight, 16, RenderTextureFormat.ARGB32);
        _playerColorRT.name = "PlayerColorRT";
        _cam.targetTexture = _playerColorRT;
        //_cam.SetTargetBuffers(
        //    _playerColorRT.colorBuffer,
        //    _secondaryCamera.targetTexture.depthBuffer
        //);
        _cam.clearFlags = CameraClearFlags.Color;

        if (_mat != null)
            _mat.SetTexture(_ref, _playerColorRT);
    }

    private void OnDestroy()
    {
        if (_playerColorRT != null)
        {
            _playerColorRT.Release();
            Destroy(_playerColorRT);
        }
    }
}