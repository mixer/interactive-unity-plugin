/*
 * Mixer Unity SDK
 *
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
 * to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
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