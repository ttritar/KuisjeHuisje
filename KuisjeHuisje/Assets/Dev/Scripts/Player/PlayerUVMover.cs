using UnityEngine;

public class PlayerUVMover : MonoBehaviour
{
	[SerializeField] private SkinColorPosition[] _positionsOfSkinColors;
	[SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
	private float _xOffset = 0.0f;
	private Mesh _mesh;
	private Vector2[] _uvs;

    public void ApplySkinColor(SkinColor color)
    {
        _mesh = Instantiate(_skinnedMeshRenderer.sharedMesh);
        _skinnedMeshRenderer.sharedMesh = _mesh;
        foreach (var entry in _positionsOfSkinColors)
        {
            if (entry.Color == color)
            {
                _xOffset = entry.Position;
                break;
            }
        }

        _uvs = _mesh.uv;
        for (int i = 0; i < _uvs.Length; i++)
        {
            _uvs[i].x += _xOffset;
        }

        _mesh.uv = _uvs;
    }
}
[System.Serializable]
public struct SkinColorPosition
{
	public SkinColor Color;
	public float Position;
}
