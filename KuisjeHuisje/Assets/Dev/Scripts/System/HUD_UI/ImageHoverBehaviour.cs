using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ImageHoverBehaviour : MonoBehaviour
{
    private Camera _mainCamera;
    
    [Header("Sprites")]
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _hoveredSprite;

    private Image _image;
    private RectTransform _rect;
    private Canvas _canvas;
    private PlayerInput _input;


    // START
    //----------------------------------------------
    void Awake()
    {
        _image = GetComponent<Image>();
        _rect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _mainCamera = Camera.main;

        _image.raycastTarget = false;

        _input = FindAnyObjectByType<PlayerInput>();
    }

    // UPDATE
    //----------------------------------------------
    void Update()
    {
        if (!_input.inputIsActive)
            return;

        Camera cam;

        if (_canvas)
            cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        else
            cam = _mainCamera;

        bool isHovering = RectTransformUtility.RectangleContainsScreenPoint(
            _rect,
            Input.mousePosition,
            cam
        );

        _image.sprite = isHovering ? _hoveredSprite : _normalSprite;
    }

}
