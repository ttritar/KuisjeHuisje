using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TreeSpawnerEditorWindow : EditorWindow
{
    // Assign your prefabs in the Editor window
    public GameObject treePrefabHappy;
    public GameObject treePrefabHappySmall;
    public GameObject treePrefabSad;
    public GameObject treePrefabSadSmall;
    public GameObject treePrefabAngry;
    public GameObject treePrefabAngrySmall;

    public GameObject treePositionsParent;

    [MenuItem("Tools/Tree Spawner")]
    public static void ShowWindow()
    {
        GetWindow<TreeSpawnerEditorWindow>("Tree Spawner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tree Prefabs", EditorStyles.boldLabel);

        treePrefabHappy = (GameObject)EditorGUILayout.ObjectField("Happy Tree", treePrefabHappy, typeof(GameObject), false);
        treePrefabHappySmall = (GameObject)EditorGUILayout.ObjectField("Happy Small Tree", treePrefabHappySmall, typeof(GameObject), false);
        treePrefabSad = (GameObject)EditorGUILayout.ObjectField("Sad Tree", treePrefabSad, typeof(GameObject), false);
        treePrefabSadSmall = (GameObject)EditorGUILayout.ObjectField("Sad Small Tree", treePrefabSadSmall, typeof(GameObject), false);
        treePrefabAngry = (GameObject)EditorGUILayout.ObjectField("Angry Tree", treePrefabAngry, typeof(GameObject), false);
        treePrefabAngrySmall = (GameObject)EditorGUILayout.ObjectField("Angry Small Tree", treePrefabAngrySmall, typeof(GameObject), false);

        GUILayout.Space(10);

        treePositionsParent = (GameObject)EditorGUILayout.ObjectField("Positions Parent", treePositionsParent, typeof(GameObject), true);

        GUILayout.Space(20);

        if (GUILayout.Button("Spawn Trees"))
        {
            if (treePositionsParent != null)
            {
                SpawnAllTrees();
            }
            else
            {
                Debug.LogWarning("Please assign a parent object containing tree positions!");
            }
        }
    }

    private void SpawnAllTrees()
    {
        Transform parentTransform = treePositionsParent.transform;

        foreach (Transform child in parentTransform)
        {
            if (child.name.Contains("Vegetation"))
            {
                parentTransform = child;
			}
		}

		// Collect all child positions
		List<GameObject> treePositions = new List<GameObject>();
        foreach (Transform child in parentTransform)
            treePositions.Add(child.gameObject);

        SpawnTrees(treePrefabHappy, treePrefabHappySmall, "Happy", treePositions, parentTransform);
        SpawnTrees(treePrefabSad, treePrefabSadSmall, "Sad", treePositions, parentTransform);
        SpawnTrees(treePrefabAngry, treePrefabAngrySmall, "Angry", treePositions, parentTransform);

        Debug.Log("Trees spawned as prefab instances!");
    }

    private void SpawnTrees(GameObject bigPrefab, GameObject smallPrefab, string parentName, List<GameObject> positions, Transform parentTransform)
    {
        GameObject parentObj = new GameObject(parentName);
        parentObj.transform.parent = parentTransform;
        parentObj.transform.position = parentTransform.position;

        foreach (GameObject pos in positions)
        {
            GameObject prefabToUse = null;
            if (pos.name.Contains("01")) prefabToUse = bigPrefab;
            else if (pos.name.Contains("02")) prefabToUse = smallPrefab;
            else continue;

            if (prefabToUse == null) continue;

            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToUse, parentObj.transform);
            obj.transform.position = pos.transform.position;
            obj.transform.rotation = pos.transform.rotation;
        }
    }
}
