using UnityEngine;

public class RebindToSkeleton : MonoBehaviour
{
	[SerializeField] private SkinnedMeshRenderer _thisSkinnedRenderer;
	public void StartRebinding(SkinnedMeshRenderer CharacterRenderer)
	{
		if (!_thisSkinnedRenderer || !CharacterRenderer) return;

		_thisSkinnedRenderer.rootBone = CharacterRenderer.rootBone;
		_thisSkinnedRenderer.bones = CharacterRenderer.bones;

		Debug.Log("T-shirt bound to character skeleton!");
	}
}
