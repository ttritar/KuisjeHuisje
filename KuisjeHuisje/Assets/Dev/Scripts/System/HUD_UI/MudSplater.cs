using System;
using System.Collections;
using UnityEngine;

public class MudSplater : MonoBehaviour
{
	[SerializeField] private GameObject[] _mudSplats;
	[SerializeField] private float _moveSpeed = 1f;
	[SerializeField] private float _timeOnScreen = 2f;
	private float _timeCounter = 0f;
	private Vector3[] _initialPosition = new Vector3[5];
	private float[] _randomSpeeds = new float[5];

	// START
	//--------------------------------------------------
	private void Start()
	{
		_initialPosition = new Vector3[_mudSplats.Length];
		for (int i = 0; i < _mudSplats.Length; i++)
		{
			_mudSplats[i].SetActive(false);
			_initialPosition[i] = _mudSplats[i].GetComponent<RectTransform>().position;
			_mudSplats[i].GetComponent<CanvasGroup>().alpha = 1f;
			_mudSplats[i].GetComponent<UIFade>().enabled = false;
			_mudSplats[i].GetComponent<UIBounceIn>().enabled = false;
		}
		var houses = Resources.FindObjectsOfTypeAll<HouseCleaningBehaviour>();

		foreach (var house in houses)
		{
			if (house.gameObject.scene.IsValid())
			{
				house.OnCleanRegressed.AddListener(EnableMudSplat);
				house.OnCleanComplete.AddListener(DisableMudSplashes);
			}
		}
	}
	// MOVE MUD SPLATS
	//--------------------------------------------------
	private void Update()
	{
		if (_timeCounter <= _timeOnScreen)
		{
			_timeCounter += Time.deltaTime;
			for (int i = 0; i < _mudSplats.Length; i++)
			{
				if (_mudSplats[i].activeSelf)
				{
					_mudSplats[i].GetComponent<RectTransform>().position += Vector3.down * _moveSpeed * Time.deltaTime * _randomSpeeds[i];
				}
			}
		}
		else
		{
			DisableMudSplashes();
		}
	}
	// ENABLE MUD SPLATS
	//--------------------------------------------------
	public void EnableMudSplat()
	{
		for (int i = 0; i < _mudSplats.Length; i++)
        {
            _timeCounter = 0f;
            _randomSpeeds[i] = UnityEngine.Random.Range(0.5f, 2f);
            _mudSplats[i].SetActive(true);
            _mudSplats[i].GetComponent<RectTransform>().position = _initialPosition[i];
            _mudSplats[i].GetComponent<UIBounceIn>().enabled = true;
			_mudSplats[i].GetComponent<UIFade>().enabled = true;
			_mudSplats[i].GetComponent<UIFade>().StopAllCoroutines();
			_mudSplats[i].GetComponent<CanvasGroup>().alpha = 1f;
			StartCoroutine(StartFadeAfterTime(i));
        }
    }

    private IEnumerator StartFadeAfterTime(int i)
    {
		yield return new WaitForSeconds(0.25f);
        _mudSplats[i].GetComponent<UIFade>().Fade(false);
    }

    public void DisableMudSplashes()
	{
		foreach (var mudSplat in _mudSplats)
		{
			mudSplat.SetActive(false);
			mudSplat.GetComponent<CanvasGroup>().alpha = 1f;
		}
	}
    private void OnDisable()
    {
        DisableMudSplashes();
	}
}
