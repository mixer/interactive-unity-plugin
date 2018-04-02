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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

internal class MixerInteractiveHelper: MonoBehaviour
{
    internal bool runInBackgroundIfInteractive = true;
    internal string defaultSceneID;
    internal Dictionary<string, string> groupSceneMapping = new Dictionary<string, string>();

    public delegate void OnInternalWebRequestStateChangedEventHandler(object sender, InternalWebRequestStateChangedEventArgs e);
    public event OnInternalWebRequestStateChangedEventHandler OnInternalWebRequestStateChanged;

    public delegate void OnInternalCheckAuthStatusCallbackEventHandler(object sender, InternalTimerCallbackEventArgs e);
    public event OnInternalCheckAuthStatusCallbackEventHandler OnInternalCheckAuthStatusTimerCallback;

    public delegate void OnInternalRefreshShortCodeCallbackEventHandler(object sender, InternalTimerCallbackEventArgs e);
    public event OnInternalRefreshShortCodeCallbackEventHandler OnInternalRefreshShortCodeTimerCallback;

    public delegate void OnInternalReconnectCallbackEventHandler(object sender, InternalTimerCallbackEventArgs e);
    public event OnInternalReconnectCallbackEventHandler OnInternalReconnectTimerCallback;

    public delegate void OnTryGetAuthTokensFromCacheCallbackEventHandler(object sender, TryGetAuthTokensFromCacheEventArgs e);
    public event OnTryGetAuthTokensFromCacheCallbackEventHandler OnTryGetAuthTokensFromCacheCallback;

    private List<InteractiveWebRequestData> _queuedWebRequests;
    private List<InteractiveTimerData> _queuedStartTimerRequests;
    private List<InteractiveTimerType> _queuedStopTimerRequests;
    private List<CoRoutineInfo> _runningCoRoutines;
    private bool _queuedTryGetAuthTokensFromCacheRequest;
    private bool _queuedWriteAuthTokensToCacheRequest;
    private string _authTokenValueToWriteToCache;
    private string _refreshTokenValueToWriteToCache;

    private static MixerInteractiveHelper _singletonInstance;
    internal static MixerInteractiveHelper SingletonInstance
    {
        get
        {
            if (_singletonInstance == null)
            {
                MixerInteractiveHelper[] MixerInteractiveHelperInstances = FindObjectsOfType<MixerInteractiveHelper>();
                if (MixerInteractiveHelperInstances.Length > 0)
                {
                    _singletonInstance = MixerInteractiveHelperInstances[0];
                }
                _singletonInstance.Initialize();
            }
            return _singletonInstance;
        }
    }

    void Update()
    {
        if (_singletonInstance != null)
        {
            foreach (InteractiveWebRequestData _queuedWebRequest in _queuedWebRequests)
            {
                _runningCoRoutines.Add(new CoRoutineInfo(
                    "MakeWebRequestCoRoutine",
                    StartCoroutine(MakeWebRequestCoRoutine(
                    _queuedWebRequest.requestID,
                    _queuedWebRequest.requestUrl, 
                    _queuedWebRequest.headers,
                    _queuedWebRequest.httpVerb,
                    _queuedWebRequest.postData)
                    )));
            }
            _queuedWebRequests.Clear();

            foreach (InteractiveTimerData _queuedStartTimerRequest in _queuedStartTimerRequests)
            {
                InteractiveTimerType type = _queuedStartTimerRequest.type;
                float interval = _queuedStartTimerRequest.interval;
                switch (type)
                {
                    case InteractiveTimerType.CheckAuthStatus:
                        StopCoroutineByName("CheckAuthStatusCoRoutine");
                        _runningCoRoutines.Add(new CoRoutineInfo(
                            "CheckAuthStatusCoRoutine",
                            StartCoroutine(CheckAuthStatusCoRoutine(interval)
                            )));
                        break;
                    case InteractiveTimerType.RefreshShortCode:
                        StopCoroutineByName("RefreshShortCodeCoRoutine");
                        _runningCoRoutines.Add(new CoRoutineInfo(
                            "RefreshShortCodeCoRoutine",
                            StartCoroutine(RefreshShortCodeCoRoutine(interval)
                            )));
                        break;
                    case InteractiveTimerType.Reconnect:
                        StopCoroutineByName("ReconnectCodeCoRoutine");
                        _runningCoRoutines.Add(new CoRoutineInfo(
                            "ReconnectCodeCoRoutine",
                            StartCoroutine(ReconnectCodeCoRoutine(interval)
                            )));
                        break;
                    default:
                        // No-op
                        break;
                }
            }
            _queuedStartTimerRequests.Clear();

            foreach (InteractiveTimerType _queuedStopTimerRequest in _queuedStopTimerRequests)
            {
                switch (_queuedStopTimerRequest)
                {
                    case InteractiveTimerType.CheckAuthStatus:
                        StopCoroutineByName("CheckAuthStatusCoRoutine");
                        break;
                    case InteractiveTimerType.RefreshShortCode:
                        StopCoroutineByName("RefreshShortCodeCoRoutine");
                        break;
                    case InteractiveTimerType.Reconnect:
                        StopCoroutineByName("ReconnectCodeCoRoutine");
                        break;
                    default:
                        // No-op
                        break;
                }
            }
            _queuedStopTimerRequests.Clear();

            if (_queuedTryGetAuthTokensFromCacheRequest)
            {
                TryGetAuthTokensFromCacheEventArgs tryGetAuthTokensFromCacheEventArgs = new TryGetAuthTokensFromCacheEventArgs();
                tryGetAuthTokensFromCacheEventArgs.AuthToken = PlayerPrefs.GetString("MixerInteractive-AuthToken");
                tryGetAuthTokensFromCacheEventArgs.RefreshToken = PlayerPrefs.GetString("MixerInteractive-RefreshToken");
                if (OnTryGetAuthTokensFromCacheCallback != null)
                {
                    OnTryGetAuthTokensFromCacheCallback(this, tryGetAuthTokensFromCacheEventArgs);
                }
                _queuedTryGetAuthTokensFromCacheRequest = false;
            }

            if (_queuedWriteAuthTokensToCacheRequest)
            {
                WriteAuthTokensToCacheImpl();
            }
        }
    }

    private void StopCoroutineByName(string name)
    {
        // If there is more than one CoRoutine, then we'll stop all of them.
        foreach (CoRoutineInfo coRoutineInfo in _runningCoRoutines)
        {
            if (coRoutineInfo.name == name)
            {
                StopCoroutine(coRoutineInfo.coRoutine);
            }
        }
    }

    private void Initialize()
    {
        _queuedWebRequests = new List<InteractiveWebRequestData>();
        _queuedStartTimerRequests = new List<InteractiveTimerData>();
        _queuedStopTimerRequests = new List<InteractiveTimerType>();
        _runningCoRoutines = new List<CoRoutineInfo>();
    }

    internal void MakeWebRequest(
        string requestID,
        string requestUrl,
        Dictionary<string, string> headers = null,
        string httpVerb = "",
        string postData = ""
        )
    {
        _queuedWebRequests.Add(new InteractiveWebRequestData(
            requestID,
            requestUrl,
            headers,
            httpVerb,
            postData
        ));
    }

    internal struct InteractiveWebRequestData
    {
        public string requestID;
        public string requestUrl;
        public Dictionary<string, string> headers;
        public string httpVerb;
        public string postData;
        public InteractiveWebRequestData(
            string newRequestID, 
            string newRequestUrl, 
            Dictionary<string, string> newHeaders, 
            string newHttpVerb,
            string newPostData
            )
        {
            requestID = newRequestID;
            requestUrl = newRequestUrl;
            headers = newHeaders;
            httpVerb = newHttpVerb;
            postData = newPostData;
        }
    }

    internal class InternalWebRequestStateChangedEventArgs
    {
        public string RequestID
        {
            get;
            private set;
        }
        public bool Succeeded
        {
            get;
            private set;
        }
        public long ResponseCode
        {
            get;
            private set;
        }
        public string ResponseText
        {
            get;
            private set;
        }
        public string ErrorMessage
        {
            get;
            private set;
        }

        internal InternalWebRequestStateChangedEventArgs(
            string requestID,
            bool succeeded,
            long responseCode,
            string responseText,
            string errorMessage)
        {
            RequestID = requestID;
            Succeeded = succeeded;
            ResponseCode = responseCode;
            ResponseText = responseText;
            ErrorMessage = errorMessage;
        }
    }

    internal void StartTimer(InteractiveTimerType type, float interval)
    {
        _queuedStartTimerRequests.Add(new InteractiveTimerData(type, interval));
    }

    internal void StopTimer(InteractiveTimerType type)
    {
        _queuedStopTimerRequests.Add(type);
    }

    internal enum InteractiveTimerType
    {
        CheckAuthStatus,
        RefreshShortCode,
        Reconnect
    }

    internal struct InteractiveTimerData
    {
        public InteractiveTimerType type;
        public float interval;
        public InteractiveTimerData(
            InteractiveTimerType newType,
            float newInterval
            )
        {
            type = newType;
            interval = newInterval;
        }
    }

    internal class InternalTimerCallbackEventArgs
    {
    }

    internal void StartTryGetAuthTokensFromCache()
    {
        _queuedTryGetAuthTokensFromCacheRequest = true;
    }

    internal class TryGetAuthTokensFromCacheEventArgs
    {
        public string AuthToken;
        public string RefreshToken;
    }

    internal void WriteAuthTokensToCache(string authToken, string refreshToken)
    {
        _queuedWriteAuthTokensToCacheRequest = true;
        _authTokenValueToWriteToCache = authToken;
        _refreshTokenValueToWriteToCache = refreshToken;
    }

    private void WriteAuthTokensToCacheImpl()
    {
        _queuedWriteAuthTokensToCacheRequest = false;
        PlayerPrefs.SetString("MixerInteractive-AuthToken", _authTokenValueToWriteToCache);
        PlayerPrefs.SetString("MixerInteractive-RefreshToken", _refreshTokenValueToWriteToCache);
        PlayerPrefs.Save();
    }

    private IEnumerator MakeWebRequestCoRoutine(
        string requestID,
        string requestUrl, 
        Dictionary<string, string> headers,
        string httpVerb,
        string postData)
    {
        UnityWebRequest request;
        if (httpVerb == "POST")
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(postData);
            UploadHandlerRaw uH = new UploadHandlerRaw(bytes);
            request = UnityWebRequest.Post(requestUrl, postData);
            request.uploadHandler = uH;
            // We need to manually set this header otherwise Unity will encode the post data incorrectly.
            request.SetRequestHeader("Content-Type", "application/json");
        }
        else
        {
            request = UnityWebRequest.Get(requestUrl);
        }
        if (headers != null)
        {
            var headersKeys = headers.Keys;
            foreach (string headerKey in headersKeys)
            {
                request.SetRequestHeader(headerKey, headers[headerKey]);
            }
        }
        yield return request.SendWebRequest();
        // We need to send raise the event on another thread, otherwise there 
        // will be frame drops.
        BackgroundWorker backgroundWorker = new BackgroundWorker();
        backgroundWorker.DoWork -= WebRequestBackgroundWorkerDoWork;
        backgroundWorker.DoWork += WebRequestBackgroundWorkerDoWork;
        backgroundWorker.RunWorkerAsync(new InternalWebRequestStateChangedEventArgs(
                requestID,
                !request.isNetworkError,
                request.responseCode,
                request.downloadHandler.text,
                request.error
            ));
        request.Dispose();
    }

    private void WebRequestBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
    {
        BackgroundWorker backgroundWorker = (sender as BackgroundWorker);
        if (backgroundWorker != null)
        {
            backgroundWorker.DoWork -= WebRequestBackgroundWorkerDoWork;
            InternalWebRequestStateChangedEventArgs eventArgs = e.Argument as InternalWebRequestStateChangedEventArgs;
            if (eventArgs != null &&
                OnInternalWebRequestStateChanged != null)
            {
                OnInternalWebRequestStateChanged(this, eventArgs);
            }
        }
    }

    private IEnumerator CheckAuthStatusCoRoutine(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            // We need to send raise the event on another thread, otherwise there 
            // will be frame drops.
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork -= CheckAuthStatusBackgroundWorkerDoWork;
            backgroundWorker.DoWork += CheckAuthStatusBackgroundWorkerDoWork;
            backgroundWorker.RunWorkerAsync();
        }
    }

    private IEnumerator RefreshShortCodeCoRoutine(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            // We need to send raise the event on another thread, otherwise there 
            // will be frame drops.
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork -= RefreshShortCodeBackgroundWorkerDoWork;
            backgroundWorker.DoWork += RefreshShortCodeBackgroundWorkerDoWork;
            backgroundWorker.RunWorkerAsync();
        }
    }

    private IEnumerator ReconnectCodeCoRoutine(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            // We need to send raise the event on another thread, otherwise there 
            // will be frame drops.
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork -= OnInternalReconnectBackgroundWorkerDoWork;
            backgroundWorker.DoWork += OnInternalReconnectBackgroundWorkerDoWork;
            backgroundWorker.RunWorkerAsync();
        }
    }

    private void CheckAuthStatusBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
    {
        BackgroundWorker backgroundWorker = (sender as BackgroundWorker);
        if (backgroundWorker != null)
        {
            backgroundWorker.DoWork -= CheckAuthStatusBackgroundWorkerDoWork;
            if (OnInternalCheckAuthStatusTimerCallback != null)
            {
                OnInternalCheckAuthStatusTimerCallback(this, new InternalTimerCallbackEventArgs());
            }
        }
    }

    private void RefreshShortCodeBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
    {
        BackgroundWorker backgroundWorker = (sender as BackgroundWorker);
        if (backgroundWorker != null)
        {
            backgroundWorker.DoWork -= RefreshShortCodeBackgroundWorkerDoWork;
            if (OnInternalRefreshShortCodeTimerCallback != null)
            {
                OnInternalRefreshShortCodeTimerCallback(this, new InternalTimerCallbackEventArgs());
            }
        }
    }

    private void OnInternalReconnectBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
    {
        BackgroundWorker backgroundWorker = (sender as BackgroundWorker);
        if (backgroundWorker != null)
        {
            backgroundWorker.DoWork -= OnInternalReconnectBackgroundWorkerDoWork;
            if (OnInternalReconnectTimerCallback != null)
            {
                OnInternalReconnectTimerCallback(this, new InternalTimerCallbackEventArgs());
            }
        }
    }

    public void Dispose()
    {
        StopAllCoroutines();
    }

    private struct CoRoutineInfo
    {
        public string name;
        public Coroutine coRoutine;
        public CoRoutineInfo(string newName, Coroutine newCoRoutine)
        {
            name = newName;
            coRoutine = newCoRoutine;
        }
    }
}