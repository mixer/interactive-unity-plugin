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
using Microsoft.Mixer;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using UnityEngine;

public class InteractiveSettingsWindow : EditorWindow
{
    private static string appID;
    private static string projectVersionID;
    private static string shareCode;
    private static int selectedControlIndex;
    private static int loggingLevelSelectIndex;
    private const string CONFIG_FILE_NAME = "interactiveconfig.json";
    private bool shouldSwitchToRunInBackground = false;
    LoggingLevel currentLogLevel;
    private Vector2 scrollPos;
    private static bool showApiExplorer;
    private static bool wasPaused;
    private static bool initialized;

    private static string[] logLevelOptions;
    private bool existingProjectInformation;

    // Use this for initialization
    void Start()
    {
    }

    void HandlePlayModeStateChanged(PlayModeStateChange args)
    {
        // The following is equivalent to the case of exiting play mode.
        if (!EditorApplication.isPaused &&
            EditorApplication.isPlaying &&
            !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            InteractivityManager.SingletonInstance.Dispose();
        }
    }

    void Initialize()
    {
        // See if we have existing config data
        existingProjectInformation = TryReadConfigFile();
        if (!existingProjectInformation)
        {
            appID = string.Empty;
            projectVersionID = string.Empty;
            shareCode = string.Empty;
        }

        titleContent = new GUIContent("Interactive Editor");
        EditorStyles.textArea.wordWrap = true;

        logLevelOptions = new string[]
           {
                "none", "minimal", "verbose"
           };

        string loggingLevel = EditorPrefs.GetString("MixerInteractive_LoggingLevel");
        loggingLevelSelectIndex = 0;
        switch (loggingLevel)
        {
            case "none":
                loggingLevelSelectIndex = 0;
                InteractivityManager.SingletonInstance.LoggingLevel = LoggingLevel.None;
                break;
            case "minimal":
                loggingLevelSelectIndex = 1;
                InteractivityManager.SingletonInstance.LoggingLevel = LoggingLevel.Minimal;
                break;
            case "verbose":
                loggingLevelSelectIndex = 2;
                InteractivityManager.SingletonInstance.LoggingLevel = LoggingLevel.Verbose;
                break;
            default:
                loggingLevelSelectIndex = 1;
                InteractivityManager.SingletonInstance.LoggingLevel = LoggingLevel.Minimal;
                break;
        };
        scrollPos = new Vector2();
        EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;

        initialized = true;
    }

    private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
    {
        throw new System.NotImplementedException();
    }

    void OnGUI()
    {
        if (!initialized)
        {
            Initialize();
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginVertical();

        // This section is used for notifications
        if (Application.isPlaying &&
           (InteractivityManager.SingletonInstance.InteractivityState == InteractivityState.InteractivityEnabled) &&
           !Application.runInBackground)
        {
            EditorGUILayout.LabelField("Warning: You may not recieve input from Mixer.", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This game is currently set to pause when Unity loses focus. If you are using the Mixer website, Unity will be paused. You can fix this by pressing the button below.", MessageType.Info);
            if (GUILayout.Button("Run in background"))
            {
                shouldSwitchToRunInBackground = true;
            }
        }

        // Project information
        EditorGUILayout.LabelField("Project information", EditorStyles.boldLabel);
        appID = EditorGUILayout.TextField("OAuth Client ID", appID);
        projectVersionID = EditorGUILayout.TextField("Version ID", projectVersionID);
        if (appID == string.Empty ||
            projectVersionID == string.Empty)
        {
            EditorGUILayout.HelpBox("Please fill in the project information above. You can find the values for OAuth Client ID and project Version ID by launching the interactive studio.", MessageType.Info);
            if (GUILayout.Button("Get IDs from Interactive Studio"))
            {
                Application.OpenURL("https://mixer.com/i/studio");
            }
            SectionSeperator();
        }
        if (GUILayout.Button("Save project information"))
        {
            if (!string.IsNullOrEmpty(appID) &&
                !string.IsNullOrEmpty(projectVersionID))
            {
                WriteConfigFile();
                EditorUtility.DisplayDialog("Project information saved successfully", "This Unity game is now associated with your interactive project.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error: Could not save project information", "The OAuth Client ID and Project Version ID cannot be empty.", "OK");
            }
        }

        SectionSeperator();
        
        // Share code
        EditorGUILayout.LabelField("Share Code", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Share Codes allow anyone running this game to access your interactive project. It is ideal for large game studios when you don't want to manually give each person access. You can get a short code from Interactive Studio by clicking the Manage Share Settings icon.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.BeginHorizontal();
        shareCode = EditorGUILayout.TextField("Share Code", shareCode);
        if (GUILayout.Button("Save"))
        {
            if (!string.IsNullOrEmpty(appID) &&
                !string.IsNullOrEmpty(projectVersionID) &&
                shareCode != null) // We allow an empty Share Code, because that allows the developer to clear the Share Code.
            {
                WriteConfigFile();
                EditorUtility.DisplayDialog("Share Code saved successfully", "Anyone running this game will now have access to the interactive project.", "Close");
            }
            else
            {
                EditorUtility.DisplayDialog("Error: Could not save project information", "The OAuth Client ID, Project Version ID and Share Code cannot be empty.", "Close");
            }
        }
        EditorGUILayout.EndHorizontal();

        SectionSeperator();

        // Clear project information
        EditorGUILayout.LabelField("Clear saved login information", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Clearing authentication tokens will delete any cached tokens. This is useful if you want to do testing with a new Mixer account.", EditorStyles.wordWrappedLabel);
        if (GUILayout.Button("Clear saved login information"))
        {
            shareCode = string.Empty;
            RemoveSavedLoginInformation();
        }

        SectionSeperator();

        EditorGUILayout.LabelField("Log level", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Change the amount of informational logging from the Mixer SDK. The output will appear in the Unity Console.", EditorStyles.wordWrappedLabel);
        int oldLogLevelIndex = loggingLevelSelectIndex;
        loggingLevelSelectIndex = EditorGUILayout.Popup("", loggingLevelSelectIndex, logLevelOptions, GUILayout.Width(96));
        if (oldLogLevelIndex != loggingLevelSelectIndex)
        {
            string newLoggingLevel = string.Empty;
            switch (loggingLevelSelectIndex)
            {
                case 0:
                    newLoggingLevel = "none";
                    InteractivityManager.SingletonInstance.LoggingLevel = LoggingLevel.None;
                    break;
                case 1:
                    newLoggingLevel = "minimal";
                    InteractivityManager.SingletonInstance.LoggingLevel = LoggingLevel.Minimal;
                    break;
                case 2:
                    newLoggingLevel = "verbose";
                    InteractivityManager.SingletonInstance.LoggingLevel = LoggingLevel.Verbose;
                    break;
                default:
                    break;
            }
            EditorPrefs.SetString("MixerInteractive_LoggingLevel", newLoggingLevel);
        }

        SectionSeperator();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    void Update()
    {
        if (shouldSwitchToRunInBackground)
        {
            Application.runInBackground = true;
        }
    }

    private void SectionSeperator()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
    }

    private bool TryReadConfigFile()
    {
        var readSucceeded = false;
        string fullPathToConfigFile = Application.streamingAssetsPath + "/" + CONFIG_FILE_NAME;
        if (File.Exists(fullPathToConfigFile))
        {
            string configText = File.ReadAllText(fullPathToConfigFile);
            try
            {
                using (StringReader stringReader = new StringReader(configText))
                using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
                {
                    while (jsonReader.Read())
                    {
                        if (jsonReader.Value != null)
                        {
                            string key = jsonReader.Value.ToString();
                            string lowercaseKey = key.ToLowerInvariant();
                            switch (lowercaseKey)
                            {
                                case "appid":
                                    jsonReader.Read();
                                    if (jsonReader.Value != null)
                                    {
                                        appID = jsonReader.Value.ToString();
                                    }
                                    break;
                                case "projectversionid":
                                    jsonReader.Read();
                                    if (jsonReader.Value != null)
                                    {
                                        projectVersionID = jsonReader.Value.ToString();
                                    }
                                    break;
                                case "sharecode":
                                    jsonReader.Read();
                                    if (jsonReader.Value != null)
                                    {
                                        shareCode = jsonReader.Value.ToString();
                                    }
                                    break;
                                default:
                                    // No-op. We don't throw an error because the SDK only implements a
                                    // subset of the total possible server messages so we expect to see
                                    // method messages that we don't know how to handle.
                                    break;
                            }
                        }
                    }
                }
                if (appID != string.Empty &&
                    projectVersionID != string.Empty)
                {
                    readSucceeded = true;
                }
            }
            catch
            {
                Debug.Log("Error: mixerconfig.json file could not be read. Make sure it is valid JSON and has the correct format.");
            }
        }
        return readSucceeded;
    }

    private void WriteConfigFile()
    {
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }
        string fullPathToConfigFile = Application.streamingAssetsPath + "/" + CONFIG_FILE_NAME;
        File.WriteAllText(fullPathToConfigFile,
            "{ \"AppID\": \"" + appID + "\", \"ProjectVersionID\": \"" + projectVersionID + "\", \"ShareCode\": \"" + shareCode + "\"}");
    }

    private void RemoveSavedLoginInformation()
    {
        MixerInteractive.ClearSavedLoginInformation();
    }
}