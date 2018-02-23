//*********************************************************
//
//    Copyright (c) Microsoft. All rights reserved.
//    This code is licensed under the Microsoft Public License.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MixerInteractivePlugin
{
    static MixerInteractivePlugin()
    {
    }

    [MenuItem("Mixer/Interactive Studio")]
    private static void InteractiveStudio()
    {
        Application.OpenURL("https://mixer.com/lab/interactive");
    }

    [MenuItem("Mixer/Documentation")]
    private static void ShowDocs()
    {
        Application.OpenURL("https://github.com/mixer/interactive-unity-plugin/wiki");
    }

    [MenuItem("Mixer/Open Mixer Editor")]
    private static void ShowMixerSettings()
    {
        InteractiveSettingsWindow window = EditorWindow.GetWindow<InteractiveSettingsWindow>();
        window.ShowTab();
    }

    [MenuItem("Mixer/File a bug")]
    private static void FileIssues()
    {
        Application.OpenURL("https://github.com/WatchBeam/interactive-unity-plugin/issues/new");
    }

    [MenuItem("Mixer/Feedback")]
    private static void Feedback()
    {
        Application.OpenURL("https://feedback.mixer.com");
    }
}