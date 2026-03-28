using UnityEngine;
using System.Collections.Generic;

public class GrassSpawner : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private MeshFilter _targetMeshFilter;       // Cylinder mesh
	[SerializeField] private GameObject _grassPrefab;            // Prefab to spawn
	[SerializeField] private Material[] _targetMaterials;        // Materials to spawn grass on


	[Header("Spawn Settings")]
	[SerializeField] private int _grassPerFace = 5;              // How many grass instances per triangle
	[SerializeField] private float _randomOffset = 0.1f;         // Small random offset for variation
	[SerializeField] private Vector2 _randomScaleRange = new Vector2(0.8f, 1.2f);
	[SerializeField] private float _spawnRangeAfterZAxis; // Not used in this version
	[SerializeField] private float _zOffset;

    // START
    //--------------------------------------------------
    void Start()
	{
		// Fixed condition: we want to stop if any references are missing or no target materials
		if (!_targetMeshFilter || !_grassPrefab || _targetMaterials == null || _targetMaterials.Length == 0)
		{
			Debug.LogError("Missing references — assign MeshFilter, Grass Prefab, and at least one Target Material.");
			return;
		}

		SpawnGrass();
	}

    // SPAWNIGN
    //--------------------------------------------------
    void SpawnGrass()
	{
		Mesh mesh = _targetMeshFilter.sharedMesh;
		if (!mesh)
		{
			Debug.LogError("No mesh found on target MeshFilter.");
			return;
		}

		// Get mesh data
		Vector3[] vertices = mesh.vertices;
		Material[] materials = _targetMeshFilter.GetComponent<MeshRenderer>().sharedMaterials;

		Transform parent = new GameObject("Grass_Instances").transform;
		parent.SetParent(transform, true);

		// Find all submesh indices that match any of the target materials
		List<int> targetSubmeshIndices = new List<int>();
		for (int i = 0; i < materials.Length; i++)
		{
			foreach (var mat in _targetMaterials)
			{
				if (materials[i] == mat)
				{
					targetSubmeshIndices.Add(i);
					break;
				}
			}
		}

		if (targetSubmeshIndices.Count == 0)
		{
			Debug.LogWarning("None of the target materials were found on the mesh.");
			return;
		}

		// Loop through all matching submeshes
		foreach (int submeshIndex in targetSubmeshIndices)
		{
			int[] subTriangles = mesh.GetTriangles(submeshIndex);

			for (int i = 0; i < subTriangles.Length; i += 3)
			{
				Vector3 v0 = vertices[subTriangles[i]];
				Vector3 v1 = vertices[subTriangles[i + 1]];
				Vector3 v2 = vertices[subTriangles[i + 2]];

				// Spawn multiple grass instances per face
				for (int j = 0; j < _grassPerFace; j++)
				{
					// Random barycentric coordinates
					float u = Random.value;
					float v = Random.value;
					if (u + v > 1f)
					{
						u = 1f - u;
						v = 1f - v;
					}
					float w = 1f - u - v;

					Vector3 localPos = v0 * u + v1 * v + v2 * w;
					Vector3 worldPos = _targetMeshFilter.transform.TransformPoint(localPos);

					// Check spawn range along Z axis
					if (_spawnRangeAfterZAxis < worldPos.z)
					{
						break;
					}

					// Small random offset for natural variation
					worldPos += new Vector3(
						Random.Range(-_randomOffset, _randomOffset),
						0f,
						Random.Range(-_randomOffset, _randomOffset)
					);

					// Instantiate grass
					GameObject grass = Instantiate(_grassPrefab, worldPos, Quaternion.identity, parent);

					// Align to face normal
					Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
					Vector3 worldNormal = _targetMeshFilter.transform.TransformDirection(normal);
					grass.transform.up = worldNormal;
					grass.transform.position += -grass.transform.up * -_zOffset; 

					// Random Y rotation + scale
					grass.transform.Rotate(Vector3.up, Random.Range(0f, 360f), Space.Self);
					float scale = Random.Range(_randomScaleRange.x, _randomScaleRange.y);
					grass.transform.localScale *= scale;
				}
			}
		}

		Debug.Log($"Spawned grass on {targetSubmeshIndices.Count} material(s).");
	}
}
