using UnityEditor;
using UnityEngine;
using static FileLogger;

[CustomEditor(typeof(FileLogger))]
public class FileLoggerEditor : Editor
{
    private string _testMessage = "Test log message";
    private FileLogger.LogCategory _category = FileLogger.LogCategory.Info;
    private string _customLogFolder = "";

    private void OnEnable()
    {
        if (target is FileLogger logger)
        {
            _customLogFolder = logger.LogDirectory;
        }
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(10);

        GUILayout.Label("File Logger Debug Tool", EditorStyles.boldLabel);

        // Test message
        _testMessage = EditorGUILayout.TextField("Message", _testMessage);
        _category = (LogCategory)EditorGUILayout.EnumPopup("Category", _category);

        FileLogger logger = (FileLogger)target;
        bool canRun = logger != null && Application.isPlaying;

        EditorGUI.BeginDisabledGroup(!canRun);
        if (GUILayout.Button("Write Test Log", GUILayout.Height(30)))
        {
            logger?.LogCustom(_testMessage, _category);
            Debug.Log($"Logged '{_testMessage}' as a {_category} via FileLogger");
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(5);
        _customLogFolder = EditorGUILayout.TextField("Folder Path", _customLogFolder);

        EditorGUI.BeginDisabledGroup(!canRun);
        if (GUILayout.Button("Apply Log Folder"))
        {
            if (!string.IsNullOrWhiteSpace(_customLogFolder))
            {
                logger.SetLogDirectory(_customLogFolder);
                Debug.Log($"FileLogger log folder set to: {_customLogFolder}");
            }
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(5);
        EditorGUI.BeginDisabledGroup(!canRun);
        if (GUILayout.Button("Open Log Folder"))
        {
            string path = logger != null ? logger.LogDirectory : Application.persistentDataPath;
            Application.OpenURL(path);
        }
        EditorGUI.EndDisabledGroup();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("FileLogger test only works in Play mode.", MessageType.Info);
        }
    }
}