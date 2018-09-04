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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
#if UNITY_WSA && !UNITY_EDITOR
using Windows.System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using System.Net.Http.Headers;
using Windows.Data.Json;
#endif
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using WebSocketSharp;
using Microsoft.Win32;
using System.Collections.Specialized;
#endif
#if UNITY_XBOXONE && !UNITY_EDITOR
using System.Diagnostics;
using System.Runtime.InteropServices;
#endif

namespace Microsoft.Mixer
{
    /// <summary>
    /// Manager service class that handles connection with the Interactive
    /// service and your game.
    /// </summary>
    public partial class InteractivityManager : IDisposable
    {
        // Events
        public delegate void OnErrorEventHandler(object sender, InteractiveEventArgs e);
        public event OnErrorEventHandler OnError;

        public delegate void OnInteractivityStateChangedHandler(object sender, InteractivityStateChangedEventArgs e);
        public event OnInteractivityStateChangedHandler OnInteractivityStateChanged;

        public delegate void OnParticipantStateChangedHandler(object sender, InteractiveParticipantStateChangedEventArgs e);
        public event OnParticipantStateChangedHandler OnParticipantStateChanged;

        public delegate void OnInteractiveButtonEventHandler(object sender, InteractiveButtonEventArgs e);
        public event OnInteractiveButtonEventHandler OnInteractiveButtonEvent;

        public delegate void OnInteractiveJoystickControlEventHandler(object sender, InteractiveJoystickEventArgs e);
        public event OnInteractiveJoystickControlEventHandler OnInteractiveJoystickControlEvent;

        public delegate void OnInteractiveMouseButtonEventHandler(object sender, InteractiveMouseButtonEventArgs e);
        public event OnInteractiveMouseButtonEventHandler OnInteractiveMouseButtonEvent;

        public delegate void OnInteractiveCoordinatesChangedHandler(object sender, InteractiveCoordinatesChangedEventArgs e);
        public event OnInteractiveCoordinatesChangedHandler OnInteractiveCoordinatesChangedEvent;

        internal delegate void OnInteractiveTextControlEventHandler(object sender, InteractiveTextEventArgs e);
        internal event OnInteractiveTextControlEventHandler OnInteractiveTextControlEvent;

        public delegate void OnInteractiveMessageEventHandler(object sender, InteractiveMessageEventArgs e);
        public event OnInteractiveMessageEventHandler OnInteractiveMessageEvent;

        public delegate void OnInteractiveDoWorkEventHandler(object sender, InteractiveEventArgs e);
        public event OnInteractiveDoWorkEventHandler OnInteractiveDoWorkEvent;

        private static InteractivityManager _singletonInstance;

        /// <summary>
        /// Gets the singleton instance of InteractivityManager.
        /// </summary>
        public static InteractivityManager SingletonInstance
        {
            get
            {
                if (_singletonInstance == null)
                {
                    _singletonInstance = new InteractivityManager();
                    _singletonInstance.InitializeInternal();
                }
                return _singletonInstance;
            }
        }

        /// <summary>
        /// Controls the amount of diagnostic output written by the Interactive SDK.
        /// </summary>
        public LoggingLevel LoggingLevel
        {
            get;
            set;
        }

        private string ProjectVersionID
        {
            get;
            set;
        }

        private string AppID
        {
            get;
            set;
        }

        private string ShareCode
        {
            get;
            set;
        }

        /// <summary>
        /// Can query the state of the InteractivityManager.
        /// </summary>
        public InteractivityState InteractivityState
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all the groups associated with the current interactivity instance.
        /// Will be empty if initialization is not complete.
        /// </summary>
        public IList<InteractiveGroup> Groups
        {
            get
            {
                return new List<InteractiveGroup>(_groups);
            }
        }

        /// <summary>
        /// Gets all the scenes associated with the current interactivity instance.
        /// Will be empty if initialization is not complete.
        /// </summary>
        public IList<InteractiveScene> Scenes
        {
            get
            {
                return new List<InteractiveScene>(_scenes);
            }
        }

        /// <summary>
        /// Returns all the participants.
        /// </summary>
        public IList<InteractiveParticipant> Participants
        {
            get
            {
                return new List<InteractiveParticipant>(_participants);
            }
        }

        internal IList<InteractiveControl> _Controls
        {
            get
            {
                return new List<InteractiveControl>(_controls);
            }
        }

        /// <summary>
        /// Retrieve a list of all of the button controls.
        /// </summary>
        public IList<InteractiveButtonControl> Buttons
        {
            get
            {
                return new List<InteractiveButtonControl>(_buttons);
            }
        }

        /// <summary>
        /// Retrieve a list of all of the joystick controls.
        /// </summary>
        public IList<InteractiveJoystickControl> Joysticks
        {
            get
            {
                return new List<InteractiveJoystickControl>(_joysticks);
            }
        }

        /// <summary>
        /// The string the broadcaster needs to enter in the Mixer website to
        /// authorize the interactive session.
        /// </summary>
        public string ShortCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the specified group. Will return null if initialization
        /// is not yet complete or group does not exist.
        /// </summary>
        /// <param name="groupID">The ID of the group.</param>
        /// <returns></returns>
        public InteractiveGroup GetGroup(string groupID)
        {
            foreach (InteractiveGroup group in _groups)
            {
                if (group.GroupID == groupID)
                {
                    return group;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the specified scene. Will return nullptr if initialization
        /// is not yet complete or scene does not exist.
        /// </summary>
        public InteractiveScene GetScene(string sceneID)
        {
            var scenes = Scenes;
            foreach (InteractiveScene scene in scenes)
            {
                if (scene.SceneID == sceneID)
                {
                    return scene;
                }
            }
            return null;
        }

        /// <summary>
        /// Kicks off a background task to set up the connection to the interactivity service.
        /// </summary>
        /// <param name="goInteractive"> If true, initializes and enters interactivity. Defaults to true</param>
        /// <param name="authToken">
        /// A token to use for authentication. This is used for when a user is on a device
        /// that supports Xbox Live tokens.
        /// </param>
        /// <remarks></remarks>
        public void Initialize(
            bool goInteractive = true,
            string authToken = "")
        {
            if (InteractivityState != InteractivityState.NotInitialized)
            {
                return;
            }

            ResetInternalState();
            UpdateInteractivityState(InteractivityState.Initializing);

            if (goInteractive)
            {
                _shouldStartInteractive = true;
            }
            if (!string.IsNullOrEmpty(authToken))
            {
                _authToken = authToken;
            }
            else
            {
#if !UNITY_EDITOR && UNITY_XBOXONE
                if (string.IsNullOrEmpty(_authToken))
                {
                    // On Xbox Live platforms, we try to get an Xbox Live token
                    var tokenData = new string(' ', 10240);
                    GCHandle pinnedMemory = GCHandle.Alloc(tokenData, GCHandleType.Pinned);
                    System.IntPtr dataPointer = pinnedMemory.AddrOfPinnedObject();
                    bool getXTokenSucceeded = MixerEraNativePlugin_GetXToken(dataPointer);
                    if (!getXTokenSucceeded)
                    {
                        _LogError("Error: Could not get a Xbox Live token. Make sure you have a user signed in.");
                    }
                    _authToken = Marshal.PtrToStringAnsi(dataPointer);
                    pinnedMemory.Free();
                }
#endif
            }
            InitiateConnection();
        }

        private void CreateStorageDirectoryIfNotExists()
        {
#if UNITY_EDITOR
            if (!Directory.Exists(_streamingAssetsPath))
            {
                Directory.CreateDirectory(_streamingAssetsPath);
            }
#endif
        }

        private void getWebsocketHosts(string potentialWebsocketUrlsJson)
        {
            _websocketHosts.Clear();
            _activeWebsocketHostIndex = 0;
            string targetWebsocketUrl = string.Empty;
            using (StringReader stringReader = new StringReader(potentialWebsocketUrlsJson))
            using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.Value != null &&
                        jsonReader.Value.ToString() == WS_MESSAGE_KEY_WEBSOCKET_ADDRESS)
                    {
                        jsonReader.Read();
                        targetWebsocketUrl = jsonReader.Value.ToString();
                        _websocketHosts.Add(targetWebsocketUrl);
                    }
                }
            }
            _interactiveWebSocketUrl = _websocketHosts[_activeWebsocketHostIndex];
        }

        internal void SetWebsocketInstance(Websocket newWebsocket)
        {
#if UNITY_XBOXONE && !UNITY_EDITOR
            _websocket = newWebsocket;
#endif
        }

        private void InitiateConnection()
        {
            try
            {
                mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestWebSocketHostsCompleted;
                mixerInteractiveHelper.OnInternalWebRequestStateChanged += OnRequestWebSocketHostsCompleted;
                mixerInteractiveHelper._MakeWebRequest(
                    "OnRequestWebSocketHostsCompleted",
                    WEBSOCKET_DISCOVERY_URL);
            }
            catch (Exception ex)
            {
                _LogError("Error: Could not retrieve the URL for the websocket. Exception details: " + ex.Message);
            }
        }

        private void CompleteInitiateConnection(string websocketHostsResponseString)
        {
            getWebsocketHosts(websocketHostsResponseString);
            if (string.IsNullOrEmpty(ProjectVersionID) ||
                (string.IsNullOrEmpty(AppID) && string.IsNullOrEmpty(ShareCode)))
            {
                PopulateConfigData();
            }
#if UNITY_XBOXONE && !UNITY_EDITOR
            ConnectToWebsocket();
#else
            if (!string.IsNullOrEmpty(_authToken))
            {
                VerifyAuthToken();
            }
            else
            {
                mixerInteractiveHelper.OnTryGetAuthTokensFromCacheCallback -= OnTryGetAuthTokensFromCacheCallback;
                mixerInteractiveHelper.OnTryGetAuthTokensFromCacheCallback += OnTryGetAuthTokensFromCacheCallback;
                mixerInteractiveHelper.StartTryGetAuthTokensFromCache();
            }
#endif
        }

        private void OnTryGetAuthTokensFromCacheCallback(object sender, MixerInteractiveHelper.TryGetAuthTokensFromCacheEventArgs e)
        {
            mixerInteractiveHelper.OnTryGetAuthTokensFromCacheCallback -= OnTryGetAuthTokensFromCacheCallback;
            OnTryGetAuthTokensFromCacheCompleted(e);
        }

        private void OnTryGetAuthTokensFromCacheCompleted(MixerInteractiveHelper.TryGetAuthTokensFromCacheEventArgs e)
        {
            _authToken = e.AuthToken;
            _oauthRefreshToken = e.RefreshToken;
            // Try to see if we have a cached auth token
            if (!string.IsNullOrEmpty(_authToken))
            {
                VerifyAuthToken();
            }
            else
            {
                // Show a shortCode
                RefreshShortCode();
            }
        }

        private void OnRequestWebSocketHostsCompleted(object sender, MixerInteractiveHelper._InternalWebRequestStateChangedEventArgs e)
        {
            if (e.RequestID != "OnRequestWebSocketHostsCompleted")
            {
                return;
            }
            mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestWebSocketHostsCompleted;
            if (e.Succeeded)
            {
                CompleteInitiateConnection(e.ResponseText);
            }
            else
            {
                _LogError("Error: Could not retrieve the URL for the websocket. Exception details: " + e.ErrorMessage);
            }
        }

        private void PopulateConfigData()
        {
            string fullPathToConfigFile = string.Empty;
            fullPathToConfigFile = _streamingAssetsPath + "/" + INTERACTIVE_CONFIG_FILE_NAME;
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
                                    case WS_MESSAGE_KEY_APPID:
                                        jsonReader.Read();
                                        if (jsonReader.Value != null)
                                        {
                                            AppID = jsonReader.Value.ToString();
                                        }
                                        break;
                                    case WS_MESSAGE_KEY_PROJECT_VERSION_ID:
                                        jsonReader.Read();
                                        if (jsonReader.Value != null)
                                        {
                                            ProjectVersionID = jsonReader.Value.ToString();
                                        }
                                        break;
                                    case WS_MESSAGE_KEY_PROJECT_SHARE_CODE:
                                        jsonReader.Read();
                                        if (jsonReader.Value != null)
                                        {
                                            ShareCode = jsonReader.Value.ToString();
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
                }
                catch
                {
                    _LogError("Error: interactiveconfig.json file could not be read. Make sure it is valid JSON and has the correct format.");
                }
            }
            else
            {
                throw new Exception("Error: You need to specify an AppID and ProjectVersionID in the Interactive Editor. You can get to the Interactivity Editor from the Mixer menu (Mixer > Open editor).");
            }
        }

        private void OnInternalCheckAuthStatusTimerCallback(object sender, MixerInteractiveHelper.InternalTimerCallbackEventArgs e)
        {
            TryGetTokenAsync();
        }

        private void TryGetTokenAsync()
        {
            _Log("Trying to obtain a new OAuth token. This is an expected and repeated call.");

            mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestOAuthExchangeTokenCompleted;
            mixerInteractiveHelper.OnInternalWebRequestStateChanged += OnRequestOAuthExchangeTokenCompleted;
            mixerInteractiveHelper._MakeWebRequest(
                "OnRequestOAuthExchangeTokenCompleted",
                API_CHECK_SHORT_CODE_AUTH_STATUS_PATH + _authShortCodeRequestHandle,
                new Dictionary<string, string>()
                {
                    { "Content-Type", "application/json" }
                }
            );
        }

        private void OnRequestOAuthExchangeTokenCompleted(object sender, MixerInteractiveHelper._InternalWebRequestStateChangedEventArgs e)
        {
            if (e.RequestID != "OnRequestOAuthExchangeTokenCompleted")
            {
                return;
            }
            mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestOAuthExchangeTokenCompleted;
            if (e.Succeeded)
            {
                CompleteRequestOAuthExchangeToken(e.ResponseCode, e.ResponseText);
            }
            else
            {
                _LogError("Error: Failed to request an OAuth exchange token. Error message: " + e.ErrorMessage);
            }
        }

        private void CompleteRequestOAuthExchangeToken(long statusCode, string getShortCodeStatusServerResponse)
        {
            switch (statusCode)
            {
                case 200: // OK
                    string oauthExchangeCode = ParseOAuthExchangeCodeFromStringResponse(getShortCodeStatusServerResponse);
                    mixerInteractiveHelper.OnInternalCheckAuthStatusTimerCallback -= OnInternalCheckAuthStatusTimerCallback;
                    mixerInteractiveHelper.StopTimer(MixerInteractiveHelper.InteractiveTimerType.CheckAuthStatus);
                    GetOauthToken(oauthExchangeCode);
                    break;
                case 204: // NoContent 
                case 404: // NotFound
                    // No-op: still waiting for user input.
                    break;
                default:
                    // No-op
                    break;
            };
        }

        private string ParseOAuthExchangeCodeFromStringResponse(string responseText)
        {
            string oauthExchangeCode = string.Empty;
            using (StringReader stringReader = new StringReader(responseText))
            using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
            {
                while (jsonReader.Read() && oauthExchangeCode == string.Empty)
                {
                    if (jsonReader.Value != null &&
                        jsonReader.Value.ToString() == WS_MESSAGE_KEY_CODE)
                    {
                        jsonReader.Read();
                        oauthExchangeCode = jsonReader.Value.ToString();
                    }
                }
            }
            return oauthExchangeCode;
        }

        private void GetOauthToken(string exchangeCode)
        {
            _Log("Retrieved an OAuth exchange token. Exchange token: " + exchangeCode + " Using AppID: " + AppID + " with exchange code: " + exchangeCode);

            string postData = "{ \"client_id\": \"" + AppID + "\", \"code\": \"" + exchangeCode + "\", \"grant_type\": \"authorization_code\" }";
            mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestOAuthTokenCompleted;
            mixerInteractiveHelper.OnInternalWebRequestStateChanged += OnRequestOAuthTokenCompleted;
            mixerInteractiveHelper._MakeWebRequest(
                "OnRequestOAuthTokenCompleted",
                API_GET_OAUTH_TOKEN_PATH,
                new Dictionary<string, string>()
                {
                    { "Content-Type", "application/json" }
                },
                "POST",
                postData
            );
        }

        private void OnRequestOAuthTokenCompleted(object sender, MixerInteractiveHelper._InternalWebRequestStateChangedEventArgs e)
        {
            if (e.RequestID != "OnRequestOAuthTokenCompleted")
            {
                return;
            }
            mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestOAuthTokenCompleted;
            if (e.Succeeded)
            {
                CompleteGetOAuthToken(e.ResponseCode, e.ResponseText);
            }
            else
            {
                _LogError("Error: Failed to request an OAuth token. Error message: " + e.ErrorMessage);
            }
        }

        private void CompleteGetOAuthToken(long statusCode, string getCodeServerResponse)
        {
            if (statusCode == 400)
            {
                _LogError("Error: " + getCodeServerResponse + " while requesting an OAuth token.");
                return;
            }
            string refreshToken = string.Empty;
            string accessToken = string.Empty;
            using (StringReader stringReader = new StringReader(getCodeServerResponse))
            using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.Value != null)
                    {
                        if (jsonReader.Value.ToString() == WS_MESSAGE_KEY_WEBSOCKET_ACCESS_TOKEN)
                        {
                            jsonReader.Read();
                            accessToken = jsonReader.Value.ToString();
                        }
                        else if (jsonReader.Value.ToString() == WS_MESSAGE_KEY_REFRESH_TOKEN)
                        {
                            jsonReader.Read();
                            refreshToken = jsonReader.Value.ToString();
                        }
                    }
                }
            }
            _authToken = "Bearer " + accessToken;
            _oauthRefreshToken = refreshToken;

            mixerInteractiveHelper.WriteAuthTokensToCache(_authToken, _oauthRefreshToken);

            _Log("Retrieved a new OAuth token. Token: " + _authToken);

            mixerInteractiveHelper.StopTimer(MixerInteractiveHelper.InteractiveTimerType.RefreshShortCode);
            mixerInteractiveHelper.StopTimer(MixerInteractiveHelper.InteractiveTimerType.CheckAuthStatus);

            ConnectToWebsocket();
        }

        private void RefreshShortCode()
        {
            string postData = "{ \"client_id\": \"" + AppID + "\", \"scope\": \"interactive:robot:self\" }";
            mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestRefresheShortCodeCompleted;
            mixerInteractiveHelper.OnInternalWebRequestStateChanged += OnRequestRefresheShortCodeCompleted;
            mixerInteractiveHelper._MakeWebRequest(
                "OnRequestRefresheShortCodeCompleted",
                API_GET_SHORT_CODE_PATH,
                new Dictionary<string, string>()
                {
                    { "Content-Type", "application/json" }
                },
                "POST",
                postData
           );
        }

        private void OnRequestRefresheShortCodeCompleted(object sender, MixerInteractiveHelper._InternalWebRequestStateChangedEventArgs e)
        {
            if (e.RequestID != "OnRequestRefresheShortCodeCompleted")
            {
                return;
            }
            mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestRefresheShortCodeCompleted;
            if (e.Succeeded)
            {
                if (e.ResponseCode == 404)
                {
                    _LogError("Error: OAuth Client ID not found. Make sure the OAuth Client ID you specified in the Unity editor matches the one in Interactive Studio.");
                    return;
                }
                CompleteRefreshShortCode(e.ResponseText);
            }
            else
            {
                _LogError("Error: Failed to retrieve a short code for short code authentication. Error message: " + e.ErrorMessage);
            }
        }

        private void CompleteRefreshShortCode(string getShortCodeServerResponse)
        {
            int shortCodeExpirationTime = -1;
            using (StringReader stringReader = new StringReader(getShortCodeServerResponse))
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
                            case WS_MESSAGE_KEY_CODE:
                                jsonReader.Read();
                                if (jsonReader.Value != null)
                                {
                                    ShortCode = jsonReader.Value.ToString();
                                }
                                break;
                            case WS_MESSAGE_KEY_EXPIRATION:
                                jsonReader.Read();
                                if (jsonReader.Value != null)
                                {
                                    shortCodeExpirationTime = Convert.ToInt32(jsonReader.Value.ToString());
                                }
                                break;
                            case WS_MESSAGE_KEY_HANDLE:
                                jsonReader.Read();
                                if (jsonReader.Value != null)
                                {
                                    _authShortCodeRequestHandle = jsonReader.Value.ToString();
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
            mixerInteractiveHelper.OnInternalRefreshShortCodeTimerCallback -= OnInternalRefreshShortCodeTimerCallback;
            mixerInteractiveHelper.OnInternalRefreshShortCodeTimerCallback += OnInternalRefreshShortCodeTimerCallback;
            mixerInteractiveHelper.StartTimer(
                MixerInteractiveHelper.InteractiveTimerType.RefreshShortCode,
                shortCodeExpirationTime
                );

            mixerInteractiveHelper.OnInternalCheckAuthStatusTimerCallback -= OnInternalCheckAuthStatusTimerCallback;
            mixerInteractiveHelper.OnInternalCheckAuthStatusTimerCallback += OnInternalCheckAuthStatusTimerCallback;
            mixerInteractiveHelper.StartTimer(
                MixerInteractiveHelper.InteractiveTimerType.CheckAuthStatus,
                POLL_FOR_SHORT_CODE_AUTH_INTERVAL
            );

            UpdateInteractivityState(InteractivityState.ShortCodeRequired);
        }

        private void VerifyAuthToken()
        {
            mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnVerifyAuthTokenRequestCompleted;
            mixerInteractiveHelper.OnInternalWebRequestStateChanged += OnVerifyAuthTokenRequestCompleted;
            mixerInteractiveHelper._MakeWebRequest(
                "OnVerifyAuthTokenRequestCompleted",
                _interactiveWebSocketUrl.Replace("wss", "https"),
                new Dictionary<string, string>()
                {
                    { "Authorization", _authToken },
                    { "X-Interactive-Version", ProjectVersionID },
                    { "X-Protocol-Version", PROTOCOL_VERSION }
                }
            );
            return;
        }

        private void OnVerifyAuthTokenRequestCompleted(object sender, MixerInteractiveHelper._InternalWebRequestStateChangedEventArgs e)
        {
            if (e.RequestID != "OnVerifyAuthTokenRequestCompleted")
            {
                return;
            }
            mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnVerifyAuthTokenRequestCompleted;
            if (e.Succeeded)
            {
                bool isTokenValid = false;
                if (e.ResponseCode == 401) /* Unauthorized */
                {
                    isTokenValid = false;
                }
                else if (e.ResponseCode == 200 || /* Ok */
                    e.ResponseCode == 400) /* BadRequest */
                {
                    // 400 - Bad request will happen when upgrading the web socket an
                    // means the request succeeded.
                    isTokenValid = true;
                }
                else
                {
                    _LogError("Error: Failed to while trying to validate a cached auth token. Error code: " + e.ResponseCode);
                }
                CompleteVerifyAuthTokenRequestStart(isTokenValid);
            }
            else
            {
                _LogError("Error: Failed to verify the auth token. Error message: " + e.ErrorMessage);
            }
        }

        private void CompleteVerifyAuthTokenRequestStart(bool isTokenValid)
        {
            if (!isTokenValid)
            {
                RefreshAuthToken();
            }
            else
            {
                ConnectToWebsocket();
            }
        }

#if UNITY_WSA && !UNITY_EDITOR
        private async void ConnectToWebsocket()
#else
        private void ConnectToWebsocket()
#endif
        {
            if (_pendingConnectToWebSocket ||
                _websocketConnected)
            {
                return;
            }
            _pendingConnectToWebSocket = false;
            _websocketConnected = true;

            string shareCodeDebugLogString = string.Empty;
            if (ShareCode != string.Empty)
            {
                shareCodeDebugLogString = ", Share Code: " + ShareCode;
            }
            _Log("Connecting to websocket with Project Version ID: " + ProjectVersionID +
                shareCodeDebugLogString +
                ", OAuth Client ID: " + AppID +
                " and Auth Token: " + _authToken + ".");

#if UNITY_WSA && !UNITY_EDITOR
            try
            {
                _websocket.SetRequestHeader("Authorization", _authToken);
                _websocket.SetRequestHeader("X-Interactive-Version", ProjectVersionID);
                _websocket.SetRequestHeader("X-Protocol-Version", PROTOCOL_VERSION);
                if (!string.IsNullOrEmpty(ShareCode))
                {
                    _websocket.SetRequestHeader("X-Interactive-Sharecode", ShareCode);
                }

                _websocket.MessageReceived += OnWebSocketMessage;
                _websocket.Closed += OnWebSocketClose;
                await _websocket.ConnectAsync(new Uri(_interactiveWebSocketUrl));
                mixerInteractiveHelper.StopTimer(MixerInteractiveHelper.InteractiveTimerType.Reconnect);
            }
            catch (Exception ex)
            {
                _LogError("Error: " + ex.Message);
            }
#elif UNITY_XBOXONE && !UNITY_EDITOR
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers["Authorization"] = _authToken;
            headers["X-Interactive-Version"] = ProjectVersionID;
            if (ShareCode != string.Empty)
            {
                headers["X-Interactive-Sharecode"] = ShareCode;
            }
            headers["X-Protocol-Version"] = PROTOCOL_VERSION;
            _websocket.OnOpen += OnWebsocketOpen;
            _websocket.OnMessage += OnWebSocketMessage;
            _websocket.OnError += OnWebSocketError;
            _websocket.OnClose += OnWebSocketClose;
            _websocket.Open(new Uri(_interactiveWebSocketUrl), headers);
#else
            _websocket = new WebSocket(_interactiveWebSocketUrl);

            NameValueCollection headerCollection = new NameValueCollection();
            headerCollection.Add("Authorization", _authToken);
            headerCollection.Add("X-Interactive-Version", ProjectVersionID);
            headerCollection.Add("X-Protocol-Version", PROTOCOL_VERSION);
            if (!string.IsNullOrEmpty(ShareCode))
            {
                headerCollection.Add("X-Interactive-Sharecode", ShareCode);
            }
            _websocket.SetHeaders(headerCollection);

            // Start a timer in case we never see the open event. WebSocketSharp
            // doesn't properly expose connection errors.

            _websocket.OnOpen += OnWebsocketOpen;
            _websocket.OnMessage += OnWebSocketMessage;
            _websocket.OnError += OnWebSocketError;
            _websocket.OnClose += OnWebSocketClose;
            _websocket.Connect();
#endif
        }

        private void OnWebsocketOpen(object sender, EventArgs args)
        {
            mixerInteractiveHelper.StopTimer(MixerInteractiveHelper.InteractiveTimerType.Reconnect);
        }

#if UNITY_WSA && !UNITY_EDITOR
        private void OnWebSocketMessage(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
#elif UNITY_XBOXONE && !UNITY_EDITOR
        private void OnWebSocketMessage(object sender, Microsoft.MessageEventArgs args)
#else
        private void OnWebSocketMessage(object sender, WebSocketSharp.MessageEventArgs args)
#endif
        {
            string messageText = string.Empty;
#if UNITY_WSA && !UNITY_EDITOR
            if (args.MessageType == SocketMessageType.Utf8)
            {
                DataReader dataReader = args.GetDataReader();
                string dataAsString = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                ProcessWebSocketMessage(dataAsString);
            }
#elif UNITY_XBOXONE && !UNITY_EDITOR
            messageText = args.Message;
#else
            if (!args.IsText)
            {
                return;
            }
            messageText = args.Data;
#endif
            ProcessWebSocketMessage(messageText);
        }

#if UNITY_WSA && !UNITY_EDITOR
        private void OnWebSocketError(object sender, Microsoft.ErrorEventArgs args)
#elif UNITY_XBOXONE && !UNITY_EDITOR
        private void OnWebSocketError(object sender, Microsoft.ErrorEventArgs args)
#else
        private void OnWebSocketError(object sender, WebSocketSharp.ErrorEventArgs args)
#endif
        {
            UpdateInteractivityState(InteractivityState.InteractivityDisabled);
            _LogError("Error: Websocket OnError: " + args.Message);
        }

#if UNITY_WSA && !UNITY_EDITOR
        private void OnWebSocketClose(IWebSocket sender, WebSocketClosedEventArgs args)
#elif UNITY_XBOXONE && !UNITY_EDITOR
        private void OnWebSocketClose(object sender, Microsoft.CloseEventArgs args)
#else
        private void OnWebSocketClose(object sender, WebSocketSharp.CloseEventArgs args)
#endif
        {
            UpdateInteractivityState(InteractivityState.InteractivityDisabled);
            if (args.Code == 4019)
            {
                _LogError("Connection failed (error code 4019): You don't have access to this project. Make sure that the account you are signed in with has " +
                    "access to this Version ID. If you are using a share code, make sure that the share code value matches the one in Interactive Studio for this project.");
            }
            else if (args.Code == 4020)
            {
                _LogError("Connection failed (error code 4020): The interactive version was not found or you do not have access to it. Make sure that the account you are signed in with has " +
                    "access to this Version ID. If you are using a share code, make sure that the share code value matches the one in Interactive Studio for this project.");
            }
            else if (args.Code == 4021)
            {
                _LogError("Connection failed (error code 4021): You are connected to this session somewhere else. Please disconnect from that session and try again.");
            }
            else if (args.Code == 4025)
            {
                _LogError("Connection failed (error code 4025): The participant can no longer access the session. This may be because they were banned.");
            }
            else if (args.Code == 4027)
            {
                _LogError("Connection failed (error code 4027): The game client was purposely terminated and should not try to reconnect.");
            }
            else
            {
                // Any other type of error means we didn't succeed in connecting. If that happens we need to try to reconnect.
                // We do a retry with a reduced interval.
                _pendingConnectToWebSocket = false;
                _websocketConnected = false;
                _activeWebsocketHostIndex++;
                _interactiveWebSocketUrl = _websocketHosts[_activeWebsocketHostIndex];
                mixerInteractiveHelper.OnInternalReconnectTimerCallback -= OnInternalReconnectTimerCallback;
                mixerInteractiveHelper.OnInternalReconnectTimerCallback += OnInternalReconnectTimerCallback;
                mixerInteractiveHelper.StartTimer(
                    MixerInteractiveHelper.InteractiveTimerType.Reconnect,
                    WEBSOCKET_RECONNECT_INTERVAL
                );
            }
        }

        private void RefreshAuthToken()
        {
            string postData = "{ \"client_id\": \"" + AppID + "\", \"refresh_token\": \"" + _oauthRefreshToken + "\", \"grant_type\": \"refresh_token\" }";
            mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestRefreshedAuthTokenCompleted;
            mixerInteractiveHelper.OnInternalWebRequestStateChanged += OnRequestRefreshedAuthTokenCompleted;
            mixerInteractiveHelper._MakeWebRequest(
                "OnRequestRefreshedAuthTokenCompleted",
                API_GET_OAUTH_TOKEN_PATH,
                new Dictionary<string, string>()
                {
                    { "Content-Type", "application/json" }
                },
                "POST",
                postData
            );
        }

        private void OnRequestRefreshedAuthTokenCompleted(object sender, MixerInteractiveHelper._InternalWebRequestStateChangedEventArgs e)
        {
            if (e.RequestID != "OnRequestRefreshedAuthTokenCompleted")
            {
                return;
            }
            mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestRefreshedAuthTokenCompleted;
            if (e.Succeeded)
            {
                CompleteRefreshAuthToken(e.ResponseCode, e.ResponseText);
            }
            else
            {
                _LogError("Error: Web request to refresh the Auth token failed. Error message: " + e.ErrorMessage);
            }
        }

        private void CompleteRefreshAuthToken(long statusCode, string getCodeServerResponse)
        {
            if (statusCode == 400)
            {
                _LogError("Error: " + getCodeServerResponse + " trying to refresh the auth token.");
            }
            string accessToken = string.Empty;
            string refreshToken = string.Empty;

            using (StringReader stringReader = new StringReader(getCodeServerResponse))
            using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.Value != null)
                    {
                        if (jsonReader.Value.ToString() == WS_MESSAGE_KEY_WEBSOCKET_ACCESS_TOKEN)
                        {
                            jsonReader.Read();
                            accessToken = jsonReader.Value.ToString();
                        }
                        else if (jsonReader.Value.ToString() == WS_MESSAGE_KEY_REFRESH_TOKEN)
                        {
                            jsonReader.Read();
                            refreshToken = jsonReader.Value.ToString();
                        }
                    }
                }
            }
            _authToken = "Bearer " + accessToken;
            _oauthRefreshToken = refreshToken;
            mixerInteractiveHelper.WriteAuthTokensToCache(_authToken, _oauthRefreshToken);

            VerifyAuthToken();
        }

        private void UpdateInteractivityState(InteractivityState state)
        {
            InteractivityState = state;
            InteractivityStateChangedEventArgs interactivityStateChangedArgs = new InteractivityStateChangedEventArgs(InteractiveEventType.InteractivityStateChanged, state);
            _queuedEvents.Add(interactivityStateChangedArgs);
        }

        private InteractiveControl ControlFromControlID(string controlID)
        {
            var controls = _Controls;
            foreach (InteractiveControl control in controls)
            {
                if (control.ControlID == controlID)
                {
                    return control;
                }
            }
            return null;
        }

        internal void CaptureTransaction(string transactionID)
        {
            // Return early if there isn't a valid Transaction ID. This may happen in the case of zero
            // spark buttons.
            if (string.IsNullOrEmpty(transactionID))
            {
                return;
            }
            _SendCaptureTransactionMessage(transactionID);
        }

        /// <summary>
        /// Trigger a cooldown, disabling the specified control for a period of time.
        /// </summary>
        /// <param name="controlID">String ID of the control to disable.</param>
        /// <param name="cooldown">Duration (in milliseconds) required between triggers.</param>
        public void TriggerCooldown(string controlID, int cooldown)
        {
            if (InteractivityState != InteractivityState.InteractivityEnabled)
            {
                throw new Exception("Error: The InteractivityManager's InteractivityState must be InteractivityEnabled before calling this method.");
            }

            if (cooldown < 1000)
            {
                _Log("Info: Did you mean to use a cooldown of " + (float)cooldown / 1000 + " seconds? Remember, cooldowns are in milliseconds.");
            }

            // Get the control from our data structure to find it's etag
            string controlEtag = string.Empty;
            string controlSceneID = string.Empty;
            InteractiveControl control = ControlFromControlID(controlID);
            if (control != null)
            {
                InteractiveButtonControl button = control as InteractiveButtonControl;
                if (button != null)
                {
                    controlSceneID = control._sceneID;
                }
                else
                {
                    _LogError("Error: The control is not a button. You can only trigger a cooldown on a button.");
                    return;
                }
            }

            Int64 computedCooldown = 0;
#if UNITY_XBOXONE && !UNITY_EDITOR
            // The DateTime class uses JIT so it does not work on Xbox One.
            var computedCooldownTicks = MixerEraNativePlugin_GetSystemTime();
            computedCooldown = (computedCooldownTicks / 10000) + cooldown; // 10000 - Ticks / millisecond
#else
            computedCooldown = (Int64)Math.Truncate(DateTime.UtcNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds + cooldown);
#endif

            var controlAsButton = control as InteractiveButtonControl;
            if (controlAsButton != null)
            {
                controlAsButton._cooldownExpirationTime = computedCooldown;
            }

            // Send an update control message
            var messageID = _currentmessageID++;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                jsonWriter.WriteValue(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                jsonWriter.WriteValue(messageID);
                jsonWriter.WritePropertyName(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WriteValue(WS_MESSAGE_METHOD_UPDATE_CONTROLS);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_SCENE_ID);
                jsonWriter.WriteValue(controlSceneID);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_CONTROLS);
                jsonWriter.WriteStartArray();
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_CONTROL_ID);
                jsonWriter.WriteValue(controlID);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ETAG);
                jsonWriter.WriteValue(controlEtag);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_COOLDOWN);
                jsonWriter.WriteValue(computedCooldown);
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();
                SendJsonString(stringWriter.ToString());
            }
            StoreIfExpectingReply(messageID, WS_MESSAGE_METHOD_UPDATE_CONTROLS);
        }

        /// <summary>
        /// Sends a custom message. The format must be JSON.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void SendMessage(string message)
        {
            SendJsonString(message);
        }

        /// <summary>
        /// Sends a custom message. The message will be formatted as JSON automatically.
        /// </summary>
        /// <param name="messageType">The name of this type of message.</param>
        /// <param name="parameters">A collection of name / value pairs.</param>
        public void SendMessage(string messageType, Dictionary<string, object> parameters)
        {
            // Send an update control message
            var messageID = _currentmessageID++;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                jsonWriter.WriteValue(messageType);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                jsonWriter.WriteValue(messageID);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                jsonWriter.WriteStartObject();
                var parameterKeys = parameters.Keys;
                foreach (var key in parameterKeys)
                {
                    jsonWriter.WritePropertyName(key);
                    jsonWriter.WriteValue(parameters[key].ToString());
                }
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();
                SendJsonString(stringWriter.ToString());
            }
            StoreIfExpectingReply(messageID, messageType);
        }

        /// <summary>
        /// Used by the title to inform the interactivity service that it is ready to recieve interactive input.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public void StartInteractive()
        {
            if (InteractivityState == InteractivityState.NotInitialized)
            {
                MixerInteractive.GoInteractive();
            }
            if (InteractivityState == InteractivityState.Initializing || 
                InteractivityState == InteractivityState.ShortCodeRequired || 
                InteractivityState == InteractivityState.InteractivityPending ||
                InteractivityState == InteractivityState.InteractivityEnabled)
            {
                // Don't throw, just return because we are already interactive
                // or about to go interactive.
                return;
            }
            // We send a ready message here, but wait for a response from the server before
            // setting the interactivity state to InteractivityEnabled.

            SendReady(true);
            _shouldStartInteractive = false;
            UpdateInteractivityState(InteractivityState.InteractivityPending);
        }

        /// <summary>
        /// Used by the title to inform the interactivity service that it is no longer receiving interactive input.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public void StopInteractive()
        {
            if (InteractivityState == InteractivityState.NotInitialized ||
                InteractivityState == InteractivityState.InteractivityDisabled)
            {
                return;
            }

            UpdateInteractivityState(InteractivityState.InteractivityDisabled);
            SendReady(false);
            InteractiveEventArgs stopInteractiveEvent = new InteractiveEventArgs(InteractiveEventType.InteractivityStateChanged);
            _queuedEvents.Add(stopInteractiveEvent);
        }

        /// <summary>
        /// Manages and maintains proper state updates between your game and the interactivity service.
        /// To ensure best performance, DoWork() must be called frequently, such as once per frame.
        /// Title needs to be thread safe when calling DoWork() since this is when states are changed.
        /// </summary>
        public void DoWork()
        {
            ClearPreviousControlState();
            RaiseQueuedInteractiveEvents();
            SendQueuedSetControlPropertyUpdates();
        }

        private void RaiseQueuedInteractiveEvents()
        {
            // Go through all list of queued events and fire events.
            foreach (InteractiveEventArgs interactiveEvent in _queuedEvents.ToArray())
            {
                switch (interactiveEvent.EventType)
                {
                    case InteractiveEventType.InteractivityStateChanged:
                        if (OnInteractivityStateChanged != null)
                        {
                            OnInteractivityStateChanged(this, interactiveEvent as InteractivityStateChangedEventArgs);
                        }
                        break;
                    case InteractiveEventType.ParticipantStateChanged:
                        if (OnParticipantStateChanged != null)
                        {
                            OnParticipantStateChanged(this, interactiveEvent as InteractiveParticipantStateChangedEventArgs);
                        }
                        break;
                    case InteractiveEventType.Button:
                        if (OnInteractiveButtonEvent != null)
                        {
                            OnInteractiveButtonEvent(this, interactiveEvent as InteractiveButtonEventArgs);
                        }
                        break;
                    case InteractiveEventType.Joystick:
                        if (OnInteractiveJoystickControlEvent != null)
                        {
                            OnInteractiveJoystickControlEvent(this, interactiveEvent as InteractiveJoystickEventArgs);
                        }
                        break;
                    case InteractiveEventType.MouseButton:
                        if (OnInteractiveMouseButtonEvent != null)
                        {
                            OnInteractiveMouseButtonEvent(this, interactiveEvent as InteractiveMouseButtonEventArgs);
                        }
                        break;
                    case InteractiveEventType.Coordinates:
                        if (OnInteractiveCoordinatesChangedEvent != null)
                        {
                            OnInteractiveCoordinatesChangedEvent(this, interactiveEvent as InteractiveCoordinatesChangedEventArgs);
                        }
                        break;
                    case InteractiveEventType.TextInput:
                        if (OnInteractiveTextControlEvent != null)
                        {
                            OnInteractiveTextControlEvent(this, interactiveEvent as InteractiveTextEventArgs);
                        }
                        break;
                    case InteractiveEventType.Error:
                        if (OnError != null)
                        {
                            OnError(this, interactiveEvent as InteractiveEventArgs);
                        }
                        break;
                    default:
                        if (OnInteractiveMessageEvent != null)
                        {
                            OnInteractiveMessageEvent(this, interactiveEvent as InteractiveMessageEventArgs);
                        }
                        break;
                }
            }
            _queuedEvents.Clear();

            // Raise an event for any other controls listening for DoWork.
            if (OnInteractiveDoWorkEvent != null)
            {
                OnInteractiveDoWorkEvent(this, new InteractiveEventArgs());
            }
        }

        private void SendQueuedSetControlPropertyUpdates()
        {
            var sceneKeys = _queuedControlPropertyUpdates.Keys;
            foreach (string sceneID in sceneKeys)
            {
                // Send an update control message
                var messageID = _currentmessageID++;
                StringBuilder stringBuilder = new StringBuilder();
                StringWriter stringWriter = new StringWriter(stringBuilder);
                using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
                {
                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                    jsonWriter.WriteValue(WS_MESSAGE_TYPE_METHOD);
                    jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                    jsonWriter.WriteValue(messageID);
                    jsonWriter.WritePropertyName(WS_MESSAGE_TYPE_METHOD);
                    jsonWriter.WriteValue(WS_MESSAGE_METHOD_UPDATE_CONTROLS);
                    jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName(WS_MESSAGE_KEY_SCENE_ID);
                    jsonWriter.WriteValue(sceneID);
                    jsonWriter.WritePropertyName(WS_MESSAGE_KEY_CONTROLS);
                    jsonWriter.WriteStartArray();

                    var controlIDs = _queuedControlPropertyUpdates[sceneID].Keys;
                    foreach (string controlID in controlIDs)
                    {
                        jsonWriter.WriteStartObject();
                        jsonWriter.WritePropertyName(WS_MESSAGE_KEY_CONTROL_ID);
                        jsonWriter.WriteValue(controlID);
                        Dictionary<string, _InternalControlPropertyMetaData> controlPropertyData = _queuedControlPropertyUpdates[sceneID][controlID].properties;
                        var controlPropertyDataKeys = controlPropertyData.Keys;
                        foreach (string controlPropertyDataKey in controlPropertyDataKeys)
                        {
                            jsonWriter.WritePropertyName(controlPropertyDataKey);
                            _InternalControlPropertyMetaData controlPropertyMetaData = controlPropertyData[controlPropertyDataKey];
                            if (controlPropertyMetaData.type == _KnownControlPropertyPrimitiveTypes.Boolean)
                            {
                                jsonWriter.WriteValue(controlPropertyMetaData.boolValue);
                            }
                            else if (controlPropertyMetaData.type == _KnownControlPropertyPrimitiveTypes.Number)
                            {
                                jsonWriter.WriteValue(controlPropertyMetaData.numberValue);
                            }
                            else
                            {
                                jsonWriter.WriteValue(controlPropertyMetaData.stringValue);
                            }
                        }
                        jsonWriter.WriteEndObject();
                    }
                    jsonWriter.WriteEndArray();
                    jsonWriter.WriteEndObject();
                    jsonWriter.WriteEnd();
                    SendJsonString(stringWriter.ToString());
                }
                StoreIfExpectingReply(messageID, WS_MESSAGE_METHOD_UPDATE_CONTROLS);
            }
            _queuedControlPropertyUpdates.Clear();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            ResetInternalState();
            mixerInteractiveHelper.Dispose();
            if (_websocket != null)
            {
#if UNITY_WSA && !UNITY_EDITOR
                _websocket.MessageReceived -= OnWebSocketMessage;
                _websocket.Closed -= OnWebSocketClose;
                _websocket.Close(0, "Dispose was called.");
#else
                _websocket.OnOpen -= OnWebsocketOpen;
                _websocket.OnMessage -= OnWebSocketMessage;
                _websocket.OnError -= OnWebSocketError;
                _websocket.OnClose -= OnWebSocketClose;
                _websocket.Close();
#endif
            }
            _disposed = true;
        }

        public void SendMockWebSocketMessage(string rawText)
        {
            ProcessWebSocketMessage(rawText);
        }

        private void ProcessWebSocketMessage(string messageText)
        {
            try
            {
                // Figure out the message type a different way
                using (StringReader stringReader = new StringReader(messageText))
                using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
                {
                    int messageID = -1;
                    string messageType = string.Empty;
                    while (jsonReader.Read())
                    {
                        if (jsonReader.Value != null)
                        {
                            if (jsonReader.Value.ToString() == WS_MESSAGE_KEY_ID)
                            {
                                jsonReader.ReadAsInt32();
                                messageID = Convert.ToInt32(jsonReader.Value);
                            }
                            if (jsonReader.Value.ToString() == WS_MESSAGE_KEY_TYPE)
                            {
                                jsonReader.Read();
                                if (jsonReader.Value != null)
                                {
                                    messageType = jsonReader.Value.ToString();
                                    if (messageType == WS_MESSAGE_TYPE_METHOD)
                                    {
                                        ProcessMethod(jsonReader);
                                    }
                                    else if (messageType == WS_MESSAGE_TYPE_REPLY)
                                    {
                                        ProcessReply(jsonReader, messageID);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                _LogError("Error: Failed to process message: " + messageText);
            }
            _Log(messageText);
            _queuedEvents.Add(new InteractiveMessageEventArgs(messageText));
        }

        private void ProcessMethod(JsonReader jsonReader)
        {
            try
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.Value != null)
                    {
                        string methodName = jsonReader.Value.ToString();
                        try
                        {
                            switch (methodName)
                            {
                                case WS_MESSAGE_METHOD_HELLO:
                                    HandleHelloMessage();
                                    break;
                                case WS_MESSAGE_METHOD_PARTICIPANT_JOIN:
                                    HandleParticipantJoin(jsonReader);
                                    break;
                                case WS_MESSAGE_METHOD_PARTICIPANT_LEAVE:
                                    HandleParticipantLeave(jsonReader);
                                    break;
                                case WS_MESSAGE_METHOD_PARTICIPANT_UPDATE:
                                    HandleParticipantUpdate(jsonReader);
                                    break;
                                case WS_MESSAGE_METHOD_GIVE_INPUT:
                                    HandleGiveInput(jsonReader);
                                    break;
                                case WS_MESSAGE_METHOD_ON_READY:
                                    HandleInteractivityStarted(jsonReader);
                                    break;
                                case WS_MESSAGE_METHOD_ON_CONTROL_UPDATE:
                                case WS_MESSAGE_METHOD_ON_CONTROL_CREATE:
                                    HandleControlUpdate(jsonReader);
                                    break;
                                case WS_MESSAGE_METHOD_ON_GROUP_CREATE:
                                    HandleGroupCreate(jsonReader);
                                    break;
                                case WS_MESSAGE_METHOD_ON_GROUP_UPDATE:
                                    HandleGroupUpdate(jsonReader);
                                    break;
                                case WS_MESSAGE_METHOD_ON_SCENE_CREATE:
                                    HandleSceneCreate(jsonReader);
                                    break;
                                default:
                                    // No-op. We don't throw an error because the SDK only implements a
                                    // subset of the total possible server messages so we expect to see
                                    // method messages that we don't know how to handle.
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _LogError("Error: Error while processing method: " + methodName + ". Error message: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _LogError("Error: Error processing websocket message. Error message: " + ex.Message);
            }
        }

        private void ProcessReply(JsonReader jsonReader, int messageIDAsInt)
        {
            uint messageID = 0;
            if (messageIDAsInt != -1)
            {
                messageID = Convert.ToUInt32(messageIDAsInt);
            }
            else
            {
                try
                {
                    while (jsonReader.Read())
                    {
                        if (jsonReader.Value != null &&
                            jsonReader.Value.ToString() == WS_MESSAGE_KEY_ID)
                        {
                            messageID = (uint)jsonReader.ReadAsInt32();
                        }
                    }
                }
                catch
                {
                    _LogError("Error: Failed to get the message ID from the reply message.");
                }
            }
            string replyMessgeMethod = string.Empty;
            _outstandingMessages.TryGetValue(messageID, out replyMessgeMethod);
            try
            {
                switch (replyMessgeMethod)
                {
                    case WS_MESSAGE_METHOD_GET_ALL_PARTICIPANTS:
                        HandleGetAllParticipants(jsonReader);
                        break;
                    case WS_MESSAGE_METHOD_GET_GROUPS:
                        HandleGetGroups(jsonReader);
                        break;
                    case WS_MESSAGE_METHOD_GET_SCENES:
                        HandleGetScenes(jsonReader);
                        break;
                    case WS_MESSAGE_METHOD_SET_CURRENT_SCENE:
                        HandlePossibleError(jsonReader);
                        break;
                    default:
                        // No-op
                        break;
                }
            }
            catch
            {
                _LogError("Error: An error occured while processing the reply: " + replyMessgeMethod);
            }
        }

        private void HandlePossibleError(JsonReader jsonReader)
        {
            int errorCode = 0;
            string errorMessage = string.Empty;
            while (jsonReader.Read())
            {
                if (jsonReader.Value != null)
                {
                    string keyValue = jsonReader.Value.ToString();
                    switch (keyValue)
                    {
                        case WS_MESSAGE_KEY_ERROR_CODE:
                            jsonReader.ReadAsInt32();
                            errorCode = Convert.ToInt32(jsonReader.Value);
                            break;
                        case WS_MESSAGE_KEY_ERROR_MESSAGE:
                            jsonReader.Read();
                            if (jsonReader.Value != null)
                            {
                                errorMessage += " Message: " + jsonReader.Value.ToString();
                            }
                            break;
                        case WS_MESSAGE_KEY_ERROR_PATH:
                            jsonReader.Read();
                            if (jsonReader.Value != null)
                            {
                                errorMessage += " Path: " + jsonReader.Value.ToString();
                            }
                            break;
                        default:
                            // No-op
                            break;
                    }
                }
            }
            if (errorCode != 0 &&
                errorMessage != string.Empty)
            {
                _LogError(errorMessage, errorCode);
            }
        }

        private void ResetInternalState()
        {
            _disposed = false;
            _initializedGroups = false;
            _initializedScenes = false;
            _shouldStartInteractive = false;
            _pendingConnectToWebSocket = false;
            _websocketConnected = false;
            UpdateInteractivityState(InteractivityState.NotInitialized);
        }

        private void HandleHelloMessage()
        {
            SendGetAllGroupsMessage();
            SendGetAllScenesMessage();
        }

        private void HandleInteractivityStarted(JsonReader jsonReader)
        {
            bool startInteractive = false;
            while (jsonReader.Read())
            {
                if (jsonReader.Value != null)
                {
                    string keyValue = jsonReader.Value.ToString();
                    if (keyValue == WS_MESSAGE_KEY_ISREADY)
                    {
                        jsonReader.ReadAsBoolean();
                        if (jsonReader.Value != null)
                        {
                            startInteractive = (bool)jsonReader.Value;
                            break;
                        }
                    }
                }
            }
            if (startInteractive)
            {
                UpdateInteractivityState(InteractivityState.InteractivityEnabled);
            }
        }

        private void HandleControlUpdate(JsonReader jsonReader)
        {
            string sceneID = string.Empty;
            while (jsonReader.Read())
            {
                if (jsonReader.Value != null)
                {
                    string keyValue = jsonReader.Value.ToString();
                    if (keyValue == WS_MESSAGE_KEY_SCENE_ID)
                    {
                        jsonReader.Read();
                        sceneID = jsonReader.Value.ToString();
                    }
                    else if (keyValue == WS_MESSAGE_KEY_CONTROLS)
                    {
                        UpdateControls(jsonReader, sceneID);
                    }
                }
            }
        }

        private void UpdateControls(JsonReader jsonReader, string sceneID)
        {
            try
            {
                while (jsonReader.Read() && jsonReader.TokenType != JsonToken.EndArray)
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        var updatedControl = ReadControl(jsonReader, sceneID);
                        InteractiveControl oldControl = null;
                        var controls = _Controls;
                        foreach (InteractiveControl control in controls)
                        {
                            if (control.ControlID == updatedControl.ControlID)
                            {
                                oldControl = control;
                                break;
                            }
                        }
                        var controlAsButton = updatedControl as InteractiveButtonControl;
                        if (controlAsButton != null)
                        {
                            var oldButtonControl = oldControl as InteractiveButtonControl;
                            if (oldButtonControl != null)
                            {
                                _buttons.Remove(oldButtonControl);
                            }
                            _buttons.Add(controlAsButton);
                        }
                        var controlAsJoystick = updatedControl as InteractiveJoystickControl;
                        if (controlAsJoystick != null)
                        {
                            var oldJoystickControl = oldControl as InteractiveJoystickControl;
                            if (oldJoystickControl != null)
                            {
                                _joysticks.Remove(oldJoystickControl);
                            }
                            _joysticks.Add(controlAsJoystick);
                        }
                        if (oldControl != null)
                        {
                            _controls.Remove(oldControl);
                        }
                        _controls.Add(updatedControl);
                    }
                }
            }
            catch
            {
                _LogError("Error: Failed reading controls for scene: " + sceneID + ".");
            }
        }

        private void HandleSceneCreate(JsonReader jsonReader)
        {
            while (jsonReader.Read())
            {
                if (jsonReader.Value != null)
                {
                    string keyValue = jsonReader.Value.ToString();
                    if (keyValue == WS_MESSAGE_KEY_SCENES)
                    {
                        _scenes.AddRange(ReadScenes(jsonReader));
                    }
                }
            }
        }

        private void HandleGroupCreate(JsonReader jsonReader)
        {
            ProcessGroups(jsonReader);
        }

        private void HandleGroupUpdate(JsonReader jsonReader)
        {
            ProcessGroups(jsonReader);
        }

        private void ProcessGroups(JsonReader jsonReader)
        {
            while (jsonReader.Read())
            {
                if (jsonReader.Value != null)
                {
                    string keyValue = jsonReader.Value.ToString();
                    if (keyValue == WS_MESSAGE_KEY_GROUPS)
                    {
                        ProcessGroupsImpl(jsonReader);
                    }
                }
            }
        }

        private void ProcessGroupsImpl(JsonReader jsonReader)
        {
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.EndArray)
                {
                    break;
                }
                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    var newGroup = ReadGroup(jsonReader);
                    var groups = Groups;
                    int existingGroupIndex = -1;
                    for (int i = 0; i < groups.Count; i++)
                    {
                        InteractiveGroup group = groups[i];
                        if (group.GroupID == newGroup.GroupID)
                        {
                            existingGroupIndex = i;
                            break;
                        }
                    }
                    if (existingGroupIndex != -1)
                    {
                        CloneGroupValues(newGroup, groups[existingGroupIndex]);
                    }
                    else
                    {
                        _groups.Add(newGroup);
                    }
                }
            }
        }

        private void CloneGroupValues(InteractiveGroup source, InteractiveGroup destination)
        {
            destination._etag = source._etag;
            destination.SceneID = source.SceneID;
            destination.GroupID = source.GroupID;
        }

        private void HandleGetAllParticipants(JsonReader jsonReader)
        {
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    _participants.Add(ReadParticipant(jsonReader));
                }
            }
        }

        private List<InteractiveParticipant> ReadParticipants(JsonReader jsonReader)
        {
            List<InteractiveParticipant> participants = new List<InteractiveParticipant>();
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    InteractiveParticipant newParticipant = ReadParticipant(jsonReader);
                    var existingParticipants = Participants;
                    int existingParticipantIndex = -1;
                    for (int i = 0; i < existingParticipants.Count; i++)
                    {
                        InteractiveParticipant participant = existingParticipants[i];
                        if (participant.SessionID == newParticipant.SessionID)
                        {
                            existingParticipantIndex = i;
                        }
                    }
                    if (existingParticipantIndex != -1)
                    {
                        CloneParticipantValues(newParticipant, existingParticipants[existingParticipantIndex]);
                    }
                    else
                    {
                        _participants.Add(newParticipant);
                    }
                    participants.Add(newParticipant);
                }
            }
            return participants;
        }

        private void CloneParticipantValues(InteractiveParticipant source, InteractiveParticipant destination)
        {
            destination.SessionID = source.SessionID;
            destination.UserID = source.UserID;
            destination.UserName = source.UserName;
            destination.Level = source.Level;
            destination.LastInputAt = source.LastInputAt;
            destination.ConnectedAt = source.ConnectedAt;
            destination.InputDisabled = source.InputDisabled;
            destination.State = source.State;
            destination._groupID = source._groupID;
            destination._etag = source._etag;
        }

        private InteractiveParticipant ReadParticipant(JsonReader jsonReader)
        {
            uint Id = 0;
            string sessionID = string.Empty;
            string etag = string.Empty;
            string interactiveUserName = string.Empty;
            string groupID = string.Empty;
            uint interactiveLevel = 0;
            bool inputDisabled = false;
            List<string> channelGroups = new List<string>();
            double connectedAtMillisecondsPastEpoch = 0;
            double lastInputAtMillisecondsPastEpoch = 0;
            DateTime lastInputAt = new DateTime();
            DateTime connectedAt = new DateTime();
            int startDepth = jsonReader.Depth;
            while (jsonReader.Read() && jsonReader.Depth > startDepth)
            {
                if (jsonReader.Value != null)
                {
                    if (jsonReader.Value != null)
                    {
                        string keyValue = jsonReader.Value.ToString();
                        switch (keyValue)
                        {
                            case WS_MESSAGE_KEY_SESSION_ID:
                                jsonReader.Read();
                                if (jsonReader.Value != null)
                                {
                                    sessionID = jsonReader.Value.ToString();
                                }
                                break;
                            case WS_MESSAGE_KEY_ETAG:
                                jsonReader.Read();
                                if (jsonReader.Value != null)
                                {
                                    etag = jsonReader.Value.ToString();
                                }
                                break;
                            case WS_MESSAGE_KEY_USER_ID:
                                jsonReader.ReadAsInt32();
                                Id = Convert.ToUInt32(jsonReader.Value);
                                break;
                            case WS_MESSAGE_KEY_USERNAME:
                                jsonReader.Read();
                                interactiveUserName = jsonReader.Value.ToString();
                                break;
                            case WS_MESSAGE_KEY_LEVEL:
                                jsonReader.Read();
                                interactiveLevel = Convert.ToUInt32(jsonReader.Value);
                                break;
                            case WS_MESSAGE_KEY_LAST_INPUT_AT:
                                jsonReader.Read();
                                lastInputAtMillisecondsPastEpoch = Convert.ToDouble(jsonReader.Value);
                                DateTime lastInputAtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                lastInputAt = lastInputAtDateTime.AddMilliseconds(lastInputAtMillisecondsPastEpoch).ToUniversalTime();
                                break;
                            case WS_MESSAGE_KEY_CONNECTED_AT:
                                jsonReader.Read();
                                connectedAtMillisecondsPastEpoch = Convert.ToDouble(jsonReader.Value);
                                DateTime connectedAtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                connectedAt = connectedAtDateTime.AddMilliseconds(connectedAtMillisecondsPastEpoch).ToUniversalTime();
                                break;
                            case WS_MESSAGE_KEY_GROUP_ID:
                                jsonReader.Read();
                                groupID = jsonReader.Value.ToString();
                                break;
                            case WS_MESSAGE_KEY_DISABLED:
                                jsonReader.ReadAsBoolean();
                                inputDisabled = (bool)jsonReader.Value;
                                break;
                            case WS_MESSAGE_KEY_CHANNEL_GROUPS:
                                // Read channel groups
                                while (jsonReader.Read())
                                {
                                    if (jsonReader.TokenType == JsonToken.EndArray)
                                    {
                                        break;
                                    }
                                    if (jsonReader.Value != null)
                                    {
                                        channelGroups.Add(jsonReader.Value.ToString());
                                    }
                                }
                                break;
                            default:
                                // No-op
                                break;
                        }
                    }
                }
            }
            InteractiveParticipantState participantState = inputDisabled ? InteractiveParticipantState.InputDisabled : InteractiveParticipantState.Joined;
            return new InteractiveParticipant(sessionID, etag, Id, groupID, interactiveUserName, channelGroups, interactiveLevel, lastInputAt, connectedAt, inputDisabled, participantState);
        }

        private void HandleGetGroups(JsonReader jsonReader)
        {
            while (jsonReader.Read())
            {
                if (jsonReader.Value != null &&
                    jsonReader.Value.ToString() == WS_MESSAGE_KEY_GROUPS)
                {
                        ProcessGroupsImpl(jsonReader);
                }
            }
            _initializedGroups = true;
            if (_initializedGroups &&
                _initializedScenes)
            {
                UpdateInteractivityState(InteractivityState.Initialized);
                if (_shouldStartInteractive)
                {
                    StartInteractive();
                }
            }
        }

        private InteractiveGroup ReadGroup(JsonReader jsonReader)
        {
            int startDepth = jsonReader.Depth;
            string etag = string.Empty;
            string sceneID = string.Empty;
            string groupID = string.Empty;
            while (jsonReader.Read() && jsonReader.Depth > startDepth)
            {
                if (jsonReader.Value != null)
                {
                    string keyValue = jsonReader.Value.ToString();
                    switch (keyValue)
                    {
                        case WS_MESSAGE_KEY_ETAG:
                            jsonReader.ReadAsString();
                            if (jsonReader.Value != null)
                            {
                                etag = jsonReader.Value.ToString();
                            }
                            break;
                        case WS_MESSAGE_KEY_SCENE_ID:
                            jsonReader.ReadAsString();
                            if (jsonReader.Value != null)
                            {
                                sceneID = jsonReader.Value.ToString();
                            }
                            break;
                        case WS_MESSAGE_KEY_GROUP_ID:
                            jsonReader.ReadAsString();
                            if (jsonReader.Value != null)
                            {
                                groupID = jsonReader.Value.ToString();
                            }
                            break;
                        default:
                            // No-op
                            break;
                    }
                }
            }
            return new InteractiveGroup(etag, sceneID, groupID);
        }

        private Dictionary<string, object> ReadMetaProperties(JsonReader jsonReader)
        {
            Dictionary<string, object> metaProperties = new Dictionary<string, object>();
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
                if (jsonReader.Value != null)
                {
                    string metaPropertyKey = jsonReader.Value.ToString();
                    ReadMetaProperty(jsonReader, metaPropertyKey, metaProperties);
                }
            }
            return metaProperties;
        }

        private void ReadMetaProperty(JsonReader jsonReader, string metaPropertyKey, Dictionary<string, object> metaProperties)
        {
            string metaPropertValue = string.Empty;
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.EndObject ||
                    metaPropertValue != string.Empty)
                {
                    break;
                }
                if (jsonReader.Value != null)
                {
                    string key = jsonReader.Value.ToString();
                    if (key == WS_MESSAGE_KEY_VALUE)
                    {
                        jsonReader.Read();
                        if (jsonReader.Value != null)
                        {
                            metaPropertValue = jsonReader.Value.ToString();
                        }
                    }
                }
            }
            metaProperties.Add(metaPropertyKey, metaPropertValue);
        }

        private void HandleGetScenes(JsonReader jsonReader)
        {
            while (jsonReader.Read())
            {
                if (jsonReader.Value != null)
                {
                    string keyValue = jsonReader.Value.ToString();
                    if (keyValue == WS_MESSAGE_KEY_SCENES)
                    {
                        _scenes = ReadScenes(jsonReader);
                    }
                }
            }
            _initializedScenes = true;
            if (_initializedGroups &&
                _initializedScenes)
            {
                UpdateInteractivityState(InteractivityState.Initialized);
                if (_shouldStartInteractive)
                {
                    StartInteractive();
                }
            }
        }

        private List<InteractiveScene> ReadScenes(JsonReader jsonReader)
        {
            List<InteractiveScene> scenes = new List<InteractiveScene>();
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    scenes.Add(ReadScene(jsonReader));
                }
            }
            return scenes;
        }

        private InteractiveScene ReadScene(JsonReader jsonReader)
        {
            InteractiveScene scene = new InteractiveScene();
            try
            {
                int startDepth = jsonReader.Depth;
                while (jsonReader.Read() && jsonReader.Depth > startDepth)
                {
                    if (jsonReader.Value != null)
                    {
                        string keyValue = jsonReader.Value.ToString();
                        switch (keyValue)
                        {
                            case WS_MESSAGE_KEY_SCENE_ID:
                                jsonReader.ReadAsString();
                                if (jsonReader.Value != null)
                                {
                                    scene.SceneID = jsonReader.Value.ToString();
                                }
                                break;
                            case WS_MESSAGE_KEY_ETAG:
                                jsonReader.ReadAsString();
                                if (jsonReader.Value != null)
                                {
                                    scene._etag = jsonReader.Value.ToString();
                                }
                                break;
                            case WS_MESSAGE_KEY_CONTROLS:
                                ReadControls(jsonReader, scene);
                                break;
                            default:
                                // No-op
                                break;
                        }
                    }
                }
            }
            catch
            {
                _LogError("Error: Error reading scene " + scene.SceneID + ".");
            }

            return scene;
        }

        private void ReadControls(JsonReader jsonReader, InteractiveScene scene)
        {
            try
            {
                while (jsonReader.Read() && jsonReader.TokenType != JsonToken.Null && jsonReader.TokenType != JsonToken.EndArray)
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        // Add the control to the scenes' controls & the global list of controls.
                        var control = ReadControl(jsonReader, scene.SceneID);
                        var controlAsButton = control as InteractiveButtonControl;
                        if (controlAsButton != null)
                        {
                            _buttons.Add(controlAsButton);
                        }
                        var controlAsJoystick = control as InteractiveJoystickControl;
                        if (controlAsJoystick != null)
                        {
                            _joysticks.Add(controlAsJoystick);
                        }
                        _controls.Add(control);
                    }
                }
            }
            catch
            {
                _LogError("Error: Failed reading controls for scene: " + scene.SceneID + ".");
            }
        }

        private InteractiveControl ReadControl(JsonReader jsonReader, string sceneID = "")
        {
            InteractiveControl newControl;
            int startDepth = jsonReader.Depth;
            string controlID = string.Empty;
            uint cost = 0;
            bool disabled = false;
            string text = string.Empty;
            string eTag = string.Empty;
            string kind = string.Empty;
            Dictionary<string, object> metaProperties = new Dictionary<string, object>();
            try
            {
                while (jsonReader.Read() && jsonReader.Depth > startDepth)
                {
                    if (jsonReader.Value != null)
                    {
                        string keyValue = jsonReader.Value.ToString();
                        switch (keyValue)
                        {
                            case WS_MESSAGE_KEY_CONTROL_ID:
                                jsonReader.ReadAsString();
                                controlID = jsonReader.Value.ToString();
                                break;
                            case WS_MESSAGE_KEY_DISABLED:
                                jsonReader.ReadAsBoolean();
                                disabled = (bool)jsonReader.Value;
                                break;
                            case _WS_MESSAGE_KEY_TEXT:
                                jsonReader.Read();
                                text = jsonReader.Value.ToString();
                                break;
                            case WS_MESSAGE_KEY_ETAG:
                                jsonReader.Read();
                                eTag = jsonReader.Value.ToString();
                                break;
                            case WS_MESSAGE_KEY_KIND:
                                jsonReader.Read();
                                kind = jsonReader.Value.ToString();
                                break;
                            case _WS_MESSAGE_KEY_COST:
                                jsonReader.ReadAsInt32();
                                cost = Convert.ToUInt32(jsonReader.Value);
                                break;
                            case WS_MESSAGE_KEY_META:
                                while (jsonReader.Read())
                                {
                                    if (jsonReader.TokenType == JsonToken.StartObject)
                                    {
                                        metaProperties = ReadMetaProperties(jsonReader);
                                        break;
                                    }
                                }
                                break;
                            default:
                                // No-op
                                break;
                        }
                    }
                }
            }
            catch
            {
                _LogError("Error: Error reading control " + controlID + ".");
            }
            if (kind == _WS_MESSAGE_VALUE_CONTROL_TYPE_BUTTON)
            {
                newControl = new InteractiveButtonControl(controlID, InteractiveEventType.Button, disabled, text, cost, eTag, sceneID, metaProperties);
            }
            else if (kind == _WS_MESSAGE_VALUE_CONTROL_TYPE_JOYSTICK)
            {
                newControl = new InteractiveJoystickControl(controlID, InteractiveEventType.Joystick, disabled, text, eTag, sceneID, metaProperties);
            }
            else if (kind == _WS_MESSAGE_VALUE_CONTROL_TYPE_TEXTBOX)
            {
                newControl = new InteractiveTextControl(controlID, InteractiveEventType.TextInput, disabled, text, eTag, sceneID, metaProperties);
            }
            else if (kind == _WS_MESSAGE_VALUE_CONTROL_TYPE_LABEL)
            {
                newControl = new InteractiveLabelControl(controlID, text, sceneID);
            }
            else
            {
                newControl = new InteractiveControl(controlID, kind, InteractiveEventType.Unknown, disabled, text, eTag, sceneID, metaProperties);
            }
            return newControl;
        }

        private _InputEvent ReadInputObject(JsonReader jsonReader)
        {
            _InputEvent inputEvent = new _InputEvent();
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    inputEvent = ReadInputInnerObject(jsonReader);
                }
            }
            return inputEvent;
        }

        private _InputEvent ReadInputInnerObject(JsonReader jsonReader)
        {
            int startDepth = jsonReader.Depth;
            string controlID = string.Empty;
            string eventName = string.Empty;
            object rawValue = null;
            bool isPressed = false;
            float x = 0;
            float y = 0;
            string textValue = string.Empty;
            try
            {
                while (jsonReader.Read() && jsonReader.Depth > startDepth)
                {
                    if (jsonReader.Value != null)
                    {
                        string keyValue = jsonReader.Value.ToString();
                        switch (keyValue)
                        {
                            case WS_MESSAGE_KEY_CONTROL_ID:
                                jsonReader.ReadAsString();
                                if (jsonReader.Value != null)
                                {
                                    controlID = jsonReader.Value.ToString();
                                }
                                break;
                            case WS_MESSAGE_KEY_EVENT:
                                eventName = jsonReader.ReadAsString();
                                if (eventName == EVENT_NAME_MOUSE_DOWN ||
                                    eventName == EVENT_NAME_MOUSE_UP ||
                                    eventName == EVENT_NAME_KEY_DOWN ||
                                    eventName == EVENT_NAME_KEY_UP)
                                {
                                    if (eventName == EVENT_NAME_MOUSE_DOWN ||
                                    eventName == EVENT_NAME_KEY_DOWN)
                                    {
                                        isPressed = true;
                                    }
                                    else if (eventName == EVENT_NAME_MOUSE_UP ||
                                    eventName == EVENT_NAME_KEY_UP)
                                    {
                                        isPressed = false;
                                    }
                                }
                                break;
                            case WS_MESSAGE_KEY_X:
                                x = (float)jsonReader.ReadAsDouble();
                                break;
                            case WS_MESSAGE_KEY_Y:
                                y = (float)jsonReader.ReadAsDouble();
                                break;
                            case WS_MESSAGE_KEY_VALUE:
                                jsonReader.Read();
                                rawValue = jsonReader.Value;
                                break;
                            default:
                                // No-op
                                break;
                        }
                        // Look for any other values the developer has asked us to track.
                        var keys = _giveInputKeyValues.Keys;
                        foreach (string key in keys)
                        {
                            if (key == keyValue)
                            {
                                _giveInputKeyValues[key] = rawValue;
                            }
                        }
                    }
                }
            }
            catch
            {
                _LogError("Error: Error reading input from control " + controlID + ".");
            }
            uint cost = 0;
            InteractiveControl control = ControlFromControlID(controlID);
            InteractiveButtonControl button = control as InteractiveButtonControl;
            if (button != null)
            {
                cost = button.Cost;
            }
            InteractiveEventType controlType = InteractiveEventTypeFromID(controlID);
            if (controlType == InteractiveEventType.TextInput)
            {
                textValue = rawValue.ToString();
            }
            return new _InputEvent(controlID, control._kind, eventName, controlType, isPressed, x, y, cost, string.Empty, textValue);
        }

        private void HandleParticipantJoin(JsonReader jsonReader)
        {
            int startDepth = jsonReader.Depth;
            while (jsonReader.Read())
            {
                if (jsonReader.Value != null)
                {
                    string keyValue = jsonReader.Value.ToString();
                    switch (keyValue)
                    {
                        case WS_MESSAGE_KEY_PARTICIPANTS:
                            List<InteractiveParticipant> participants = ReadParticipants(jsonReader);
                            for (int i = 0; i < participants.Count; i++)
                            {
                                InteractiveParticipant newParticipant = participants[i];
                                newParticipant.State = InteractiveParticipantState.Joined;
                                _queuedEvents.Add(new InteractiveParticipantStateChangedEventArgs(InteractiveEventType.ParticipantStateChanged, newParticipant, newParticipant.State));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void HandleParticipantLeave(JsonReader jsonReader)
        {
            try
            {
                int startDepth = jsonReader.Depth;
                while (jsonReader.Read())
                {
                    if (jsonReader.Value != null)
                    {
                        string keyValue = jsonReader.Value.ToString();
                        switch (keyValue)
                        {
                            case WS_MESSAGE_KEY_PARTICIPANTS:
                                List<InteractiveParticipant> participants = ReadParticipants(jsonReader);
                                for (int i = 0; i < participants.Count; i++)
                                {
                                    for (int j = _participants.Count - 1; j >= 0; j--)
                                    {
                                        if (_participants[j].SessionID == participants[i].SessionID)
                                        {
                                            InteractiveParticipant participant = _participants[j];
                                            participant.State = InteractiveParticipantState.Left;
                                            _queuedEvents.Add(new InteractiveParticipantStateChangedEventArgs(InteractiveEventType.ParticipantStateChanged, participant, participant.State));
                                        }
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch
            {
                _LogError("Error: Error while processing participant leave message.");
            }
        }

        private void HandleParticipantUpdate(JsonReader jsonReader)
        {
            int startDepth = jsonReader.Depth;
            while (jsonReader.Read())
            {
                if (jsonReader.Value != null)
                {
                    string keyValue = jsonReader.Value.ToString();
                    switch (keyValue)
                    {
                        case WS_MESSAGE_KEY_PARTICIPANTS:
                            ReadParticipants(jsonReader);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        internal struct _InputEvent
        {
            internal string ControlID;
            internal string Kind;
            internal string Event;
            // Type is a legacy field. For future controls,
            // use the kind property.
            internal InteractiveEventType Type;
            internal uint Cost;
            internal bool IsPressed;
            internal string TransactionID;
            internal float X;
            internal float Y;
            internal string TextValue;

            internal _InputEvent(
                string controlID,
                string kind,
                string eventName,
                InteractiveEventType type,
                bool isPressed,
                float x,
                float y,
                uint cost,
                string transactionID,
                string textValue
                )
            {
                ControlID = controlID;
                Kind = kind;
                Event = eventName;
                Type = type;
                Cost = cost;
                TransactionID = transactionID;
                IsPressed = isPressed;
                X = x;
                Y = y;
                TextValue = textValue;
            }
        };

        private void HandleGiveInput(JsonReader jsonReader)
        {
            string participantSessionID = string.Empty;
            string transactionID = string.Empty;
            _InputEvent inputEvent = new _InputEvent();

            while (jsonReader.Read())
            {
                if (jsonReader.Value != null)
                {
                    var value = jsonReader.Value.ToString();
                    switch (value)
                    {
                        case WS_MESSAGE_KEY_PARTICIPANT_ID:
                            jsonReader.Read();
                            participantSessionID = jsonReader.Value.ToString();
                            break;
                        case WS_MESSAGE_KEY_INPUT:
                            inputEvent = ReadInputObject(jsonReader);
                            break;
                        case WS_MESSAGE_KEY_TRANSACTION_ID:
                            jsonReader.Read();
                            transactionID = jsonReader.Value.ToString();
                            break;
                        default:
                            // No-op
                            break;
                    }
                }
            }

            inputEvent.TransactionID = transactionID;
            InternalTransactionIDState newTransactionIDState = new InternalTransactionIDState();
            if (_transactionIDsState.ContainsKey(inputEvent.ControlID))
            {
                newTransactionIDState = _transactionIDsState[inputEvent.ControlID];
            }
            newTransactionIDState.nextTransactionID = transactionID;
            _transactionIDsState[inputEvent.ControlID] = newTransactionIDState;

            InteractiveParticipant participant = _ParticipantBySessionID(participantSessionID);
            if (!_participantsWhoTriggeredGiveInput.ContainsKey(inputEvent.ControlID))
            {
                _participantsWhoTriggeredGiveInput.Add(inputEvent.ControlID, new _InternalParticipantTrackingState(participant));
            }
            participant.LastInputAt = DateTime.UtcNow;
            if (inputEvent.Type == InteractiveEventType.Button)
            {
                InteractiveButtonEventArgs eventArgs = new InteractiveButtonEventArgs(inputEvent.Type, inputEvent.ControlID, participant, inputEvent.IsPressed, inputEvent.Cost, inputEvent.TransactionID);
                _queuedEvents.Add(eventArgs);
                UpdateInternalButtonState(eventArgs);
            }
            else if (inputEvent.Type == InteractiveEventType.Joystick)
            {
                InteractiveJoystickEventArgs eventArgs = new InteractiveJoystickEventArgs(inputEvent.Type, inputEvent.ControlID, participant, inputEvent.X, inputEvent.Y);
                _queuedEvents.Add(eventArgs);
                UpdateInternalJoystickState(eventArgs);
            }
            else if (inputEvent.Type == InteractiveEventType.TextInput)
            {
                InteractiveTextEventArgs textEventArgs = new InteractiveTextEventArgs(inputEvent.Type, inputEvent.ControlID, participant, inputEvent.TextValue, inputEvent.TransactionID);
                _queuedEvents.Add(textEventArgs);
                UpdateInternalTextBoxState(textEventArgs);
            }

            string sessionID = participant.SessionID;

            // Handle screen input
            if (inputEvent.Kind == _CONTROL_KIND_SCREEN)
            {
                // Update x, y coordinates
                if (inputEvent.Event == EVENT_NAME_MOVE)
                {
                    Vector2 newMousePosition = new Vector2(inputEvent.X, inputEvent.Y);
                    // Translate the position to screen space.
                    newMousePosition.x = newMousePosition.x * Screen.width;
                    newMousePosition.y = newMousePosition.y * Screen.height;
                    if (_mousePositionsByParticipant.ContainsKey(sessionID))
                    {
                        _mousePositionsByParticipant[sessionID] = newMousePosition;
                    }
                    else
                    {
                        _mousePositionsByParticipant.Add(sessionID, newMousePosition);
                    }
                    InteractiveCoordinatesChangedEventArgs mouseMoveEventArgs = new InteractiveCoordinatesChangedEventArgs(
                       inputEvent.ControlID,
                       participant,
                       newMousePosition
                       );
                    _queuedEvents.Add(mouseMoveEventArgs);
                }
                else if (inputEvent.Event == EVENT_NAME_MOUSE_DOWN ||
                    inputEvent.Event == EVENT_NAME_MOUSE_UP)
                {
                    Vector2 newMousePosition = new Vector2(inputEvent.X, inputEvent.Y);
                    // Translate the position to screen space.
                    newMousePosition.x = newMousePosition.x * Screen.width;
                    newMousePosition.y = newMousePosition.y * Screen.height;
                    InteractiveMouseButtonEventArgs mouseButtonEventArgs = new InteractiveMouseButtonEventArgs(
                       inputEvent.ControlID,
                       participant,
                       inputEvent.IsPressed,
                       newMousePosition
                       );
                    _queuedEvents.Add(mouseButtonEventArgs);
                    UpdateInternalMouseButtonState(mouseButtonEventArgs);
                }
            }

            // Put the input key values in the control data structure.
            string controlID = inputEvent.ControlID;
            string controlType = inputEvent.Kind;
            Dictionary<string, object> controlData = new Dictionary<string, object>();
            if (_giveInputControlData.TryGetValue(controlID, out controlData))
            {
                var controlDataKeys = controlData.Keys;
                foreach (string key in controlDataKeys)
                {
                    object value = null;
                    if (_giveInputKeyValues.TryGetValue(key, out value))
                    {
                        controlData[key] = value;
                    }
                }
                _giveInputControlData[controlID] = controlData;
            }
            else
            {
                _giveInputControlData[controlID] = new Dictionary<string, object>();
            }

            // Update the by participant data structure.
            Dictionary<string, Dictionary<string, object>> controlDataByParticipant = new Dictionary<string, Dictionary<string, object>>();
            if (_giveInputControlDataByParticipant.TryGetValue(controlType, out controlDataByParticipant))
            {
                Dictionary<string, object> controlValues = new Dictionary<string, object>();
                if (controlDataByParticipant.TryGetValue(sessionID, out controlValues))
                {
                    var controlDataKeys = controlValues.Keys;
                    foreach (string key in controlDataKeys)
                    {
                        object value = null;
                        if (_giveInputKeyValues.TryGetValue(key, out value))
                        {
                            controlValues[key] = value;
                        }
                    }
                    controlDataByParticipant[sessionID] = controlValues;
                }
                else
                {
                    controlValues = new Dictionary<string, object>();
                }
                _giveInputControlDataByParticipant[controlType] = controlDataByParticipant;
            }
            else
            {
                _giveInputControlDataByParticipant[controlType] = new Dictionary<string, Dictionary<string, object>>();
            }
        }

        internal InteractiveParticipant _ParticipantBySessionID(string sessionID)
        {
            InteractiveParticipant target = null;
            var existingParticipants = Participants;
            foreach (InteractiveParticipant participant in existingParticipants)
            {
                if (participant.SessionID == sessionID)
                {
                    target = participant;
                    break;
                }
            }
            return target;
        }

        private InteractiveParticipant _ParticipantByUserID(uint userID)
        {
            InteractiveParticipant target = null;
            var existingParticipants = Participants;
            foreach (InteractiveParticipant participant in existingParticipants)
            {
                if (participant.UserID == userID)
                {
                    target = participant;
                    break;
                }
            }
            return target;
        }

        internal bool _GetButtonDownByUserID(string controlID, uint userID)
        {
            InteractiveParticipant participant = _ParticipantByUserID(userID);
            string sessionID = participant == null ? string.Empty : participant.SessionID;
            return _GetButtonDown(controlID, sessionID);
        }

        internal bool _GetButtonDown(string controlID, string sessionID)
        {
            bool getButtonDownResult = false;
            bool participantExists = false;
            Dictionary<string, _InternalButtonState> participantControls;
            if (!string.IsNullOrEmpty(sessionID))
            {
                participantExists = _buttonStatesByParticipant.TryGetValue(sessionID, out participantControls);
                if (participantExists)
                {
                    bool controlExists = false;
                    _InternalButtonState buttonState;
                    controlExists = participantControls.TryGetValue(controlID, out buttonState);
                    if (controlExists)
                    {
                        getButtonDownResult = buttonState.ButtonCountState.CountOfButtonDownEvents > 0;
                    }
                }
                else
                {
                    getButtonDownResult = false;
                }
            }
            return getButtonDownResult;
        }

        internal bool _GetButtonPressedByUserID(string controlID, uint userID)
        {
            InteractiveParticipant participant = _ParticipantByUserID(userID);
            string sessionID = participant == null ? string.Empty : participant.SessionID;
            return _GetButtonPressed(controlID, sessionID);
        }

        internal bool _GetButtonPressed(string controlID, string sessionID)
        {
            bool getButtonResult = false;
            bool participantExists = false;
            Dictionary<string, _InternalButtonState> participantControls;
            if (!string.IsNullOrEmpty(sessionID))
            {
                participantExists = _buttonStatesByParticipant.TryGetValue(sessionID, out participantControls);
                if (participantExists)
                {
                    bool controlExists = false;
                    _InternalButtonState buttonState;
                    controlExists = participantControls.TryGetValue(controlID, out buttonState);
                    if (controlExists)
                    {
                        getButtonResult = buttonState.ButtonCountState.CountOfButtonPressEvents > 0;
                    }
                }
                else
                {
                    getButtonResult = false;
                }
            }
            return getButtonResult;
        }

        internal bool _GetButtonUpByUserID(string controlID, uint userID)
        {
            InteractiveParticipant participant = _ParticipantByUserID(userID);
            string sessionID = participant == null ? string.Empty : participant.SessionID;
            return _GetButtonUp(controlID, sessionID);
        }

        internal bool _GetButtonUp(string controlID, string sessionID)
        {
            bool getButtonUpResult = false;
            bool participantExists = false;
            Dictionary<string, _InternalButtonState> participantControls;
            if (!string.IsNullOrEmpty(sessionID))
            {
                participantExists = _buttonStatesByParticipant.TryGetValue(sessionID, out participantControls);
                if (participantExists)
                {
                    bool controlExists = false;
                    _InternalButtonState buttonState;
                    controlExists = participantControls.TryGetValue(controlID, out buttonState);
                    if (controlExists)
                    {
                        getButtonUpResult = buttonState.ButtonCountState.CountOfButtonUpEvents > 0;
                    }
                }
                else
                {
                    getButtonUpResult = false;
                }
            }
            return getButtonUpResult;
        }

        internal uint _GetCountOfButtonDownsByUserID(string controlID, uint userID)
        {
            InteractiveParticipant participant = _ParticipantByUserID(userID);
            string sessionID = participant == null ? string.Empty : participant.SessionID;
            return _GetCountOfButtonDowns(controlID, sessionID);
        }

        internal uint _GetCountOfButtonDowns(string controlID, string sessionID)
        {
            uint countOfButtonDownEvents = 0;
            bool participantExists = false;
            Dictionary<string, _InternalButtonState> participantControls;
            if (!string.IsNullOrEmpty(sessionID))
            {
                participantExists = _buttonStatesByParticipant.TryGetValue(sessionID, out participantControls);
                if (participantExists)
                {
                    bool controlExists = false;
                    _InternalButtonState buttonState;
                    controlExists = participantControls.TryGetValue(controlID, out buttonState);
                    if (controlExists)
                    {
                        countOfButtonDownEvents = buttonState.ButtonCountState.CountOfButtonDownEvents;
                    }
                }
            }
            return countOfButtonDownEvents;
        }

        internal uint _GetCountOfButtonPressesByUserID(string controlID, uint userID)
        {
            InteractiveParticipant participant = _ParticipantByUserID(userID);
            string sessionID = participant == null ? string.Empty : participant.SessionID;
            return _GetCountOfButtonPresses(controlID, sessionID);
        }

        internal uint _GetCountOfButtonPresses(string controlID, string sessionID)
        {
            uint countOfButtonPressEvents = 0;
            bool participantExists = false;
            Dictionary<string, _InternalButtonState> participantControls;
            if (!string.IsNullOrEmpty(sessionID))
            {
                participantExists = _buttonStatesByParticipant.TryGetValue(sessionID, out participantControls);
                if (participantExists)
                {
                    bool controlExists = false;
                    _InternalButtonState buttonState;
                    controlExists = participantControls.TryGetValue(controlID, out buttonState);
                    if (controlExists)
                    {
                        countOfButtonPressEvents = buttonState.ButtonCountState.CountOfButtonPressEvents;
                    }
                }
            }
            return countOfButtonPressEvents;
        }

        internal uint _GetCountOfButtonUps(string controlID, uint userID)
        {
            InteractiveParticipant participant = _ParticipantByUserID(userID);
            string sessionID = participant == null ? string.Empty : participant.SessionID;
            return _GetCountOfButtonUps(controlID, sessionID);
        }

        internal uint _GetCountOfButtonUps(string controlID, string sessionID)
        {
            uint countOfButtonUpEvents = 0;
            _InternalButtonState buttonState;
            bool participantExists = false;
            Dictionary<string, _InternalButtonState> participantControls;
            if (!string.IsNullOrEmpty(sessionID))
            {
                participantExists = _buttonStatesByParticipant.TryGetValue(sessionID, out participantControls);
                if (participantExists)
                {
                    bool controlExists = false;
                    controlExists = participantControls.TryGetValue(controlID, out buttonState);
                    if (controlExists)
                    {
                        countOfButtonUpEvents = buttonState.ButtonCountState.CountOfButtonUpEvents;
                    }
                }
            }
            return countOfButtonUpEvents;
        }

        internal bool _TryGetButtonStateByParticipant(string sessionID, string controlID, out _InternalButtonState buttonState)
        {
            buttonState = new _InternalButtonState();
            bool buttonExists = false;
            bool participantExists = false;
            Dictionary<string, _InternalButtonState> participantControls;
            if (!string.IsNullOrEmpty(sessionID))
            {
                participantExists = _buttonStatesByParticipant.TryGetValue(sessionID, out participantControls);
                if (participantExists)
                {
                    bool controlExists = false;
                    controlExists = participantControls.TryGetValue(controlID, out buttonState);
                    if (controlExists)
                    {
                        buttonExists = true;
                    }
                }
            }
            return buttonExists;
        }

        internal InteractiveJoystickControl _GetJoystick(string controlID, uint userID)
        {
            InteractiveJoystickControl joystick = new InteractiveJoystickControl(controlID, InteractiveEventType.Joystick, true, string.Empty, string.Empty, string.Empty, new Dictionary<string, object>());
            var joysticks = Joysticks;
            foreach (InteractiveJoystickControl potential in joysticks)
            {
                if (potential.ControlID == controlID)
                {
                    joystick = potential;
                }
            }
            joystick._userID = userID;
            return joystick;
        }

        internal double _GetJoystickXByUserID(string controlID, uint userID)
        {
            double joystickX = 0;
            InteractiveParticipant participant = _ParticipantByUserID(userID);
            string sessionID = participant == null ? string.Empty : participant.SessionID;
            if (!string.IsNullOrEmpty(sessionID))
            {
                joystickX = _GetJoystickX(controlID, sessionID);
            }
            return joystickX;
        }

        internal double _GetJoystickX(string controlID, string sessionID)
        {
            double joystickX = 0;
            _InternalJoystickState joystickState;
            if (string.IsNullOrEmpty(sessionID) && 
                TryGetJoystickStateByParticipant(sessionID, controlID, out joystickState))
            {
                joystickX = joystickState.X;
            }
            return joystickX;
        }

        internal double _GetJoystickYByUserID(string controlID, uint userID)
        {
            InteractiveParticipant participant = _ParticipantByUserID(userID);
            string sessionID = participant == null ? string.Empty : participant.SessionID;
            return _GetJoystickY(controlID, sessionID);
        }

        internal double _GetJoystickY(string controlID, string sessionID)
        {
            double joystickY = 0;
            _InternalJoystickState joystickState;
            if (!string.IsNullOrEmpty(sessionID) &&
                TryGetJoystickStateByParticipant(sessionID, controlID, out joystickState))
            {
                joystickY = joystickState.Y;
            }
            return joystickY;
        }

        private bool TryGetJoystickStateByParticipant(string sessionID, string controlID, out _InternalJoystickState joystickState)
        {
            joystickState = new _InternalJoystickState();
            bool joystickExists = false;
            bool participantExists = false;
            Dictionary<string, _InternalJoystickState> participantControls;
            if (!string.IsNullOrEmpty(sessionID))
            {
                participantExists = _joystickStatesByParticipant.TryGetValue(sessionID, out participantControls);
                if (participantExists)
                {
                    bool controlExists = false;
                    controlExists = participantControls.TryGetValue(controlID, out joystickState);
                    if (controlExists)
                    {
                        joystickExists = true;
                    }
                }
            }
            return joystickExists;
        }

        internal _InternalMouseButtonState TryGetMouseButtonState(string sessionID)
        {
            _InternalMouseButtonState mouseButtonState = new _InternalMouseButtonState();
            _mouseButtonStateByParticipant.TryGetValue(sessionID, out mouseButtonState);
            return mouseButtonState;
        }

        internal string GetText(string controlID, string sessionID)
        {
            string text = string.Empty;
            Dictionary<string, string> participantTextBoxes;
            if (!string.IsNullOrEmpty(sessionID))
            {
                bool participantExists = _textboxValuesByParticipant.TryGetValue(sessionID, out participantTextBoxes);
                if (participantExists)
                {
                    participantTextBoxes.TryGetValue(controlID, out text);
                }
            }
            return text;
        }

        internal InteractiveControl _GetControl(string controlID)
        {
            InteractiveControl control = new InteractiveControl(controlID, "", InteractiveEventType.Unknown, true, "", "", "", new Dictionary<string, object>());
            var controls = _Controls;
            foreach (InteractiveControl currentControl in controls)
            {
                if (currentControl.ControlID == controlID)
                {
                    control = currentControl;
                    break;
                }
            }
            return control;
        }

        /// <summary>
        /// Gets a button control object by ID.
        /// </summary>
        /// <param name="controlID">The ID of the control.</param>
        /// <returns></returns>
        public InteractiveButtonControl GetButton(string controlID)
        {
            InteractiveButtonControl buttonControl = new InteractiveButtonControl(controlID, InteractiveEventType.Button, false, string.Empty, 0, string.Empty, string.Empty, new Dictionary<string, object>());
            var buttons = Buttons;
            foreach (InteractiveButtonControl currentButtonControl in buttons)
            {
                if (currentButtonControl.ControlID == controlID)
                {
                    buttonControl = currentButtonControl;
                    break;
                }
            }
            return buttonControl;
        }

        /// <summary>
        /// Gets a joystick control object by ID.
        /// </summary>
        /// <param name="controlID">The ID of the control.</param>
        /// <returns></returns>
        public InteractiveJoystickControl GetJoystick(string controlID)
        {
            InteractiveJoystickControl joystickControl = new InteractiveJoystickControl(controlID, InteractiveEventType.Joystick, true, "", "", "", new Dictionary<string, object>());
            var joysticks = Joysticks;
            foreach (InteractiveJoystickControl currentJoystick in joysticks)
            {
                if (currentJoystick.ControlID == controlID)
                {
                    joystickControl = currentJoystick;
                    break;
                }
            }
            return joystickControl;
        }

        /// <summary>
        /// Gets the current scene for the default group.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentScene()
        {
            InteractiveGroup group = GroupFromID(_WS_MESSAGE_VALUE_DEFAULT_GROUP_ID);
            return group.SceneID;
        }

        /// <summary>
        /// Sets the current scene for the default group.
        /// </summary>
        /// <param name="sceneID">The ID of the scene to change to.</param>
        public void SetCurrentScene(string sceneID)
        {
            InteractiveGroup defaultGroup = GroupFromID(_WS_MESSAGE_VALUE_DEFAULT_GROUP_ID);
            if (defaultGroup != null)
            {
                defaultGroup.SetScene(sceneID);
            }
        }

        internal IList<InteractiveTextResult> _GetText(string controlID)
        {
            List<InteractiveTextResult> interactiveTextResults = new List<InteractiveTextResult>();
            InteractivityManager interactivityManager = SingletonInstance;
            Dictionary<string, Dictionary<string, string>> textboxValuesByParticipant = _textboxValuesByParticipant;
            var participantSessionIDs = textboxValuesByParticipant.Keys;
            foreach (string participantSessionID in participantSessionIDs)
            {
                Dictionary<string, string> textboxValues = textboxValuesByParticipant[participantSessionID];
                string text = string.Empty;
                textboxValues.TryGetValue(controlID, out text);
                var newTextResult = new InteractiveTextResult();
                newTextResult.Participant = interactivityManager._ParticipantBySessionID(participantSessionID);
                newTextResult.Text = text;
                interactiveTextResults.Add(newTextResult);
            }
            return interactiveTextResults;
        }

        internal void _SetCurrentSceneInternal(InteractiveGroup group, string sceneID)
        {
            _SendSetUpdateGroupsMessage(group.GroupID, sceneID, group._etag);
        }

        private InteractiveGroup GroupFromID(string groupID)
        {
            InteractiveGroup target = new InteractiveGroup("", groupID, _WS_MESSAGE_VALUE_DEFAULT_GROUP_ID);
            var groups = Groups;
            foreach (InteractiveGroup group in groups)
            {
                if (group.GroupID == groupID)
                {
                    target = group;
                    break;
                }
            }
            return target;
        }

        private InteractiveScene SceneFromID(string sceneID)
        {
            InteractiveScene target = new InteractiveScene(sceneID);
            var scenes = Scenes;
            foreach (InteractiveScene scene in scenes)
            {
                if (scene.SceneID == sceneID)
                {
                    target = scene;
                    break;
                }
            }
            return target;
        }

        private InteractiveEventType InteractiveEventTypeFromID(string controlID)
        {
            InteractiveEventType type = InteractiveEventType.Unknown;
            foreach (var control in _controls)
            {
                if (controlID == control.ControlID)
                {
                    type = control._type;
                    break;
                }
            }
            return type;
        }

        // Private methods to send WebSocket messages
        private void SendReady(bool isReady)
        {
            uint messageID = _currentmessageID++;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                jsonWriter.WriteValue(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                jsonWriter.WriteValue(messageID);
                jsonWriter.WritePropertyName(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WriteValue(WS_MESSAGE_METHOD_READY);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(READY_PARAMETER_IS_READY);
                jsonWriter.WriteValue(isReady);
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();
                SendJsonString(stringWriter.ToString());
            }
            StoreIfExpectingReply(messageID, WS_MESSAGE_METHOD_READY);
        }

        internal void _SendCaptureTransactionMessage(string transactionID)
        {
            var messageID = _currentmessageID++;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                jsonWriter.WriteValue(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                jsonWriter.WriteValue(messageID);
                jsonWriter.WritePropertyName(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WriteValue(WS_MESSAGE_METHOD_SET_CAPTURE_TRANSACTION);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TRANSACTION_ID);
                jsonWriter.WriteValue(transactionID);
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();
                SendJsonString(stringWriter.ToString());
            }
            StoreIfExpectingReply(messageID, WS_MESSAGE_METHOD_SET_CAPTURE_TRANSACTION);
        }

        internal void _SendCreateGroupsMessage(string groupID, string sceneID)
        {
            var messageID = _currentmessageID++;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                jsonWriter.WriteValue(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                jsonWriter.WriteValue(messageID);
                jsonWriter.WritePropertyName(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WriteValue(WS_MESSAGE_METHOD_CREATE_GROUPS);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_GROUPS);
                jsonWriter.WriteStartArray();
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_GROUP_ID);
                jsonWriter.WriteValue(groupID);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_SCENE_ID);
                jsonWriter.WriteValue(sceneID);
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();
                SendJsonString(stringWriter.ToString());
            }
            StoreIfExpectingReply(messageID, WS_MESSAGE_METHOD_SET_CURRENT_SCENE);
        }

        internal void _SendSetUpdateGroupsMessage(string groupID, string sceneID, string groupEtag)
        {
            var messageID = _currentmessageID++;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                jsonWriter.WriteValue(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                jsonWriter.WriteValue(messageID);
                jsonWriter.WritePropertyName(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WriteValue(WS_MESSAGE_METHOD_UPDATE_GROUPS);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_GROUPS);
                jsonWriter.WriteStartArray();
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_GROUP_ID);
                jsonWriter.WriteValue(groupID);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_SCENE_ID);
                jsonWriter.WriteValue(sceneID);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ETAG);
                jsonWriter.WriteValue(groupEtag);
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();
                SendJsonString(stringWriter.ToString());
            }
            StoreIfExpectingReply(messageID, WS_MESSAGE_METHOD_SET_CURRENT_SCENE);
        }

        internal void _SendSetUpdateScenesMessage(InteractiveScene scene)
        {
            var messageID = _currentmessageID++;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                jsonWriter.WriteValue(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                jsonWriter.WriteValue(messageID);
                jsonWriter.WritePropertyName(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WriteValue(WS_MESSAGE_METHOD_UPDATE_SCENES);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_SCENES);
                jsonWriter.WriteStartArray();
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_SCENE_ID);
                jsonWriter.WriteValue(scene.SceneID);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ETAG);
                jsonWriter.WriteValue(scene._etag);
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();
                SendJsonString(stringWriter.ToString());
            }
            StoreIfExpectingReply(messageID, WS_MESSAGE_METHOD_SET_CURRENT_SCENE);
        }

        internal void _SendUpdateParticipantsMessage(InteractiveParticipant participant)
        {
            var messageID = _currentmessageID++;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                jsonWriter.WriteValue(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                jsonWriter.WriteValue(messageID);
                jsonWriter.WritePropertyName(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WriteValue(WS_MESSAGE_METHOD_UPDATE_PARTICIPANTS);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARTICIPANTS);
                jsonWriter.WriteStartArray();
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_SESSION_ID);
                jsonWriter.WriteValue(participant.SessionID);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ETAG);
                jsonWriter.WriteValue(participant._etag);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_GROUP_ID);
                jsonWriter.WriteValue(participant._groupID);
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();
                SendJsonString(stringWriter.ToString());
            }
            StoreIfExpectingReply(messageID, WS_MESSAGE_METHOD_UPDATE_PARTICIPANTS);
        }

        private void SendSetCompressionMessage()
        {
            var messageID = _currentmessageID++;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                jsonWriter.WriteValue(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                jsonWriter.WriteValue(messageID);
                jsonWriter.WritePropertyName(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WriteValue(WS_MESSAGE_METHOD_SET_COMPRESSION);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_SCHEME);
                jsonWriter.WriteStartArray();
                jsonWriter.WriteValue(COMPRESSION_TYPE_GZIP);
                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();
                SendJsonString(stringWriter.ToString());
            }
            StoreIfExpectingReply(messageID, WS_MESSAGE_METHOD_SET_COMPRESSION);
        }

        internal void _SendSetJoystickSetCoordinates(string controlID, double x, double y)
        {
            InteractiveControl control = ControlFromControlID(controlID);
            if (control == null)
            {
                return;
            }
            var messageID = _currentmessageID++;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                jsonWriter.WriteValue(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                jsonWriter.WriteValue(messageID);
                jsonWriter.WritePropertyName(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WriteValue(WS_MESSAGE_METHOD_UPDATE_CONTROLS);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_SCENE_ID);
                jsonWriter.WriteValue(control._sceneID);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_CONTROLS);
                jsonWriter.WriteStartArray();
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_CONTROL_ID);
                jsonWriter.WriteValue(controlID);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ETAG);
                jsonWriter.WriteValue(control._eTag);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_X);
                jsonWriter.WriteValue(x);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_Y);
                jsonWriter.WriteValue(y);
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();
                SendJsonString(stringWriter.ToString());
            }
            StoreIfExpectingReply(messageID, WS_MESSAGE_METHOD_SET_JOYSTICK_COORDINATES);
        }

        internal void _SendSetButtonControlProperties(
            string controlID,
            string propertyName,
            bool disabled,
            float progress,
            string text,
            uint cost
            )
        {
            InteractiveControl control = ControlFromControlID(controlID);
            if (control == null)
            {
                return;
            }
            var messageID = _currentmessageID++;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                jsonWriter.WriteValue(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                jsonWriter.WriteValue(messageID);
                jsonWriter.WritePropertyName(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WriteValue(WS_MESSAGE_METHOD_UPDATE_CONTROLS);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_SCENE_ID);
                jsonWriter.WriteValue(control._sceneID);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_CONTROLS);
                jsonWriter.WriteStartArray();
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_CONTROL_ID);
                jsonWriter.WriteValue(controlID);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ETAG);
                jsonWriter.WriteValue(control._eTag);
                if (propertyName == _WS_MESSAGE_VALUE_DISABLED)
                {
                    jsonWriter.WritePropertyName(_WS_MESSAGE_VALUE_DISABLED);
                    jsonWriter.WriteValue(disabled);
                }
                if (propertyName == _WS_MESSAGE_KEY_PROGRESS)
                {
                    jsonWriter.WritePropertyName(_WS_MESSAGE_KEY_PROGRESS);
                    jsonWriter.WriteValue(progress);
                }
                if (propertyName == _WS_MESSAGE_KEY_TEXT)
                {
                    jsonWriter.WritePropertyName(_WS_MESSAGE_KEY_TEXT);
                    jsonWriter.WriteValue(text);
                }
                if (propertyName == _WS_MESSAGE_KEY_COST)
                {
                    jsonWriter.WritePropertyName(_WS_MESSAGE_KEY_COST);
                    jsonWriter.WriteValue(cost);
                }
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();
                SendJsonString(stringWriter.ToString());
            }
            StoreIfExpectingReply(messageID, WS_MESSAGE_METHOD_SET_BUTTON_CONTROL_PROPERTIES);
        }

        private void SendGetAllGroupsMessage()
        {
            SendCallMethodMessage(WS_MESSAGE_METHOD_GET_GROUPS);
        }

        private void SendGetAllScenesMessage()
        {
            SendCallMethodMessage(WS_MESSAGE_METHOD_GET_SCENES);
        }

        private void SendGetAllParticipants()
        {
            SendCallMethodMessage(WS_MESSAGE_METHOD_GET_ALL_PARTICIPANTS);
        }

        private void SendCallMethodMessage(string method)
        {
            uint messageID = _currentmessageID++;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_TYPE);
                jsonWriter.WriteValue(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_ID);
                jsonWriter.WriteValue(messageID);
                jsonWriter.WritePropertyName(WS_MESSAGE_TYPE_METHOD);
                jsonWriter.WriteValue(method);
                jsonWriter.WritePropertyName(WS_MESSAGE_KEY_PARAMETERS);
                jsonWriter.WriteStartObject();
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEnd();

                try
                {
                    SendJsonString(stringWriter.ToString());
                }
                catch (Exception e)
                {
                    var foo = e.Message;
                    _LogError("Error: Unable to send message: " + method);
                }
            }
            StoreIfExpectingReply(messageID, method);
        }

#if UNITY_WSA && !UNITY_EDITOR
        private async void SendJsonString(string jsonString)
#else
        private void SendJsonString(string jsonString)
#endif
        {
            if (_websocket == null)
            {
                return;
            }
#if UNITY_WSA && !UNITY_EDITOR
            _messageWriter.WriteString(jsonString);
            await _messageWriter.StoreAsync();
#else
            _websocket.Send(jsonString);
#endif
            _Log(jsonString);
        }

        // We don't add every message to this list because otherwise it will become unbounded
        // and on performance critical platforms like consoles the size of the list can have an
        // effect on game performance. So we have a wrapper function that checks if we need
        // to add the message or not.
        private void StoreIfExpectingReply(uint messageID, string messageType)
        {
            if (messageType != WS_MESSAGE_METHOD_GET_ALL_PARTICIPANTS ||
                messageType != WS_MESSAGE_METHOD_GET_GROUPS ||
                messageType != WS_MESSAGE_METHOD_GET_SCENES ||
                messageType != WS_MESSAGE_METHOD_SET_CURRENT_SCENE)
            {
                _outstandingMessages.Add(messageID, messageType);
            }
        }

        List<InteractiveEventArgs> _queuedEvents = new List<InteractiveEventArgs>();
        Dictionary<uint, string> _outstandingMessages = new Dictionary<uint, string>();

#if UNITY_WSA && !UNITY_EDITOR
        MessageWebSocket _websocket;
        DataWriter _messageWriter;
#elif UNITY_XBOXONE && !UNITY_EDITOR
        Microsoft.Websocket _websocket;
#else
        WebSocketSharp.WebSocket _websocket;
#endif

        private string _interactiveWebSocketUrl = string.Empty;
        private uint _currentmessageID = 1;
        private bool _disposed = false;
        private string _authShortCodeRequestHandle;
        internal string _authToken;
        private string _oauthRefreshToken;
        private bool _initializedGroups = false;
        private bool _initializedScenes = false;
        private bool _pendingConnectToWebSocket = false;
        private bool _websocketConnected = false;
        private bool _shouldStartInteractive = true;
        private string _streamingAssetsPath = string.Empty;

        private List<InteractiveGroup> _groups;
        private List<InteractiveScene> _scenes;
        private List<InteractiveParticipant> _participants;
        private List<InteractiveControl> _controls;
        private List<InteractiveButtonControl> _buttons;
        private List<InteractiveJoystickControl> _joysticks;
        private List<string> _websocketHosts;
        private int _activeWebsocketHostIndex;

        MixerInteractiveHelper mixerInteractiveHelper;

        private const string API_BASE = "https://mixer.com/api/v1/";
        private const string WEBSOCKET_DISCOVERY_URL = API_BASE + "interactive/hosts";
        private const string API_CHECK_SHORT_CODE_AUTH_STATUS_PATH = API_BASE + "oauth/shortcode/check/";
        private const string API_GET_SHORT_CODE_PATH = API_BASE + "oauth/shortcode";
        private const string API_GET_OAUTH_TOKEN_PATH = API_BASE + "oauth/token";
        private const string INTERACTIVE_DATA_FILE_NAME = "interactivedata.json";
        private const string CONFIG_FILE_NAME = "interactiveconfig.json";
        private const float POLL_FOR_SHORT_CODE_AUTH_INTERVAL = 0.5f; // Seconds
        private const float WEBSOCKET_RECONNECT_INTERVAL = 0.5f; // Seconds

        // Consts
        private const string INTERACTIVE_CONFIG_FILE_NAME = "interactiveconfig.json";

        // Keys
        private const string WS_MESSAGE_KEY_ACCESS_TOKEN_FROM_FILE = "AuthToken";
        private const string WS_MESSAGE_KEY_APPID = "appid";
        private const string WS_MESSAGE_KEY_CHANNEL_GROUPS = "channelGroups";
        private const string WS_MESSAGE_KEY_CODE = "code";
        private const string WS_MESSAGE_KEY_COOLDOWN = "cooldown";
        private const string WS_MESSAGE_KEY_CONNECTED_AT = "connectedAt";
        private const string WS_MESSAGE_KEY_CONTROLS = "controls";
        private const string WS_MESSAGE_KEY_CONTROL_ID = "controlID";
        internal const string _WS_MESSAGE_KEY_COST = "cost";
        private const string WS_MESSAGE_KEY_DISABLED = "disabled";
        private const string WS_MESSAGE_KEY_ERROR_CODE = "code";
        private const string WS_MESSAGE_KEY_ERROR_MESSAGE = "message";
        private const string WS_MESSAGE_KEY_ERROR_PATH = "path";
        private const string WS_MESSAGE_KEY_ETAG = "etag";
        private const string WS_MESSAGE_KEY_EVENT = "event";
        private const string WS_MESSAGE_KEY_EXPIRATION = "expires_in";
        private const string WS_MESSAGE_KEY_GROUP = "group";
        private const string WS_MESSAGE_KEY_GROUPS = "groups";
        private const string WS_MESSAGE_KEY_GROUP_ID = "groupID";
        private const string WS_MESSAGE_KEY_LAST_INPUT_AT = "lastInputAt";
        private const string WS_MESSAGE_KEY_HANDLE = "handle";
        private const string WS_MESSAGE_KEY_ID = "id";
        private const string WS_MESSAGE_KEY_INPUT = "input";
        private const string WS_MESSAGE_KEY_INTENSITY = "intensity";
        private const string WS_MESSAGE_KEY_ISREADY = "isReady";
        private const string WS_MESSAGE_KEY_KIND = "kind";
        private const string WS_MESSAGE_KEY_LEVEL = "level";
        private const string WS_MESSAGE_KEY_REFRESH_TOKEN = "refresh_token";
        private const string WS_MESSAGE_KEY_REFRESH_TOKEN_FROM_FILE = "RefreshToken";
        private const string WS_MESSAGE_KEY_META = "meta";
        private const string WS_MESSAGE_KEY_PARTICIPANT_ID = "participantID";
        private const string WS_MESSAGE_KEY_PARTICIPANTS = "participants";
        private const string WS_MESSAGE_KEY_PARAMETERS = "params";
        internal const string _WS_MESSAGE_KEY_PROGRESS = "progress";
        private const string WS_MESSAGE_KEY_PROJECT_VERSION_ID = "projectversionid";
        private const string WS_MESSAGE_KEY_RESULT = "result";
        private const string WS_MESSAGE_KEY_SCENE_ID = "sceneID";
        private const string WS_MESSAGE_KEY_SCENES = "scenes";
        private const string WS_MESSAGE_KEY_SCHEME = "scheme";
        private const string WS_MESSAGE_KEY_SESSION_ID = "sessionID";
        private const string WS_MESSAGE_KEY_PROJECT_SHARE_CODE = "sharecode";
        internal const string _WS_MESSAGE_KEY_TEXT = "text";
        private const string WS_MESSAGE_KEY_TRANSACTION_ID = "transactionID";
        private const string WS_MESSAGE_KEY_TYPE = "type";
        private const string WS_MESSAGE_KEY_USER_ID = "userID";
        private const string WS_MESSAGE_KEY_USERNAME = "username";
        private const string WS_MESSAGE_KEY_VALUE = "value";
        private const string WS_MESSAGE_KEY_WEBSOCKET_ACCESS_TOKEN = "access_token";
        private const string WS_MESSAGE_KEY_WEBSOCKET_ADDRESS = "address";
        private const string WS_MESSAGE_KEY_X = "x";
        private const string WS_MESSAGE_KEY_Y = "y";

        // Values
        internal const string _WS_MESSAGE_VALUE_CONTROL_TYPE_BUTTON = "button";
        internal const string _WS_MESSAGE_VALUE_DISABLED = "disabled";
        internal const string _WS_MESSAGE_VALUE_DEFAULT_GROUP_ID = "default";
        internal const string _WS_MESSAGE_VALUE_DEFAULT_SCENE_ID = "default";
        internal const string _WS_MESSAGE_VALUE_CONTROL_TYPE_JOYSTICK = "joystick";
        internal const string _WS_MESSAGE_VALUE_CONTROL_TYPE_LABEL = "label";
        internal const string _WS_MESSAGE_VALUE_CONTROL_TYPE_TEXTBOX = "textbox";
        private const bool WS_MESSAGE_VALUE_TRUE = true;

        // Message types
        private const string WS_MESSAGE_TYPE_METHOD = "method";
        private const string WS_MESSAGE_TYPE_REPLY = "reply";

        // Methods
        private const string WS_MESSAGE_METHOD_CREATE_GROUPS = "createGroups";
        private const string WS_MESSAGE_METHOD_GET_ALL_PARTICIPANTS = "getAllParticipants";
        private const string WS_MESSAGE_METHOD_GET_GROUPS = "getGroups";
        private const string WS_MESSAGE_METHOD_GET_SCENES = "getScenes";
        private const string WS_MESSAGE_METHOD_GIVE_INPUT = "giveInput";
        private const string WS_MESSAGE_METHOD_HELLO = "hello";
        private const string WS_MESSAGE_METHOD_PARTICIPANT_JOIN = "onParticipantJoin";
        private const string WS_MESSAGE_METHOD_PARTICIPANT_LEAVE = "onParticipantLeave";
        private const string WS_MESSAGE_METHOD_PARTICIPANT_UPDATE = "onParticipantUpdate";
        private const string WS_MESSAGE_METHOD_READY = "ready";
        private const string WS_MESSAGE_METHOD_ON_CONTROL_UPDATE = "onControlUpdate";
        private const string WS_MESSAGE_METHOD_ON_CONTROL_CREATE = "onControlCreate";
        private const string WS_MESSAGE_METHOD_ON_GROUP_CREATE = "onGroupCreate";
        private const string WS_MESSAGE_METHOD_ON_GROUP_UPDATE = "onGroupUpdate";
        private const string WS_MESSAGE_METHOD_ON_READY = "onReady";
        private const string WS_MESSAGE_METHOD_ON_SCENE_CREATE = "onSceneCreate";
        private const string WS_MESSAGE_METHOD_SET_CAPTURE_TRANSACTION = "capture";
        private const string WS_MESSAGE_METHOD_SET_COMPRESSION = "setCompression";
        private const string WS_MESSAGE_METHOD_SET_CONTROL_FIRED = "setControlFired";
        private const string WS_MESSAGE_METHOD_SET_JOYSTICK_COORDINATES = "setJoystickCoordinates";
        private const string WS_MESSAGE_METHOD_SET_JOYSTICK_INTENSITY = "setJoystickIntensity";
        private const string WS_MESSAGE_METHOD_SET_BUTTON_CONTROL_PROPERTIES = "setButtonControlProperties";
        private const string WS_MESSAGE_METHOD_SET_CONTROL_TEXT = "setControlText";
        private const string WS_MESSAGE_METHOD_SET_CURRENT_SCENE = "setCurrentScene";
        private const string WS_MESSAGE_METHOD_UPDATE_CONTROLS = "updateControls";
        private const string WS_MESSAGE_METHOD_UPDATE_GROUPS = "updateGroups";
        private const string WS_MESSAGE_METHOD_UPDATE_PARTICIPANTS = "updateParticipants";
        private const string WS_MESSAGE_METHOD_UPDATE_SCENES = "updateScenes";

        // Other message types
        private const string WS_MESSAGE_ERROR = "error";

        // Input
        internal const string _CONTROL_TYPE_BUTTON = "button";
        internal const string _CONTROL_TYPE_JOYSTICK = "joystick";
        internal const string _CONTROL_KIND_LABEL = "label";
        internal const string _CONTROL_KIND_TEXTBOX = "textbox";
        internal const string _CONTROL_KIND_SCREEN = "screen";

        // Event names
        private const string EVENT_NAME_MOUSE_DOWN = "mousedown";
        private const string EVENT_NAME_MOUSE_UP = "mouseup";
        private const string EVENT_NAME_KEY_DOWN = "keydown";
        private const string EVENT_NAME_KEY_UP = "keyup";
        private const string EVENT_NAME_MOVE = "move";
        private const string EVENT_NAME_SUBMIT = "submit";

        // Message parameters
        private const string BOOLEAN_TRUE_VALUE = "true";
        private const string COMPRESSION_TYPE_GZIP = "gzip";
        private const string READY_PARAMETER_IS_READY = "isReady";

        // Errors
        private int ERROR_FAIL = 83;

        // Misc
        private const string PROTOCOL_VERSION = "2.0";

        // Control-specific data structures
        internal static Dictionary<string, _InternalButtonCountState> _buttonStates;
        internal static Dictionary<string, Dictionary<string, _InternalButtonState>> _buttonStatesByParticipant;
        internal static Dictionary<string, _InternalJoystickState> _joystickStates;
        internal static Dictionary<string, Dictionary<string, _InternalJoystickState>> _joystickStatesByParticipant;
        internal static Dictionary<string, Dictionary<string, string>> _textboxValuesByParticipant;
        internal static Dictionary<string, _InternalMouseButtonState> _mouseButtonStateByParticipant;
        internal static Dictionary<string, Vector2> _mousePositionsByParticipant;

        // Generic data structures for storing any control data
        internal static Dictionary<string, Dictionary<string, Dictionary<string, object>>> _giveInputControlDataByParticipant;
        internal static Dictionary<string, Dictionary<string, object>> _giveInputControlData;
        internal static Dictionary<string, object> _giveInputKeyValues;
        internal static Dictionary<string, _InternalParticipantTrackingState> _participantsWhoTriggeredGiveInput;
        private static Dictionary<string, Dictionary<string, _InternalControlPropertyUpdateData>> _queuedControlPropertyUpdates;
        internal static Dictionary<string, InternalTransactionIDState> _transactionIDsState;

        // For MockData
        public static bool useMockData = false;

#if UNITY_XBOXONE && !UNITY_EDITOR
        [DllImport("MixerEraNativePlugin")]
        private static extern bool MixerEraNativePlugin_GetXToken(System.IntPtr strData);

        [DllImport("MixerEraNativePlugin")]
        private static extern long MixerEraNativePlugin_GetSystemTime();
#endif

        // Ctor
        private void InitializeInternal()
        {
            UpdateInteractivityState(InteractivityState.NotInitialized);

            _buttons = new List<InteractiveButtonControl>();
            _controls = new List<InteractiveControl>();
            _groups = new List<InteractiveGroup>();
            _joysticks = new List<InteractiveJoystickControl>();
            _participants = new List<InteractiveParticipant>();
            _scenes = new List<InteractiveScene>();
            _websocketHosts = new List<string>();

            _buttonStates = new Dictionary<string, _InternalButtonCountState>();
            _buttonStatesByParticipant = new Dictionary<string, Dictionary<string, _InternalButtonState>>();

            if (Application.isEditor)
            {
#if UNITY_EDITOR
                string loggingLevelAsString = UnityEditor.EditorPrefs.GetString("MixerInteractive_LoggingLevel");
                switch (loggingLevelAsString)
                {
                    case "none":
                        SingletonInstance.LoggingLevel = LoggingLevel.None;
                        break;
                    case "minimal":
                        SingletonInstance.LoggingLevel = LoggingLevel.Minimal;
                        break;
                    case "verbose":
                        SingletonInstance.LoggingLevel = LoggingLevel.Verbose;
                        break;
                    default:
                        SingletonInstance.LoggingLevel = LoggingLevel.Minimal;
                        break;
                };
#endif
            }
            else
            {
                LoggingLevel = LoggingLevel.None;
            }

            _joystickStates = new Dictionary<string, _InternalJoystickState>();
            _joystickStatesByParticipant = new Dictionary<string, Dictionary<string, _InternalJoystickState>>();
            _mouseButtonStateByParticipant = new Dictionary<string, _InternalMouseButtonState>();
            _mousePositionsByParticipant = new Dictionary<string, Vector2>();

            _participantsWhoTriggeredGiveInput = new Dictionary<string, _InternalParticipantTrackingState>();
            _queuedControlPropertyUpdates = new Dictionary<string, Dictionary<string, _InternalControlPropertyUpdateData>>();
            _transactionIDsState = new Dictionary<string, InternalTransactionIDState>();

            _giveInputControlDataByParticipant = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
            _giveInputControlData = new Dictionary<string, Dictionary<string, object>>();
            _giveInputKeyValues = new Dictionary<string, object>();
            _textboxValuesByParticipant = new Dictionary<string, Dictionary<string, string>>();

            _streamingAssetsPath = Application.streamingAssetsPath;

            CreateStorageDirectoryIfNotExists();

#if UNITY_WSA && !UNITY_EDITOR
            _websocket = new MessageWebSocket();
            _messageWriter = new DataWriter(_websocket.OutputStream);
#endif

            mixerInteractiveHelper = MixerInteractiveHelper._SingletonInstance;
        }

        private void OnInternalRefreshShortCodeTimerCallback(object sender, MixerInteractiveHelper.InternalTimerCallbackEventArgs e)
        {
            mixerInteractiveHelper.OnInternalRefreshShortCodeTimerCallback -= OnInternalRefreshShortCodeTimerCallback;
            RefreshShortCode();
        }

        private void OnInternalReconnectTimerCallback(object sender, MixerInteractiveHelper.InternalTimerCallbackEventArgs e)
        {
            mixerInteractiveHelper.OnInternalReconnectTimerCallback -= OnInternalReconnectTimerCallback;
            VerifyAuthToken();
        }

        internal void _LogError(string message)
        {
            _LogError(message, ERROR_FAIL);
        }

        internal void _LogError(string message, int code)
        {
            _queuedEvents.Add(new InteractiveEventArgs(InteractiveEventType.Error, code, message));
            _Log(message, LoggingLevel.Minimal);
        }

        internal void _Log(string message, LoggingLevel level = LoggingLevel.Verbose)
        {
            if (LoggingLevel == LoggingLevel.None ||
                (LoggingLevel == LoggingLevel.Minimal && level == LoggingLevel.Verbose))
            {
                return;
            }
            UnityEngine.Debug.Log(message);
        }

        private void ClearPreviousControlState()
        {
            if (InteractivityState != InteractivityState.InteractivityEnabled)
            {
                return;
            }

            List<string> _buttonStatesKeys = new List<string>(_buttonStates.Keys);
            foreach (string key in _buttonStatesKeys)
            {
                _InternalButtonCountState oldButtonState = _buttonStates[key];
                _InternalButtonCountState newButtonState = new _InternalButtonCountState();
                newButtonState.PreviousCountOfButtonDownEvents = oldButtonState.CountOfButtonDownEvents;
                newButtonState.CountOfButtonDownEvents = oldButtonState.NextCountOfButtonDownEvents;
                newButtonState.NextCountOfButtonDownEvents = 0;

                newButtonState.PreviousCountOfButtonPressEvents = oldButtonState.CountOfButtonPressEvents;
                newButtonState.CountOfButtonPressEvents = oldButtonState.NextCountOfButtonPressEvents;
                newButtonState.NextCountOfButtonPressEvents = 0;

                newButtonState.PreviousCountOfButtonUpEvents = oldButtonState.CountOfButtonUpEvents;
                newButtonState.CountOfButtonUpEvents = oldButtonState.NextCountOfButtonUpEvents;
                newButtonState.NextCountOfButtonUpEvents = 0;

                newButtonState.PreviousTransactionID = oldButtonState.TransactionID;
                newButtonState.TransactionID = oldButtonState.NextTransactionID;
                newButtonState.NextTransactionID = string.Empty;

                _buttonStates[key] = newButtonState;
            }

            List<string> _buttonStatesByParticipantKeys = new List<string>(_buttonStatesByParticipant.Keys);
            foreach (string key in _buttonStatesByParticipantKeys)
            {
                List<string> _buttonStatesByParticipantButtonStateKeys = new List<string>(_buttonStatesByParticipant[key].Keys);
                foreach (string controlKey in _buttonStatesByParticipantButtonStateKeys)
                {
                    _InternalButtonState oldButtonState = _buttonStatesByParticipant[key][controlKey];
                    _InternalButtonState newButtonState = new _InternalButtonState();
                    _InternalButtonCountState buttonCountState = new _InternalButtonCountState();
                    buttonCountState.PreviousCountOfButtonDownEvents = oldButtonState.ButtonCountState.CountOfButtonDownEvents;
                    buttonCountState.CountOfButtonDownEvents = oldButtonState.ButtonCountState.NextCountOfButtonDownEvents;
                    buttonCountState.NextCountOfButtonDownEvents = 0;

                    buttonCountState.PreviousCountOfButtonPressEvents = oldButtonState.ButtonCountState.CountOfButtonPressEvents;
                    buttonCountState.CountOfButtonPressEvents = oldButtonState.ButtonCountState.NextCountOfButtonPressEvents;
                    buttonCountState.NextCountOfButtonPressEvents = 0;

                    buttonCountState.PreviousCountOfButtonUpEvents = oldButtonState.ButtonCountState.CountOfButtonUpEvents;
                    buttonCountState.CountOfButtonUpEvents = oldButtonState.ButtonCountState.NextCountOfButtonUpEvents;
                    buttonCountState.NextCountOfButtonUpEvents = 0;

                    newButtonState.ButtonCountState = buttonCountState;
                    _buttonStatesByParticipant[key][controlKey] = newButtonState;
                }
            }

            // Mouse button state
            List<string> _mouseButtonStateByParticipantKeys = new List<string>(_mouseButtonStateByParticipant.Keys);
            foreach (string _mouseButtonStateByParticipantKey in _mouseButtonStateByParticipantKeys)
            {
                _InternalMouseButtonState oldMouseButtonState = _mouseButtonStateByParticipant[_mouseButtonStateByParticipantKey];
                _InternalMouseButtonState newMouseButtonState = new _InternalMouseButtonState();
                // If we just recieved a mouse down, but not an up, then the state is pressed
                if (oldMouseButtonState.NextIsDown)
                {
                    newMouseButtonState.IsDown = true;
                    newMouseButtonState.IsPressed = true;
                    newMouseButtonState.IsUp = false;

                    newMouseButtonState.NextIsDown = false;
                    newMouseButtonState.NextIsPressed = true;
                    newMouseButtonState.NextIsUp = false;
                }
                else if (oldMouseButtonState.NextIsUp)
                {
                    newMouseButtonState.IsDown = false;
                    newMouseButtonState.IsPressed = false;
                    newMouseButtonState.IsUp = true;

                    newMouseButtonState.NextIsDown = false;
                    newMouseButtonState.NextIsPressed = false;
                    newMouseButtonState.NextIsUp = false;
                }
                else if (oldMouseButtonState.NextIsPressed)
                {
                    newMouseButtonState.IsDown = false;
                    newMouseButtonState.IsPressed = true;
                    newMouseButtonState.IsUp = false;

                    newMouseButtonState.NextIsDown = false;
                    newMouseButtonState.NextIsPressed = true;
                    newMouseButtonState.NextIsUp = false;
                }
                else
                {
                    newMouseButtonState.IsDown = false;
                    newMouseButtonState.IsPressed = false;
                    newMouseButtonState.IsUp = false;

                    newMouseButtonState.NextIsDown = false;
                    newMouseButtonState.NextIsPressed = false;
                    newMouseButtonState.NextIsUp = false;
                }
                _mouseButtonStateByParticipant[_mouseButtonStateByParticipantKey] = newMouseButtonState;
            }

            var controlIDKeys = new List<string>(_participantsWhoTriggeredGiveInput.Keys);
            foreach (string controlIDKey in controlIDKeys)
            {
                _InternalParticipantTrackingState oldParticipantTrackingState = _participantsWhoTriggeredGiveInput[controlIDKey];
                _InternalParticipantTrackingState newParticipantTrackingState = new _InternalParticipantTrackingState();

                newParticipantTrackingState.previousParticpant = oldParticipantTrackingState.particpant;
                newParticipantTrackingState.particpant = oldParticipantTrackingState.nextParticpant;
                newParticipantTrackingState.nextParticpant = null;

                _participantsWhoTriggeredGiveInput[controlIDKey] = newParticipantTrackingState;
            }

            // Clear transaction IDs
            var transactionIDsStateKeys = new List<string>(_transactionIDsState.Keys);
            foreach (string transactionIDsStateKey in transactionIDsStateKeys)
            {
                InternalTransactionIDState oldTransactionIDState = _transactionIDsState[transactionIDsStateKey];
                InternalTransactionIDState newTransactionIDState = new InternalTransactionIDState();

                newTransactionIDState.previousTransactionID = oldTransactionIDState.transactionID;
                newTransactionIDState.transactionID = oldTransactionIDState.nextTransactionID;
                newTransactionIDState.nextTransactionID = string.Empty;

                _transactionIDsState[transactionIDsStateKey] = newTransactionIDState;
            }
        }

        private void UpdateInternalButtonState(InteractiveButtonEventArgs e)
        {
            // Make sure the entry exists
            string sessionID = e.Participant.SessionID;
            string controlID = e.ControlID;
            Dictionary<string, _InternalButtonState> buttonState;
            bool participantEntryExists = _buttonStatesByParticipant.TryGetValue(sessionID, out buttonState);
            if (!participantEntryExists)
            {
                buttonState = new Dictionary<string, _InternalButtonState>();
                _InternalButtonState newControlButtonState = new _InternalButtonState();
                newControlButtonState.IsDown = e.IsPressed;
                newControlButtonState.IsPressed = e.IsPressed;
                newControlButtonState.IsUp = !e.IsPressed;
                buttonState.Add(controlID, newControlButtonState);
                _buttonStatesByParticipant.Add(sessionID, buttonState);
            }
            else
            {
                _InternalButtonState controlButtonState;
                bool previousStateControlEntryExists = buttonState.TryGetValue(controlID, out controlButtonState);
                if (!previousStateControlEntryExists)
                {
                    controlButtonState = new _InternalButtonState();
                    _InternalButtonState newControlButtonState = new _InternalButtonState();
                    newControlButtonState.IsDown = e.IsPressed;
                    newControlButtonState.IsPressed = e.IsPressed;
                    newControlButtonState.IsUp = !e.IsPressed;
                    buttonState.Add(controlID, newControlButtonState);
                }
            }

            // Populate the structure that's by participant
            bool wasPreviouslyPressed = _buttonStatesByParticipant[sessionID][controlID].ButtonCountState.NextCountOfButtonPressEvents > 0;
            bool isCurrentlyPressed = e.IsPressed;
            _InternalButtonState newState = _buttonStatesByParticipant[sessionID][controlID];
            if (isCurrentlyPressed)
            {
                if (!wasPreviouslyPressed)
                {
                    newState.IsDown = true;
                    newState.IsPressed = true;
                    newState.IsUp = false;
                }
                else
                {
                    newState.IsDown = false;
                    newState.IsPressed = true;
                    newState.IsUp = false;
                }
            }
            else
            {
                // This means IsPressed on the event was false, so it was a mouse up event.
                newState.IsDown = false;
                newState.IsPressed = false;
                newState.IsUp = true;
            }

            // Fill in the button counts
            _InternalButtonCountState ButtonCountState = newState.ButtonCountState;
            if (newState.IsDown)
            {
                ButtonCountState.NextCountOfButtonDownEvents++;
            }
            if (newState.IsPressed)
            {
                ButtonCountState.NextCountOfButtonPressEvents++;
            }
            if (newState.IsUp)
            {
                ButtonCountState.NextCountOfButtonUpEvents++;
            }
            if (!string.IsNullOrEmpty(e.TransactionID))
            {
                ButtonCountState.NextTransactionID = e.TransactionID;
            }
            newState.ButtonCountState = ButtonCountState;

            _buttonStatesByParticipant[sessionID][controlID] = newState;

            // Populate button count state
            _InternalButtonCountState existingButtonCountState;
            bool buttonStateExists = _buttonStates.TryGetValue(controlID, out existingButtonCountState);
            if (buttonStateExists)
            {
                _buttonStates[controlID] = newState.ButtonCountState;
            }
            else
            {
                _buttonStates.Add(controlID, newState.ButtonCountState);
            }
        }

        private void UpdateInternalJoystickState(InteractiveJoystickEventArgs e)
        {
            // Make sure the entry exists
            string sessionID = e.Participant.SessionID;
            string controlID = e.ControlID;
            Dictionary<string, _InternalJoystickState> joystickByParticipant;
            _InternalJoystickState newJoystickStateByParticipant;
            bool participantEntryExists = _joystickStatesByParticipant.TryGetValue(sessionID, out joystickByParticipant);
            if (!participantEntryExists)
            {
                joystickByParticipant = new Dictionary<string, _InternalJoystickState>();
                newJoystickStateByParticipant = new _InternalJoystickState();
                newJoystickStateByParticipant.X = e.X;
                newJoystickStateByParticipant.Y = e.Y;
                newJoystickStateByParticipant.countOfUniqueJoystickInputs = 1;
                _joystickStatesByParticipant.Add(sessionID, joystickByParticipant);
            }
            else
            {
                newJoystickStateByParticipant = new _InternalJoystickState();
                bool joystickByParticipantEntryExists = joystickByParticipant.TryGetValue(controlID, out newJoystickStateByParticipant);
                if (!joystickByParticipantEntryExists)
                {
                    newJoystickStateByParticipant.X = e.X;
                    newJoystickStateByParticipant.Y = e.Y;
                    newJoystickStateByParticipant.countOfUniqueJoystickInputs = 1;
                    joystickByParticipant.Add(controlID, newJoystickStateByParticipant);
                }
                int countOfUniqueJoystickByParticipantInputs = newJoystickStateByParticipant.countOfUniqueJoystickInputs;
                // We always give the average of the joystick so that there is input smoothing.
                newJoystickStateByParticipant.X =
                    (newJoystickStateByParticipant.X * (countOfUniqueJoystickByParticipantInputs - 1) / (countOfUniqueJoystickByParticipantInputs)) +
                    (e.X * (1 / countOfUniqueJoystickByParticipantInputs));
                newJoystickStateByParticipant.Y =
                    (newJoystickStateByParticipant.Y * (countOfUniqueJoystickByParticipantInputs - 1) / (countOfUniqueJoystickByParticipantInputs)) +
                    (e.Y * (1 / countOfUniqueJoystickByParticipantInputs));
            }
            _joystickStatesByParticipant[sessionID][e.ControlID] = newJoystickStateByParticipant;

            // Update the joystick state
            _InternalJoystickState newJoystickState;
            bool joystickEntryExists = joystickByParticipant.TryGetValue(controlID, out newJoystickState);
            if (!joystickEntryExists)
            {
                newJoystickState.X = e.X;
                newJoystickState.Y = e.Y;
                newJoystickState.countOfUniqueJoystickInputs = 1;
                joystickByParticipant.Add(controlID, newJoystickState);
            }
            newJoystickState.countOfUniqueJoystickInputs++;
            int countOfUniqueJoystickInputs = newJoystickState.countOfUniqueJoystickInputs;
            // We always give the average of the joystick so that there is input smoothing.
            newJoystickState.X =
                (newJoystickState.X * (countOfUniqueJoystickInputs - 1) / (countOfUniqueJoystickInputs)) +
                (e.X * (1 / countOfUniqueJoystickInputs));
            newJoystickState.Y =
                (newJoystickState.Y * (countOfUniqueJoystickInputs - 1) / (countOfUniqueJoystickInputs)) +
                (e.Y * (1 / countOfUniqueJoystickInputs));
            _joystickStates[e.ControlID] = newJoystickState;
        }

        private void UpdateInternalTextBoxState(InteractiveTextEventArgs e)
        {
            // Make sure the entry exists
            string sessionID = e.Participant.SessionID;
            string controlID = e.ControlID;
            string text = e.Text;
            Dictionary<string, string> newTextStateByParticipant;
            string newTextState = string.Empty;
            bool participantEntryExists = _textboxValuesByParticipant.TryGetValue(sessionID, out newTextStateByParticipant);
            if (!participantEntryExists)
            {
                newTextStateByParticipant = new Dictionary<string, string>();
                newTextStateByParticipant.Add(controlID, text);
                _textboxValuesByParticipant.Add(sessionID, newTextStateByParticipant);
            }
            else
            {
                bool textStateByParticipantEntryExists = newTextStateByParticipant.TryGetValue(controlID, out newTextState);
                if (!textStateByParticipantEntryExists)
                {
                    newTextStateByParticipant.Add(controlID, text);
                }
            }
            _textboxValuesByParticipant[sessionID][e.ControlID] = text;
        }

        private void UpdateInternalMouseButtonState(InteractiveMouseButtonEventArgs e)
        {
            // Make sure the entry exists
            string sessionId = e.Participant.SessionID;
            bool isPressed = e.IsPressed;
            _InternalMouseButtonState buttonState;
            bool participantEntryExists = _mouseButtonStateByParticipant.TryGetValue(sessionId, out buttonState);
            if (!participantEntryExists)
            {
                buttonState = new _InternalMouseButtonState();
                buttonState.IsDown = false;
                buttonState.IsPressed = false;
                buttonState.IsUp = false;
                buttonState.NextIsDown = e.IsPressed;
                buttonState.NextIsPressed = e.IsPressed;
                buttonState.NextIsUp = !e.IsPressed;
                _mouseButtonStateByParticipant.Add(sessionId, buttonState);
            }
            _InternalMouseButtonState newState = _mouseButtonStateByParticipant[sessionId];
            newState.NextIsDown = isPressed;
            newState.NextIsPressed = isPressed;
            newState.NextIsUp = !isPressed;
            _mouseButtonStateByParticipant[sessionId] = newState;
        }

        internal void _QueuePropertyUpdate(string sceneID, string controlID, string name, bool value)
        {
            _KnownControlPropertyPrimitiveTypes type = _KnownControlPropertyPrimitiveTypes.Boolean;
            _QueuePropertyUpdateImpl(sceneID, controlID, name, type, value);
        }
        internal void _QueuePropertyUpdate(string sceneID, string controlID, string name, double value)
        {
            _KnownControlPropertyPrimitiveTypes type = _KnownControlPropertyPrimitiveTypes.Number;
            _QueuePropertyUpdateImpl(sceneID, controlID, name, type, value);
        }
        internal void _QueuePropertyUpdate(string sceneID, string controlID, string name, string value)
        {
            _KnownControlPropertyPrimitiveTypes type = _KnownControlPropertyPrimitiveTypes.String;
            _QueuePropertyUpdateImpl(sceneID, controlID, name, type, value);
        }
        internal void _QueuePropertyUpdate(string sceneID, string controlID, string name, object value)
        {
            _KnownControlPropertyPrimitiveTypes type = _KnownControlPropertyPrimitiveTypes.Unknown;
            _QueuePropertyUpdateImpl(sceneID, controlID, name, type, value);
        }

        internal void _QueuePropertyUpdateImpl(string sceneID, string controlID, string name, _KnownControlPropertyPrimitiveTypes type, object value)
        {
            // If a scene entry doesn't exist, add one.
            if (!_queuedControlPropertyUpdates.ContainsKey(sceneID))
            {
                _InternalControlPropertyUpdateData controlPropertyData = new _InternalControlPropertyUpdateData(name, type, value);
                Dictionary<string, _InternalControlPropertyUpdateData> controlData = new Dictionary<string, _InternalControlPropertyUpdateData>();
                controlData.Add(controlID, controlPropertyData);
                _queuedControlPropertyUpdates.Add(sceneID, controlData);
            }
            else
            {
                // Scene exists, but if control entry doesn't exist, create one.
                Dictionary<string, _InternalControlPropertyUpdateData> controlData = _queuedControlPropertyUpdates[sceneID];
                if (!controlData.ContainsKey(controlID))
                {
                    _InternalControlPropertyUpdateData controlPropertyData = new _InternalControlPropertyUpdateData(name, type, value);
                    _queuedControlPropertyUpdates[sceneID].Add(controlID, controlPropertyData);
                }
                else
                {
                    // Control entry exists, but does property entry exist?
                    _InternalControlPropertyUpdateData controlPropertyData = controlData[controlID];
                    _InternalControlPropertyMetaData controlPropertyMetaData = new _InternalControlPropertyMetaData();
                    controlPropertyMetaData.type = type;
                    if (type == _KnownControlPropertyPrimitiveTypes.Boolean)
                    {
                        controlPropertyMetaData.boolValue = (bool)value;
                    }
                    else if (type == _KnownControlPropertyPrimitiveTypes.Number)
                    {
                        controlPropertyMetaData.numberValue = (double)value;
                    }
                    else
                    {
                        controlPropertyMetaData.stringValue = value.ToString();
                    }
                    if (!controlPropertyData.properties.ContainsKey(name))
                    {
                        _queuedControlPropertyUpdates[sceneID][controlID].properties.Add(name, controlPropertyMetaData);
                    }
                    else
                    {
                        _queuedControlPropertyUpdates[sceneID][controlID].properties[name] = controlPropertyMetaData;
                    }
                }
            }
        }

        internal void _RegisterControlForValueUpdates(string controlTypeName, List<string> valuesToTrack)
        {
            if (!_giveInputControlData.ContainsKey(controlTypeName))
            {
                Dictionary<string, object> controlValues = new Dictionary<string, object>();
                _giveInputControlData[controlTypeName] = controlValues;
            }
            foreach (string key in valuesToTrack)
            {
                if (!_giveInputKeyValues.ContainsKey(key))
                {
                    _giveInputKeyValues.Add(key, null);
                }
            }
        }

        internal string _InteractiveControlPropertyToString(InteractiveControlProperty property)
        {
            string controlPropertyString = string.Empty;
            switch (property)
            {
                case InteractiveControlProperty.Text:
                    controlPropertyString = "text";
                    break;
                case InteractiveControlProperty.BackgroundColor:
                    controlPropertyString = "backgroundColor";
                    break;
                case InteractiveControlProperty.BackgroundImage:
                    controlPropertyString = "backgroundImage";
                    break;
                case InteractiveControlProperty.TextColor:
                    controlPropertyString = "textColor";
                    break;
                case InteractiveControlProperty.TextSize:
                    controlPropertyString = "textSize";
                    break;
                case InteractiveControlProperty.BorderColor:
                    controlPropertyString = "borderColor";
                    break;
                case InteractiveControlProperty.FocusColor:
                    controlPropertyString = "focusColor";
                    break;
                case InteractiveControlProperty.AccentColor:
                    controlPropertyString = "accentColor";
                    break;
                default:
                    // No-op: Unexpected property.
                    break;
            }
            return controlPropertyString;
        }
    }

    internal struct _InternalButtonCountState
    {
        internal uint PreviousCountOfButtonDownEvents;
        internal uint PreviousCountOfButtonPressEvents;
        internal uint PreviousCountOfButtonUpEvents;

        internal uint CountOfButtonDownEvents;
        internal uint CountOfButtonPressEvents;
        internal uint CountOfButtonUpEvents;

        internal uint NextCountOfButtonDownEvents;
        internal uint NextCountOfButtonPressEvents;
        internal uint NextCountOfButtonUpEvents;

        internal string PreviousTransactionID;
        internal string TransactionID;
        internal string NextTransactionID;
    }

    internal struct _InternalButtonState
    {
        internal bool IsDown;
        internal bool IsPressed;
        internal bool IsUp;
        internal _InternalButtonCountState ButtonCountState;
    }

    internal struct _InternalControlPropertyUpdateData
    {
        internal Dictionary<string, _InternalControlPropertyMetaData> properties;
        public _InternalControlPropertyUpdateData(string name, _KnownControlPropertyPrimitiveTypes type, object value)
        {
            properties = new Dictionary<string, _InternalControlPropertyMetaData>();
            _InternalControlPropertyMetaData typeData = new _InternalControlPropertyMetaData();
            typeData.type = type;
            // This should never fail, but just in case.
            try
            {
                if (type == _KnownControlPropertyPrimitiveTypes.Boolean)
                {
                    typeData.boolValue = (bool)value;
                }
                else if (type == _KnownControlPropertyPrimitiveTypes.Number)
                {
                    typeData.numberValue = (double)value;
                }
                else
                {
                    typeData.stringValue = value.ToString();
                }
            }
            catch (Exception ex)
            {
                InteractivityManager.SingletonInstance._LogError("Failed to cast the value to a known type. Exception: " + ex.Message);
            }
            properties.Add(name, typeData);
        }
    }

    internal struct _InternalControlPropertyMetaData
    {
        public object objectValue;
        public bool boolValue;
        public double numberValue;
        public string stringValue;
        public _KnownControlPropertyPrimitiveTypes type;
    }

    internal enum _KnownControlPropertyPrimitiveTypes
    {
        Unknown,
        Boolean,
        Number,
        String
    }

    internal struct _InternalJoystickState
    {
        internal double X;
        internal double Y;
        internal int countOfUniqueJoystickInputs;
    }

    internal struct _InternalMouseButtonState
    {
        internal bool IsDown;
        internal bool IsPressed;
        internal bool IsUp;
        internal bool NextIsDown;
        internal bool NextIsPressed;
        internal bool NextIsUp;
    }

    internal struct _InternalParticipantTrackingState
    {
        internal InteractiveParticipant previousParticpant;
        internal InteractiveParticipant particpant;
        internal InteractiveParticipant nextParticpant;
        public _InternalParticipantTrackingState(InteractiveParticipant newParticipant)
        {
            nextParticpant = newParticipant;
            particpant = null;
            previousParticpant = null;
        }
    }

    internal struct InternalTransactionIDState
    {
        internal string previousTransactionID;
        internal string transactionID;
        internal string nextTransactionID;
        public InternalTransactionIDState(string newTransactionID)
        {
            nextTransactionID = newTransactionID;
            transactionID = null;
            previousTransactionID = null;
        }
    }
}