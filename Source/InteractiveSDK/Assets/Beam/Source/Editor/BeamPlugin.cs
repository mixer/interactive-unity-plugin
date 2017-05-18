using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BeamPlugin
{
    static BeamPlugin()
    {
    }

    [MenuItem("Beam/Interactive Studio")]
    private static void InteractiveStudio()
    {
        Application.OpenURL("https://beam.pro/i/studio");
    }

    [MenuItem("Beam/Documentation")]
    private static void ShowDocs()
    {
        Application.OpenURL("https://dev.beam.pro/");
    }

    [MenuItem("Beam/Open Beam Editor")]
    private static void ShowBeamSettings()
    {
        BeamSettingsWindow window = EditorWindow.GetWindow<BeamSettingsWindow>();
        window.ShowTab();
    }

    [MenuItem("Beam/File a bug")]
    private static void FileIssues()
    {
        Application.OpenURL("https://github.com/WatchBeam/interactive-unity-plugin/issues/new");
    }

    [MenuItem("Beam/Feedback")]
    private static void Feedback()
    {
        Application.OpenURL("https://feedback.beam.pro");
    }
}