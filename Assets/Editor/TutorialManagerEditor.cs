using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TutorialManager))]
public class TutorialManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(10);

        GUILayout.Label("Tutorial Manager Debug Tool", EditorStyles.boldLabel);

        // Test message
        TutorialManager manager = (TutorialManager)target;
        bool canRun = manager && Application.isPlaying && !manager.IsTutorialRunning;

        EditorGUI.BeginDisabledGroup(!canRun);
        if (GUILayout.Button("Start Next Tutorial", GUILayout.Height(30)))
            manager.StartNextTutorial();
        EditorGUI.EndDisabledGroup();
    }
}