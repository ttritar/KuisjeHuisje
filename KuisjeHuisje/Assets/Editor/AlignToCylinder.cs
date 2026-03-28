using UnityEngine;
using UnityEditor;

public class AlignToCylinder : MonoBehaviour
{
	[Header("Cylinder Reference")]
	public Transform cylinder;

	[Header("Props to Align")]
	public Transform[] props;

	[Header("Cylinder Settings")]
	public float cylinderRadius = 1f; // If not auto-detected

	[ContextMenu("Align Props To Cylinder")]
	public void AlignProps()
	{
		if (cylinder == null || props == null || props.Length == 0)
		{
			Debug.LogWarning("Please assign a cylinder and at least one prop.");
			return;
		}

		Vector3 cylinderCenter = cylinder.position;

		foreach (var prop in props)
		{
			if (prop == null) continue;

			// Direction from center of cylinder to prop
			Vector3 direction = (prop.position - cylinderCenter);
			direction.y = 0; // Ignore height difference for circular alignment
			direction.Normalize();

			// Move prop to the cylinder surface
			Vector3 surfacePos = cylinderCenter + direction * cylinderRadius;
			surfacePos.y = prop.position.y; // Keep existing height
			prop.position = surfacePos;

			// Rotate prop to face outward from cylinder
			prop.rotation = Quaternion.LookRotation(direction, Vector3.up);
		}
	}
}

[CustomEditor(typeof(AlignToCylinder))]
public class AlignToCylinderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		AlignToCylinder aligner = (AlignToCylinder)target;

		if (GUILayout.Button("Align Props To Cylinder"))
		{
			aligner.AlignProps();
		}
	}
}
