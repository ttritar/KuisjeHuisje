using System;
using System.Collections.Generic;
using UnityEngine;

public class StrikeCounter : MonoBehaviour
{
    [SerializeField] private GameObject _strikePrefab;
    [SerializeField] private float _spacing = 80f;
    [SerializeField] private RectTransform _parent;
    [SerializeField][Range(0f, 1f)] private float _alpha;
    private int _maxStrikes = 0;
    private List<GameObject> _uiList = new List<GameObject>();

    // START
    //--------------------------------------------------
    private void Reinit()
    {
        foreach (var ui in _uiList)
            Destroy(ui);
        _uiList.Clear();

        for (int i = 0; i < _maxStrikes; i++)
        {
            float offset = (i - (_maxStrikes - 1) * 0.5f) * _spacing;

            var strike = Instantiate(_strikePrefab, _parent);
            RectTransform rt = strike.GetComponent<RectTransform>();

            rt.anchoredPosition = new Vector2(offset, 0f);

            foreach (Transform obj in strike.transform)
                obj.transform.localScale = Vector3.one;

            _uiList.Add(strike.GetComponentInChildren<UIBounceIn>().gameObject);
        }

    }

    // EVENTS
    //--------------------------------------------------
    public void OnUpdateStrikes(int strikeCount, int maxCount)
    {
        if (maxCount != _maxStrikes)
        {
            _maxStrikes = maxCount;
            Reinit();
        }

        for (int i = 0; i < _uiList.Count; i++)
            _uiList[i].SetActive(i < strikeCount);
    }
}
