using UnityEngine;
using UnityEngine.InputSystem;
public class StickerInputManager : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    private DragAndDropPayload _currentPayload;
    private bool _isDraggingLeft = false;
    private bool _isDraggingRight = false;

    [Header("Raycast Settings")]
    [SerializeField] private float _raycastDistance = 100f;
    [SerializeField] private LayerMask _payloadLayerMask;

    [Header("Idle Settings")]
    [SerializeField] private float _idleThreshold = 2f;
    private float _idleTimer = 0f;
    public bool IsIdle => _idleTimer >= _idleThreshold;

    [Header("Player Feedback")]
    [SerializeField] private Texture2D _cursorHovered;
    [SerializeField] private Texture2D _cursorHeld;

    private CursorState _currentCursorState = CursorState.Default;
    private enum CursorState
    {
        Default,
        Hover,
        Held
    }

    // START
    //--------------------------------------------------
    private void Start()
    {
        _mainCamera = Camera.main;
    }


    private void Update()
    {
        // update idle timer
        if (_isDraggingLeft || _isDraggingRight)
            _idleTimer = 0f;
        else
            _idleTimer += Time.deltaTime;

        CursorState desiredState = CursorState.Default;

        if (_isDraggingLeft || _isDraggingRight)
        {
            desiredState = CursorState.Held;
        }
        else
        {
            Ray ray = _mainCamera.ScreenPointToRay(Pointer.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, _payloadLayerMask))
            {
                desiredState = CursorState.Hover;
            }
        }

        // update cursor if changed
        if (desiredState != _currentCursorState)
        {
            _currentCursorState = desiredState;

            switch (_currentCursorState)
            {
                case CursorState.Default:
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    break;
                case CursorState.Hover:
                    Cursor.SetCursor(_cursorHovered, Vector2.zero, CursorMode.Auto);
                    break;
                case CursorState.Held:
                    Cursor.SetCursor(_cursorHeld, Vector2.zero, CursorMode.Auto);
                    break;
            }
        }
    }


    // INPUT
    //--------------------------------------------------
    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (Pointer.current == null) return;


        // pressed
        if (context.performed)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Pointer.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit,_raycastDistance, _payloadLayerMask))
            {
                var payload = hit.collider.GetComponent<DragAndDropPayload>();
                if (payload != null)
                {
                    _currentPayload = payload;
                    _currentPayload.BeginLeftDrag(Pointer.current.position.ReadValue());

                    _isDraggingLeft = true;

                    // change cursor
                    Cursor.SetCursor(_cursorHeld, Vector2.zero, CursorMode.Auto);
                }
            }
        }
        // released
        else if (context.canceled&& _isDraggingLeft&& _currentPayload != null)
        {
            _currentPayload.EndLeftDrag();
            _currentPayload = null;
            _isDraggingLeft = false;

            // reset cursor
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (Pointer.current == null) return;

        // pressed
        if (context.performed)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Pointer.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, _payloadLayerMask))
            {
                var payload = hit.collider.GetComponent<DragAndDropPayload>();
                if (payload != null)
                {
                    _currentPayload = payload;
                    // CALL SOMETHING?
                    _isDraggingRight = true;

                    // change cursor
                    // Cursor.SetCursor(_cursorHeld, Vector2.zero, CursorMode.Auto);
                }
            }
        }
        // released
        else if (context.canceled && _isDraggingRight && _currentPayload != null)
        {
            // CALL SOMETHING?
            _currentPayload = null;
            _isDraggingRight = false;

            // reset cursor
            // Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void OnMousePositionChanged(InputAction.CallbackContext context)
    {
        if (Pointer.current == null) return;

        if (_isDraggingLeft && _currentPayload != null)
        {
            _currentPayload.UpdateLeftDrag(context.ReadValue<Vector2>());
        }
        else if (_isDraggingRight && _currentPayload != null)
        {
            // CALL SOMETHING?
        }
    }
}
