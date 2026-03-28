using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CustomRelationshipSticker : MonoBehaviour
{
    [SerializeField] private DragAndDropPayload _payload;
    [SerializeField] TextMeshPro _label;
    [SerializeField] TextMeshPro _labelBack;
    [SerializeField] private Collider _stickerCollider;
    private bool _isEditing = true;
    private bool _hasDuplicated = false;

    [Header("Player Feedback")]
    [SerializeField] private GameObject _editingOverlay;

    // ENABLE / DISABLE
    //--------------------------------------------------
    public void OnEnable()
    {
        _isEditing = true;
        _hasDuplicated = false;

        if (Keyboard.current != null)
            Keyboard.current.onTextInput += OnTextInput;

        // PLAYER FEEDBACK
        if (_editingOverlay != null)
            _editingOverlay.SetActive(true);
    }
    public void OnDisable()
    {
        _isEditing = false;

        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;

    }

    // EVENTS
    //--------------------------------------------------
    private void OnTextInput(char c)
    {
        if (Keyboard.current == null || !_isEditing)
            return;

        if (_isEditing && c != '\b' && c != '\n' && c != '\r')
        {
            _label.text += c;
            _labelBack.text += c;
            _payload.PayloadMessage = _label.text;
        }
        else if (c == '\b')
        {
            if (_label.text.Length <= 0) return;

            _label.text = _label.text.Substring(0, _label.text.Length - 1);
            _labelBack.text = _labelBack.text.Substring(0, _labelBack.text.Length - 1);
            _payload.PayloadMessage = _label.text;
        }
    }
    public void OnBeginDrag()
    {
        if (_isEditing)
        {
            StopEditingAndDuplicate();
        }
    }
    private void StopEditingAndDuplicate()
    {
        _isEditing = false;

        if (!_hasDuplicated)
        {
            DuplicateSticker();
            _hasDuplicated = true;

            // PLAYER FEEDBACK
            if (_editingOverlay != null)
                _editingOverlay.SetActive(false);
        }

        this.enabled = false;
    }

    private void DuplicateSticker()
    {
        GameObject duplicate = Instantiate(gameObject, transform.parent);
        duplicate.transform.position = transform.position;
        duplicate.transform.rotation = transform.rotation;

        // disable typing on duplicate
        CustomRelationshipSticker duplicateSticker = duplicate.GetComponent<CustomRelationshipSticker>();
        if (duplicateSticker != null)
        {
            duplicateSticker.enabled = true;
        }

        // clear label on duplicate
        TextMeshPro dupLabel = duplicate.GetComponentInChildren<TextMeshPro>();
        if (dupLabel != null)
        {
            dupLabel.text = "";
        }

        // reset payload 
        DragAndDropPayload dupPayload = duplicate.GetComponent<DragAndDropPayload>();
        if (dupPayload != null)
        {
            dupPayload.PayloadMessage = "";
            dupPayload.DuplicateOnDrag = false;
        }
    }
}
