using UnityEngine;

public class HairUVMover : MonoBehaviour
{
	[SerializeField] private HairColorPosition[] _positionsOfHairColors;

	private float _xOffset = 0.0f;
	private float _yOffset = 0.0f;
	private Mesh _mesh;
	private Vector2[] _uvs;

	public void ApplyHairColor(HairColor color, MeshFilter meshFilter)
	{
		if(!meshFilter)
			return;

		_mesh = Instantiate(meshFilter.sharedMesh);
		meshFilter.mesh = _mesh;

		foreach (var entry in _positionsOfHairColors)
		{
			if (entry.Color == color)
			{
				_xOffset = entry.Position.x;
				_yOffset = entry.Position.y;
				break;
			}
		}

		_uvs = _mesh.uv;
		for (int i = 0; i < _uvs.Length; i++)
		{
			_uvs[i].x += _xOffset;
			_uvs[i].y += _yOffset;
		}

		_mesh.uv = _uvs;
	}
}

[System.Serializable]
public struct HairColorPosition
{
	public HairColor Color;
	public Vector2 Position;
}

