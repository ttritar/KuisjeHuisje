using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CameraSwitchManager : ISingleton<CameraSwitchManager>
{
    [Header("Cameras")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Camera _secondaryCamera;
    private MimicCamera _cameraMimic;
    [SerializeField] private Camera _playerRenderCamera;

    [Header("Transition")]
    [SerializeField] private float _transitionSpeed = 1f;
    [SerializeField] private RawImage _screen;

    private Material _blendMat;
    private float _defaultValue = 0;
    private float _scale = 5;

    // START
    //--------------------------------------------------
    protected override void Awake()
    {
        base.Awake();

        _secondaryCamera.enabled = false;
        _cameraMimic = _secondaryCamera.GetComponent<MimicCamera>();
        _playerRenderCamera.enabled = false;

        _blendMat = _screen.material;
        _blendMat.SetFloat("_Scale", _defaultValue);
    }
    void Start()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    // TRANSITION
    //--------------------------------------------------
    public IEnumerator TransitionToWorld(Vector3 offset)
    {
        _cameraMimic.ChangeOffsetPosition(offset);
        _secondaryCamera.enabled = true;
        _playerRenderCamera.enabled = true;
        float t = 0;
        while (t < _scale)
        {
            t += Time.deltaTime * _transitionSpeed * _scale;
            _blendMat.SetFloat("_Scale", t);
            yield return null;
        }

        _blendMat.SetFloat("_Scale", _defaultValue);
        _playerRenderCamera.enabled = false;
        _secondaryCamera.enabled = false;
    }

    // CAMERA LIGHTS
    //--------------------------------------------------
    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        WorldData currentWorld = WorldSwitchManager.Instance.CurrentWorldPair.world;
        WorldData nextWorld = WorldSwitchManager.Instance.NextWorldPair?.world ?? null;

        if (camera == _mainCamera)
        {
            TurnEffects(currentWorld.Effects, true);
            if(nextWorld != null)
                TurnEffects(nextWorld.Effects, false);
        }
        else if (nextWorld != null && (camera == _playerRenderCamera || camera == _secondaryCamera))
        {
            TurnEffects(nextWorld.Effects, true);
            if (nextWorld != null)
                TurnEffects(currentWorld.Effects, false);
        }
    }

    void TurnEffects(List<GameObject> effects, bool on)
    {
        foreach (GameObject e in effects)
            e.SetActive(on);
    }

    // STOPPING
    //--------------------------------------------------
    void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    // CAMERA
    //--------------------------------------------------
    public Camera GetClosestCamera(Transform t)
    {
        var cam1 = Vector3.SqrMagnitude(t.position - _mainCamera.transform.position);
        var cam2 = Vector3.SqrMagnitude(t.position - _secondaryCamera.transform.position);

        return cam1 < cam2 ? _mainCamera : _secondaryCamera;
    }
}
