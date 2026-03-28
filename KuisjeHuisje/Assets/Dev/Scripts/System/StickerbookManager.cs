using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class StickerbookManager : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private Book _book;

    [Header("Render Textures")]
    [SerializeField] private RenderTexture _mainRenderTexture;
    [SerializeField] private RenderTexture _secondaryRenderTexture;
    [SerializeField] private Vector3 _offset;

    [Header("Pages")]
    [FormerlySerializedAs("_uiPagePairs")]
    [SerializeField] private List<WorldObject> _worldObjects;
    [FormerlySerializedAs("_uiPageObjPairs")]
    [SerializeField] private List<StickerObject> _stickerObjects;
    private int _dir = -1;
    [Serializable] struct WorldObject
    {
        public int pageIndex;
        public string uiKey;
    }
    [Serializable] struct StickerObject
    {
        public int pageIndex;
        public GameObject uiObj;
    }

    // START
    //--------------------------------------------------
    private void Start()
    {
        _book.OnFlipStart.AddListener(OnBeginFlip);
        foreach (var ui in _stickerObjects)
        {
            ui.uiObj.transform.position += _offset;
            ui.uiObj.SetActive(false);
        }

        int currentPage = _book.currentPage;
        SetRenderTextures(currentPage);
        UpdatePageObjectsOnFlipStart(currentPage);
    }

    // EVENT
    //--------------------------------------------------
    public void OnBeginFlip(int dir)
    {
        _dir = dir;
        int currentPage = _book.currentPage;

        SetRenderTextures(currentPage);
        UpdatePageObjectsOnFlipStart(currentPage);
        SetUIForPage(currentPage, false);
    }
    public void OnEndFlip()
    {
        int currentPage = _book.currentPage;

        UpdatePageObjectsOnFlipEnd(currentPage);
        SetUIForPage(currentPage, true);
        ResetRenderTextures();

        _dir = 0;
    }

    // HELPER
    //--------------------------------------------------
    private bool IsValidBookIndex(int pageIndex)
    {
        int pageCount = _book.bookPages.Length;
        return pageIndex - 1 >= 0 && pageIndex - 1 < pageCount;
    }

    private void ResetRenderTextures()
    {
        for (int i = 0; i < _book.bookPages.Length; i++)
            _book.bookPages[i] = _mainRenderTexture;
        _book.background = _mainRenderTexture;
        _book.ForceUpdateTextures(_dir);
    }
    private void SetRenderTextures(int pageIndex)
    {
        int index = pageIndex - 1;
        if (IsValidBookIndex(index))
            _book.bookPages[index] = _mainRenderTexture;

        index = pageIndex;
        if (IsValidBookIndex(index))
            _book.bookPages[index] = _mainRenderTexture;

        if (_dir == 1)
        {
            index = pageIndex + 1;
            if (IsValidBookIndex(index))
                _book.bookPages[index] = _secondaryRenderTexture;
                index = pageIndex + 2;
            if (IsValidBookIndex(index))
                _book.bookPages[index] = _secondaryRenderTexture;
        }
        else if (_dir == -1)
        {
            index = pageIndex - 2;
            if (IsValidBookIndex(index))
                _book.bookPages[index] = _secondaryRenderTexture;
            else
            {
                _book.bookPages[0] = _secondaryRenderTexture;
                _book.background = _secondaryRenderTexture;
            }
            index = pageIndex - 3;
            if (IsValidBookIndex(index))
                _book.bookPages[index] = _secondaryRenderTexture;
            else
            {
                _book.bookPages[0] = _secondaryRenderTexture;
                _book.background = _secondaryRenderTexture;
            }
        }

        _book.ForceUpdateTextures(_dir);
    }
    private void UpdatePageObjectsOnFlipStart(int pageIndex)
    {
        foreach (var pageObj in _stickerObjects)
        {
            if (pageObj.pageIndex == pageIndex && !pageObj.uiObj.activeSelf)
            {
                pageObj.uiObj.transform.position -= _offset;
                pageObj.uiObj.SetActive(true);
            }
            else if (pageObj.pageIndex == pageIndex + 2 * _dir)
            {
                pageObj.uiObj.SetActive(true);
            }
        }
    }
    private void UpdatePageObjectsOnFlipEnd(int pageIndex)
    {
        foreach (var pageObj in _stickerObjects)
        {
            if (pageObj.pageIndex == pageIndex)
            {
                pageObj.uiObj.transform.position -= _offset;
                pageObj.uiObj.SetActive(true);
            }
            else if (pageObj.pageIndex == pageIndex - 2 * _dir)
            {
                pageObj.uiObj.transform.position += _offset;
                pageObj.uiObj.SetActive(false);
            }
        }
    }
    private void SetUIForPage(int pageIndex, bool enable)
    {
        foreach (var ui in _worldObjects)
        {
            if (ui.pageIndex != pageIndex)
                continue;

            if (enable) _uiManager.EnableUI(ui.uiKey);
            else _uiManager.DisableUI(ui.uiKey);
        }
    }
}
