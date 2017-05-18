using Microsoft;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Beam))]
[CanEditMultipleObjects]
public class BeamManagerEditor : Editor
{
    private SerializedProperty runInBackground;
    private SerializedProperty defaultSceneID;

    // Custom Unity Inspectors are not great at displaying complex objects
    // so we'll store these as seperate variables instead of a List.
    public SerializedProperty group1ID;
    public SerializedProperty group2ID;
    public SerializedProperty group3ID;
    public SerializedProperty group4ID;
    public SerializedProperty group5ID;
    public SerializedProperty group6ID;
    public SerializedProperty group7ID;
    public SerializedProperty group8ID;
    public SerializedProperty group9ID;
    public SerializedProperty group10ID;
    public SerializedProperty group1SceneID;
    public SerializedProperty group2SceneID;
    public SerializedProperty group3SceneID;
    public SerializedProperty group4SceneID;
    public SerializedProperty group5SceneID;
    public SerializedProperty group6SceneID;
    public SerializedProperty group7SceneID;
    public SerializedProperty group8SceneID;
    public SerializedProperty group9SceneID;
    public SerializedProperty group10SceneID;

    private int numberOfGroups;

    void OnEnable()
    {
        runInBackground = serializedObject.FindProperty("runInBackground");
        defaultSceneID = serializedObject.FindProperty("defaultSceneID");

        group1ID = serializedObject.FindProperty("group1ID");
        group2ID = serializedObject.FindProperty("group2ID");
        group3ID = serializedObject.FindProperty("group3ID");
        group4ID = serializedObject.FindProperty("group4ID");
        group5ID = serializedObject.FindProperty("group5ID");
        group6ID = serializedObject.FindProperty("group6ID");
        group7ID = serializedObject.FindProperty("group7ID");
        group8ID = serializedObject.FindProperty("group8ID");
        group9ID = serializedObject.FindProperty("group9ID");
        group10ID = serializedObject.FindProperty("group10ID");
        group1SceneID = serializedObject.FindProperty("group1SceneID");
        group2SceneID = serializedObject.FindProperty("group2SceneID");
        group3SceneID = serializedObject.FindProperty("group3SceneID");
        group4SceneID = serializedObject.FindProperty("group4SceneID");
        group5SceneID = serializedObject.FindProperty("group5SceneID");
        group6SceneID = serializedObject.FindProperty("group6SceneID");
        group7SceneID = serializedObject.FindProperty("group7SceneID");
        group8SceneID = serializedObject.FindProperty("group8SceneID");
        group9SceneID = serializedObject.FindProperty("group9SceneID");
        group10SceneID = serializedObject.FindProperty("group10SceneID");

        numberOfGroups = 1;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SectionSeperator();

        EditorGUILayout.LabelField("Run in background", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Allow this game to run in the background even if the window does not have focus.");
        EditorGUILayout.PropertyField(runInBackground);
        EditorGUILayout.HelpBox("Unity will pause the game by default if the window does not have focus. This means if you are using the Beam website, Unity will be paused. To allow your game to run while you use the Beam website for testing, check the Run In Background checkbox.", MessageType.Info);

        SectionSeperator();

        EditorGUILayout.LabelField("Scenes", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Specify the scene you want your audience to see.");
        if (defaultSceneID.stringValue == string.Empty)
        {
            defaultSceneID.stringValue = DEFAULT_SCENE;
        }
        EditorGUILayout.PropertyField(defaultSceneID);
        EditorGUILayout.HelpBox("If you have more than one Beam scene, you can specify which scene the BeamManager will show. All projects have a default scene, called \"default\".", MessageType.Info);

        SectionSeperator();

        EditorGUILayout.LabelField("Groups", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Create new groups and specify which Beam scene to show the group. Groups allow you to show different controls to different segments of the audience.", EditorStyles.wordWrappedLabel);

        if (numberOfGroups > 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Group ID");
            EditorGUILayout.LabelField("Scene ID");
            EditorGUILayout.EndHorizontal();

            RenderGroupControl(group1ID, group1SceneID);
        }

        if (numberOfGroups > 1)
        {
            RenderGroupControl(group2ID, group2SceneID);
        }

        if (numberOfGroups > 2)
        {
            RenderGroupControl(group3ID, group3SceneID);
        }

        if (numberOfGroups > 3)
        {
            RenderGroupControl(group4ID, group4SceneID);
        }

        if (numberOfGroups > 4)
        {
            RenderGroupControl(group5ID, group5SceneID);
        }

        if (numberOfGroups > 5)
        {
            RenderGroupControl(group6ID, group6SceneID);
        }

        if (numberOfGroups > 6)
        {
            RenderGroupControl(group7ID, group7SceneID);
        }

        if (numberOfGroups > 7)
        {
            RenderGroupControl(group8ID, group8SceneID);
        }

        if (numberOfGroups > 8)
        {
            RenderGroupControl(group9ID, group9SceneID);
        }

        if (numberOfGroups > 9)
        {
            RenderGroupControl(group10ID, group10SceneID);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add", GUILayout.Width(64)))
        {
            numberOfGroups++;
            switch (numberOfGroups)
            {
                case 1:
                    group1ID.stringValue = string.Empty;
                    group1SceneID.stringValue = defaultSceneID.stringValue;
                    break;
                case 2:
                    group2ID.stringValue = string.Empty;
                    group2SceneID.stringValue = defaultSceneID.stringValue;
                    break;
                case 3:
                    group3ID.stringValue = string.Empty;
                    group3SceneID.stringValue = defaultSceneID.stringValue;
                    break;
                case 4:
                    group4ID.stringValue = string.Empty;
                    group4SceneID.stringValue = defaultSceneID.stringValue;
                    break;
                case 5:
                    group5ID.stringValue = string.Empty;
                    group5SceneID.stringValue = defaultSceneID.stringValue;
                    break;
                case 6:
                    group6ID.stringValue = string.Empty;
                    group6SceneID.stringValue = defaultSceneID.stringValue;
                    break;
                case 7:
                    group7ID.stringValue = string.Empty;
                    group7SceneID.stringValue = defaultSceneID.stringValue;
                    break;
                case 8:
                    group8ID.stringValue = string.Empty;
                    group8SceneID.stringValue = defaultSceneID.stringValue;
                    break;
                case 9:
                    group9ID.stringValue = string.Empty;
                    group9SceneID.stringValue = defaultSceneID.stringValue;
                    break;
                case 10:
                    group10ID.stringValue = string.Empty;
                    group10SceneID.stringValue = defaultSceneID.stringValue;
                    break;
                default:
                    // No-op
                    break;
            }
        }
        EditorGUILayout.EndHorizontal();

        SectionSeperator();

        serializedObject.ApplyModifiedProperties();
    }

    private void RenderGroupControl(SerializedProperty groupID, SerializedProperty sceneID)
    {
        EditorGUILayout.BeginHorizontal();
        groupID.stringValue = EditorGUILayout.TextField(groupID.stringValue);
        if (sceneID.stringValue == string.Empty ||
            sceneID.stringValue == DEFAULT_SCENE)
        {
            sceneID.stringValue = EditorGUILayout.TextField(defaultSceneID.stringValue);
        }
        else
        {
            sceneID.stringValue = EditorGUILayout.TextField(sceneID.stringValue);
        }
        if (GUILayout.Button("Remove", GUILayout.Width(64)))
        {
            groupID.stringValue = string.Empty;
            sceneID.stringValue = string.Empty;
            numberOfGroups--;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void SectionSeperator()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
    }

    private const string DEFAULT_SCENE = "default";
}
