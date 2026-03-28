using UnityEngine;

public class ClothingSelection : MonoBehaviour
{
	[SerializeField] private ClothingTop[] _clothingTopItems;
    [SerializeField] private ClothingBottom[] _clothingBottomItems;
    [SerializeField] private HairStyle[] _hairStyles;
	[SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] private GameObject _hairParent;

	private GameObject[] _instantiatedClothing;
    private GameObject _instantiatedHair;
    public GameObject InstantiatedHair => _instantiatedHair;

    // SELECT CLOTHING
    //--------------------------------------------------
    public void ApplyAll(Top top, Bottom bottom, Hair hair)
    {
        if(_instantiatedHair)
            Destroy(_instantiatedHair);
        _instantiatedHair = null;

        if (_instantiatedClothing != null)
        {
            foreach (var clothing in _instantiatedClothing)
                Destroy(clothing);
        }
        _instantiatedClothing = new GameObject[3];

        // INSTANTIATE SELECTED TOP

        foreach (var item in _clothingTopItems)
        {
            if (item.Clothing != top) continue;
            ApplyTop(item.Prefab);
            break;
        }

        // INSTANTIATE SELECTED PANTS

        foreach (var item in _clothingBottomItems)
        {
            if(bottom != Bottom.COUNT && item.Clothing == bottom)
            {
                ApplyPants(item.Prefab);
                break;
			}
			if (bottom != Bottom.COUNT) continue;
            break;
        }

        // INSTANTIATE SELECTED HAIRSTYLE
        foreach (var item in _hairStyles)
        {
            if (hair != Hair.Bald && item.Hair == hair)
            {
                ApplyHair(item.Prefab);
                break;
            }

            if (hair != Hair.Bald) continue;
            ApplyHair(null);
            break;
        }
    }
    private void ApplyTop(GameObject prefab)
    {
        if (_instantiatedClothing == null || _instantiatedClothing.Length <= 0)
            return;

        if(_instantiatedClothing[0] != null)
            Destroy(_instantiatedClothing[0]);
        _instantiatedClothing[0] = Instantiate(prefab, transform);

        if (_instantiatedClothing[0] == null)
            return;

        _instantiatedClothing[0].SetActive(true);
        _instantiatedClothing[0].GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        _instantiatedClothing[0].GetComponentInChildren<Animator>().enabled = true;
        _instantiatedClothing[0].GetComponent<RebindToSkeleton>().StartRebinding(_skinnedMeshRenderer);

    }
    private void ApplyPants(GameObject prefab)
    {
        if (_instantiatedClothing == null || _instantiatedClothing.Length <= 1)
            return;

        if (_instantiatedClothing[1] != null)
            Destroy(_instantiatedClothing[1]);
        _instantiatedClothing[1] = Instantiate(prefab, transform);

        if (_instantiatedClothing[1] == null)
            return;

        _instantiatedClothing[1].SetActive(true);
        _instantiatedClothing[1].GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        _instantiatedClothing[1].GetComponentInChildren<Animator>().enabled = true;
        _instantiatedClothing[1].GetComponent<RebindToSkeleton>().StartRebinding(_skinnedMeshRenderer);
    }
    private void ApplyHair(GameObject prefab)
    {
        if (_instantiatedHair != null)
            Destroy(_instantiatedHair);

        _instantiatedHair = prefab != null ? Instantiate(prefab) : null;

        if (_instantiatedHair == null) return;

        _instantiatedHair.SetActive(true);
        _instantiatedHair.transform.SetParent(_hairParent.transform, worldPositionStays: false);
    }
}
[System.Serializable]
public struct ClothingTop
{
    public Top Clothing;
    public GameObject Prefab;
}
[System.Serializable]
public struct ClothingBottom
{
	public Bottom Clothing;
	public GameObject Prefab;
}
[System.Serializable]
public struct HairStyle
{
	public Hair Hair;
	public GameObject Prefab;
}
