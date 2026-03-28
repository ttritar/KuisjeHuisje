using UnityEngine;

public class CharacterBodyColorSwitch : MonoBehaviour
{
	[SerializeField] private CharacterSkinColorPosition[] _positionsOfColors;
	[SerializeField] public CharacterSkinColor CurrentAnimal;
	[SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
	private Vector2 _offset = Vector2.zero;
	private Mesh _mesh;
	private Vector2[] _uvs;

	void Start()
	{
		_mesh = Instantiate(_skinnedMeshRenderer.sharedMesh);
		_skinnedMeshRenderer.sharedMesh = _mesh;

		foreach (var entry in _positionsOfColors)
		{
			if (entry.Color == CurrentAnimal)
			{
				_offset = entry.Position;
				break;
			}
		}

		_uvs = _mesh.uv;
		for (int i = 0; i < _uvs.Length; i++)
		{
			_uvs[i].x += _offset.x;
			_uvs[i].y += _offset.y;
		}

		_mesh.uv = _uvs;
	}
}
[System.Serializable]
public struct CharacterSkinColorPosition
{
	public CharacterSkinColor Color;
	public Vector2 Position;
}
public enum CharacterSkinColor
{
	Bear,
	Bunny,
	Cat,
	Dog,
	Frog,
	Lion,
	Sheep,
	Wizard
}
