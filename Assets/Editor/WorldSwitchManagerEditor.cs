using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldSwitchManager))]
public class WorldSwitchManagerEditor : Editor
{
    private Emotion _selectedWorld = Emotion.Sad;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("EDITOR");
        WorldSwitchManager manager = (WorldSwitchManager)target;
        _selectedWorld = (Emotion)EditorGUILayout.EnumPopup("World to switch to", _selectedWorld);

        if (GUILayout.Button("Switch World"))
        {
            manager.SwitchWorld(_selectedWorld);
        }
    }
}