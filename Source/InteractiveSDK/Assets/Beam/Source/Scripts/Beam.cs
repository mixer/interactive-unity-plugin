using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Security;
using UnityEngine;
using Xbox.Services.Beam;
#if WINDOWS_UWP
using System;
using Windows.Security.Credentials;
using Windows.Security.Authentication.Web.Core;
using System.Threading.Tasks;
#endif

namespace Microsoft
{
    public class Beam : MonoBehaviour
    {
        public bool runInBackground = true;
        public string defaultSceneID;

        // Custom Unity Inspectors are not great at displaying complex objects
        // so we'll store these as seperate variables instead of a List.
        public string group1ID;
        public string group2ID;
        public string group3ID;
        public string group4ID;
        public string group5ID;
        public string group6ID;
        public string group7ID;
        public string group8ID;
        public string group9ID;
        public string group10ID;
        public string group1SceneID;
        public string group2SceneID;
        public string group3SceneID;
        public string group4SceneID;
        public string group5SceneID;
        public string group6SceneID;
        public string group7SceneID;
        public string group8SceneID;
        public string group9SceneID;
        public string group10SceneID;

        // Events
        public delegate void OnErrorEventHandler(object sender, BeamEventArgs e);
        public static event OnErrorEventHandler OnError;

        public delegate void OnGoInteractiveHandler(object sender, BeamEventArgs e);
        public static event OnGoInteractiveHandler OnGoInteractive;

        public delegate void OnInteractivityStateChangedHandler(object sender, BeamInteractivityStateChangedEventArgs e);
        public static event OnInteractivityStateChangedHandler OnInteractivityStateChanged;

        public delegate void OnParticipantStateChangedHandler(object sender, BeamParticipantStateChangedEventArgs e);
        public static event OnParticipantStateChangedHandler OnParticipantStateChanged;

        public delegate void OnBeamButtonEventHandler(object sender, BeamButtonEventArgs e);
        public static event OnBeamButtonEventHandler OnBeamButtonEvent;

        public delegate void OnBeamJoystickControlEventHandler(object sender, BeamJoystickEventArgs e);
        public static event OnBeamJoystickControlEventHandler OnBeamJoystickControlEvent;

        private static BeamManager beamManager;
        private static List<BeamEventArgs> queuedEvents;
        private static bool previousRunInBackgroundValue;
        private static BeamDialog beamDialog;
        private static bool pendingGoInteractive;
        private static string outstandingSetDefaultSceneRequest;
        private static List<string> outstandingCreateGroupsRequests;
        private static bool outstandingRequestsCompleted;
        private static float lastCheckForOutstandingRequestsTime;
        private static bool processedSerializedProperties;
        private static bool hasFiredGoInteractiveEvent;
        private static bool shouldCheckForOutstandingRequests;

#if !WINDOWS_UWP
        private static BackgroundWorker backgroundWorker;
#endif

        private const string DEFAULT_GROUP_ID = "default";
        private const float CHECK_FOR_OUTSTANDING_REQUESTS_INTERVAL = 1f;

        // Use this for initialization
        void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (beamDialog == null)
            {
                beamDialog = FindObjectOfType<BeamDialog>();
            }
            if (queuedEvents == null)
            {
                queuedEvents = new List<BeamEventArgs>();
            }
            // Listen for Beam events
            bool beamManagerAlreadyInitialized = false;
            if (beamManager == null)
            {
                beamManager = BeamManager.SingletonInstance;

                beamManager.OnError -= HandleError;
                beamManager.OnInteractivityStateChanged -= HandleInteractivityStateChanged;
                beamManager.OnParticipantStateChanged -= HandleParticipantStateChanged;
                beamManager.OnBeamButtonEvent -= HandleBeamButtonEvent;
                beamManager.OnBeamJoystickControlEvent -= HandleBeamJoystickControlEvent;

                beamManager.OnError += HandleError;
                beamManager.OnInteractivityStateChanged += HandleInteractivityStateChanged;
                beamManager.OnParticipantStateChanged += HandleParticipantStateChanged;
                beamManager.OnBeamButtonEvent += HandleBeamButtonEvent;
                beamManager.OnBeamJoystickControlEvent += HandleBeamJoystickControlEvent;
            }
            else
            {
                beamManagerAlreadyInitialized = true;
            }
            BeamHelper helper = BeamHelper.SingletonInstance;
            helper.runInBackgroundIfInteractive = runInBackground;
            helper.defaultSceneID = defaultSceneID;
            if (group1ID != string.Empty &&
                !helper.groupSceneMapping.ContainsKey(group1ID))
            {
                helper.groupSceneMapping.Add(group1ID, group1SceneID);
            }
            if (group2ID != string.Empty &&
                !helper.groupSceneMapping.ContainsKey(group2ID))
            {
                helper.groupSceneMapping.Add(group2ID, group2SceneID);
            }
            if (group3ID != string.Empty &&
                !helper.groupSceneMapping.ContainsKey(group3ID))
            {
                helper.groupSceneMapping.Add(group3ID, group3SceneID);
            }
            if (group4ID != string.Empty &&
                !helper.groupSceneMapping.ContainsKey(group4ID))
            {
                helper.groupSceneMapping.Add(group4ID, group4SceneID);
            }
            if (group5ID != string.Empty &&
                !helper.groupSceneMapping.ContainsKey(group5ID))
            {
                helper.groupSceneMapping.Add(group5ID, group5SceneID);
            }
            if (group6ID != string.Empty &&
                !helper.groupSceneMapping.ContainsKey(group6ID))
            {
                helper.groupSceneMapping.Add(group6ID, group6SceneID);
            }
            if (group7ID != string.Empty &&
                !helper.groupSceneMapping.ContainsKey(group7ID))
            {
                helper.groupSceneMapping.Add(group7ID, group7SceneID);
            }
            if (group8ID != string.Empty &&
                !helper.groupSceneMapping.ContainsKey(group8ID))
            {
                helper.groupSceneMapping.Add(group8ID, group8SceneID);
            }
            if (group9ID != string.Empty &&
                !helper.groupSceneMapping.ContainsKey(group9ID))
            {
                helper.groupSceneMapping.Add(group9ID, group9SceneID);
            }
            if (group10ID != string.Empty &&
                !helper.groupSceneMapping.ContainsKey(group10ID))
            {
                helper.groupSceneMapping.Add(group10ID, group10SceneID);
            }
            if (outstandingCreateGroupsRequests == null)
            {
                outstandingCreateGroupsRequests = new List<string>();
            }
            outstandingSetDefaultSceneRequest = string.Empty;
            processedSerializedProperties = false;
            outstandingRequestsCompleted = false;
            shouldCheckForOutstandingRequests = false;
            lastCheckForOutstandingRequestsTime = -1;
#if !WINDOWS_UWP
            backgroundWorker = new BackgroundWorker();
#endif
            if (beamManagerAlreadyInitialized &&
                BeamManager.SingletonInstance.InteractivityState == BeamInteractivityState.InteractivityEnabled)
            {
                ProcessSerializedProperties();
            }
        }

        private static void HandleBeamJoystickControlEvent(object sender, BeamJoystickEventArgs e)
        {
            queuedEvents.Add(e);
        }

        private static void HandleBeamButtonEvent(object sender, BeamButtonEventArgs e)
        {
            queuedEvents.Add(e);
        }

        private static void HandleParticipantStateChanged(object sender, BeamEventArgs e)
        {
            queuedEvents.Add(e);
        }

        private static void HandleInteractivityStateChanged(object sender, BeamInteractivityStateChangedEventArgs e)
        {
            queuedEvents.Add(e);
        }

        private static void HandleError(object sender, BeamEventArgs e)
        {
            queuedEvents.Add(e);
        }

        /// <summary>
        /// Can query the state of the beam manager.
        /// </summary>
        public static BeamInteractivityState InteractivityState
        {
            get
            {
                return BeamManager.SingletonInstance.InteractivityState;
            }
        }

        /// <summary>
        /// Gets all the groups associated with the current interactivity instance.
        /// Will be empty if initialization is not complete.
        /// </summary>
        public static IList<BeamGroup> Groups
        {
            get
            {
                return BeamManager.SingletonInstance.Groups;
            }
        }

        /// <summary>
        /// Gets all the scenes associated with the current interactivity instance.
        /// </summary>
        public static IList<BeamScene> Scenes
        {
            get
            {
                return BeamManager.SingletonInstance.Scenes;
            }
        }

        /// <summary>
        /// Returns all the participants.
        /// </summary>
        public static IList<BeamParticipant> Participants
        {
            get
            {
                return BeamManager.SingletonInstance.Participants;
            }
        }

        /// <summary>
        /// Retrieve a list of all of the button controls.
        /// </summary>
        public static IList<BeamButtonControl> Buttons
        {
            get
            {
                return BeamManager.SingletonInstance.Buttons;
            }
        }

        /// <summary>
        /// Retrieve a list of all of the joystick controls.
        /// </summary>
        public static IList<BeamJoystickControl> Joysticks
        {
            get
            {
                return BeamManager.SingletonInstance.Joysticks;
            }
        }

        /// <summary>
        /// The string the broadcaster needs to enter in the beam website to
        /// authorize the interactive session.
        /// </summary>
        public static string ShortCode
        {
            get
            {
                return BeamManager.SingletonInstance.ShortCode;
            }
        }

        /// <summary>
        /// Kicks off a background task to set up the connection to the Beam Interactivity service.
        /// </summary>
        /// <returns>true if initialization request was accepted, false if not</returns>
        /// <param name="goInteractive"> If true, initializes and enters interactivity. Defaults to true</param>
        /// <remarks></remarks>
        public static void Initialize(bool goInteractive = true)
        {
            BeamManager.SingletonInstance.Initialize(goInteractive);
        }

        /// <summary>
        /// Trigger a cooldown, disabling the specified control for a period of time.
        /// </summary>
        /// <param name="controlID">String ID of the control to disable.</param>
        /// <param name="cooldown">Duration (in milliseconds) required between triggers.</param>
        public static void TriggerCooldown(string controlID, int cooldown)
        {
            BeamManager.SingletonInstance.TriggerCooldown(controlID, cooldown);
        }

        /// <summary>
        /// Used by the title to inform the Beam service that it is ready to recieve interactive input.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static void StartInteractive()
        {
            BeamManager.SingletonInstance.StartInteractive();
        }

        /// <summary>
        /// Used by the title to inform the Beam service that it is no longer receiving interactive input.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static void StopInteractive()
        {
            BeamManager.SingletonInstance.StopInteractive();
            pendingGoInteractive = false;
            if (BeamHelper.SingletonInstance.runInBackgroundIfInteractive)
            {
                Application.runInBackground = previousRunInBackgroundValue;
            }
        }

        /// <summary>
        /// Manages and maintains proper state updates between the title and the Beam Service.
        /// To ensure best performance, DoWork() must be called frequently, such as once per frame.
        /// Title needs to be thread safe when calling DoWork() since this is when states are changed.
        /// </summary>
        public static void DoWork()
        {
            BeamManager.SingletonInstance.DoWork();
        }

        /// <summary>
        /// Frees resources used by the BeamManager.
        /// </summary>
        public static void Dispose()
        {
            BeamManager beamManager = BeamManager.SingletonInstance;
            if (beamManager != null)
            {
                beamManager.OnInteractivityStateChanged -= HandleInteractivityStateChangedInternal;

#if !WINDOWS_UWP
                // Run initialization in another thread.
                backgroundWorker.DoWork -= BackgroundWorkerDoWork;
#endif
            }
            if (queuedEvents != null)
            {
                queuedEvents.Clear();
            }
            previousRunInBackgroundValue = true;
            pendingGoInteractive = false;
            outstandingSetDefaultSceneRequest = string.Empty;
            if (outstandingCreateGroupsRequests != null)
            {
                outstandingCreateGroupsRequests.Clear();
            }
            outstandingRequestsCompleted = false;
            lastCheckForOutstandingRequestsTime = -1;
            processedSerializedProperties = false;
            hasFiredGoInteractiveEvent = false;
            BeamManager.SingletonInstance.Dispose();
        }

        private void ResetInternalState()
        {
            previousRunInBackgroundValue = true;
            outstandingSetDefaultSceneRequest = string.Empty;
            if (outstandingCreateGroupsRequests != null)
            {
                outstandingCreateGroupsRequests.Clear();
            }
            outstandingRequestsCompleted = false;
            lastCheckForOutstandingRequestsTime = -1;
            processedSerializedProperties = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (processedSerializedProperties &&
                shouldCheckForOutstandingRequests &&
                !outstandingRequestsCompleted &&
                Time.time - lastCheckForOutstandingRequestsTime > CHECK_FOR_OUTSTANDING_REQUESTS_INTERVAL)
            {
                lastCheckForOutstandingRequestsTime = Time.time;
                outstandingRequestsCompleted = CheckForOutStandingRequestsCompleted();
            }

            BeamManager.SingletonInstance.DoWork();

            List<BeamEventArgs> processedEvents = new List<BeamEventArgs>();
            if (queuedEvents != null)
            {
                // Raise events
                foreach (BeamEventArgs beamEvent in queuedEvents)
                {
                    if (beamEvent == null)
                    {
                        continue;
                    }
                    switch (beamEvent.EventType)
                    {
                        case BeamEventType.InteractivityStateChanged:
                            BeamInteractivityStateChangedEventArgs interactivityStateChangedArgs = beamEvent as BeamInteractivityStateChangedEventArgs;
                            if (interactivityStateChangedArgs.State == BeamInteractivityState.InteractivityEnabled &&
                                (!shouldCheckForOutstandingRequests || outstandingRequestsCompleted) &&
                                !hasFiredGoInteractiveEvent)
                            {
                                if (OnGoInteractive != null)
                                {
                                    hasFiredGoInteractiveEvent = true;
                                    OnGoInteractive(this, interactivityStateChangedArgs);
                                }
                            }
                            if (OnInteractivityStateChanged != null)
                            {
                                OnInteractivityStateChanged(this, interactivityStateChangedArgs);
                            }
                            processedEvents.Add(beamEvent);
                            break;
                        case BeamEventType.ParticipantStateChanged:
                            if (outstandingRequestsCompleted)
                            {
                                if (OnParticipantStateChanged != null)
                                {
                                    OnParticipantStateChanged(this, beamEvent as BeamParticipantStateChangedEventArgs);
                                }
                                processedEvents.Add(beamEvent);
                            }
                            break;
                        case BeamEventType.Button:
                            if (OnBeamButtonEvent != null)
                            {
                                OnBeamButtonEvent(this, beamEvent as BeamButtonEventArgs);
                            }
                            processedEvents.Add(beamEvent);
                            break;
                        case BeamEventType.Joystick:
                            if (OnBeamJoystickControlEvent != null)
                            {
                                OnBeamJoystickControlEvent(this, beamEvent as BeamJoystickEventArgs);
                            }
                            processedEvents.Add(beamEvent);
                            break;
                        case BeamEventType.Error:
                            if (OnError != null)
                            {
                                OnError(this, beamEvent as BeamEventArgs);
                            }
                            processedEvents.Add(beamEvent);
                            break;
                        default:
                            // Throw exception for unexpected event type.
                            break;
                    }
                }
                foreach (BeamEventArgs eventArgs in processedEvents)
                {
                    queuedEvents.Remove(eventArgs);
                }
            }
            if (BeamManager.SingletonInstance.InteractivityState == BeamInteractivityState.InteractivityEnabled &&
                shouldCheckForOutstandingRequests &&
                outstandingRequestsCompleted &&
                !hasFiredGoInteractiveEvent)
            {
                if (OnGoInteractive != null)
                {
                    hasFiredGoInteractiveEvent = true;
                    OnGoInteractive(this, new BeamEventArgs());
                }
            }
        }

        /// <summary>
        /// Returns whether the button with the given control ID is currently down.
        /// </summary>
        /// <param name="controlID">String ID of the control.</param>
        public static bool GetButtonDown(string controlID)
        {
            return BeamManager.SingletonInstance.GetButton(controlID).ButtonDown;
        }

        /// <summary>
        /// Returns whether the button with the given control ID is currently pressed.
        /// </summary>
        /// <param name="controlID">String ID of the control.</param>
        public static bool GetButton(string controlID)
        {
            return BeamManager.SingletonInstance.GetButton(controlID).ButtonPressed;
        }

        /// <summary>
        /// Returns whether the button with the given control ID is currently up.
        /// </summary>
        /// <param name="controlID">String ID of the control.</param>
        public static bool GetButtonUp(string controlID)
        {
            return BeamManager.SingletonInstance.GetButton(controlID).ButtonUp;
        }

        /// <summary>
        /// Returns how many buttons with the given control ID are pressed down.
        /// </summary>
        /// <param name="controlID">String ID of the control.</param>
        public static uint GetCountOfButtonDowns(string controlID)
        {
            return BeamManager.SingletonInstance.GetButton(controlID).CountOfButtonDowns;
        }

        /// <summary>
        /// Returns how many buttons with the given control ID are pressed.
        /// </summary>
        /// <param name="controlID">String ID of the control.</param>
        public static uint GetCountOfButtons(string controlID)
        {
            return BeamManager.SingletonInstance.GetButton(controlID).CountOfButtonPresses;
        }

        /// <summary>
        /// Returns how many buttons with the given control ID are up.
        /// </summary>
        /// <param name="controlID">String ID of the control.</param>
        public static uint GetCountOfButtonUps(string controlID)
        {
            return BeamManager.SingletonInstance.GetButton(controlID).CountOfButtonUps;
        }

        /// <summary>
        /// Returns the joystick with the given control ID.
        /// </summary>
        /// <param name="controlID">String ID of the control.</param>
        public static BeamJoystickControl GetJoystick(string controlID)
        {
            return BeamManager.SingletonInstance.GetJoystick(controlID);
        }

        /// <summary>
        /// Returns the X coordinate of the joystick with the given control ID.
        /// </summary>
        /// <param name="controlID">String ID of the control.</param>
        public static float GetJoystickX(string controlID)
        {
            return (float)BeamManager.SingletonInstance.GetJoystick(controlID).X;
        }

        /// <summary>
        /// Returns the Y coordinate of the joystick with the given control ID.
        /// </summary>
        /// <param name="controlID">String ID of the control.</param>
        public static float GetJoystickY(string controlID)
        {
            return (float)BeamManager.SingletonInstance.GetJoystick(controlID).Y;
        }

        /// <summary>
        /// Gets a button control object by ID.
        /// </summary>
        /// <param name="controlID">The ID of the control.</param>
        /// <returns></returns>
        public static BeamButtonControl Button(string controlID)
        {
            return BeamManager.SingletonInstance.GetButton(controlID);
        }

        /// <summary>
        /// Gets the current scene for the default group.
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentScene()
        {
            return BeamManager.SingletonInstance.GetCurrentScene();
        }

        /// <summary>
        /// Sets the current scene for the default group.
        /// </summary>
        /// <param name="sceneID">The ID of the scene to change to.</param>
        public static void SetCurrentScene(string sceneID)
        {
            BeamManager.SingletonInstance.SetCurrentScene(sceneID);
        }

        /// <summary>
        /// Returns the specified group. Will return null if initialization
        /// is not yet complete or group does not exist.
        /// </summary>
        /// <param name="groupID">The ID of the group.</param>
        /// <returns></returns>
        public static BeamGroup GetGroup(string groupID)
        {
            return BeamManager.SingletonInstance.GetGroup(groupID);
        }

        /// <summary>
        /// Returns the specified scene. Will return nullptr if initialization
        /// is not yet complete or scene does not exist.
        /// </summary>
        public static BeamScene GetScene(string sceneID)
        {
            return BeamManager.SingletonInstance.GetScene(sceneID);
        }

        /// <summary>
        /// Connects to the Beam service and signals the service that the BeamManager is ready to recieve messages.
        /// It also, handles signals authentication events if necessary.
        /// </summary>
        public static void GoInteractive()
        {
            if (pendingGoInteractive)
            {
                return;
            }
            pendingGoInteractive = true;
            // We fire the OnGoInteractive event again even if we are already interactive, because
            // it could have been a scene change and the developer has updated group or scene data
            // in the BeamManager prefab.
            hasFiredGoInteractiveEvent = false;
            var beamManager = BeamManager.SingletonInstance;
            beamManager.OnInteractivityStateChanged -= HandleInteractivityStateChangedInternal;
            beamManager.OnInteractivityStateChanged += HandleInteractivityStateChangedInternal;

#if !WINDOWS_UWP
            // Run initialization in another thread.
            // Workaround - in certain cases Unity does not call the Start function, which means
            // initialization does not happen. We need to check if the background worker hasn't
            // been initialized and if not, initialize it.
            if (backgroundWorker == null)
            {
                backgroundWorker = new BackgroundWorker();
            }
            backgroundWorker.DoWork -= BackgroundWorkerDoWork;
            backgroundWorker.DoWork += BackgroundWorkerDoWork;
            backgroundWorker.RunWorkerAsync();
#else
        InitializeAsync();
#endif

            if (BeamHelper.SingletonInstance.runInBackgroundIfInteractive)
            {
                previousRunInBackgroundValue = Application.runInBackground;
                Application.runInBackground = true;
            }
        }

#if WINDOWS_UWP
    private static async void InitializeAsync()
    {
        await Task.Run(() => {
            BeamManager.SingletonInstance.Initialize(true);
        });
    }
#endif

        private static void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            BeamManager.SingletonInstance.Initialize();
        }

        private static void HandleInteractivityStateChangedInternal(object sender, BeamInteractivityStateChangedEventArgs e)
        {
            var state = e.State;
            switch (state)
            {
                case BeamInteractivityState.ShortCodeRequired:
                    beamDialog.Show(BeamManager.SingletonInstance.ShortCode);
                    break;
                case BeamInteractivityState.InteractivityEnabled:
                    beamDialog.Hide();
                    ProcessSerializedProperties();
                    pendingGoInteractive = false;
                    break;
                default:
                    break;
            }
        }

        private static void ProcessSerializedProperties()
        {
            BeamHelper helper = BeamHelper.SingletonInstance;
            BeamManager beamManager = BeamManager.SingletonInstance;
            string defaultSceneID = helper.defaultSceneID;
            if (helper.groupSceneMapping.Count > 0 ||
                defaultSceneID != string.Empty)
            {
                shouldCheckForOutstandingRequests = true;
            }
            if (helper.groupSceneMapping.Count > 0)
            {
                var groupIDs = helper.groupSceneMapping.Keys;
                foreach (var groupID in groupIDs)
                {
                    // Supress this warning because calling the contructor
                    // triggers the creation of a group.
#pragma warning disable 0219
                    BeamGroup group;
#pragma warning restore 0219
                    string sceneID = helper.groupSceneMapping[groupID];
                    if (sceneID != string.Empty)
                    {
                        group = new BeamGroup(groupID, sceneID);
                    }
                    else
                    {
                        group = new BeamGroup(groupID);
                    }
                    outstandingCreateGroupsRequests.Add(groupID);
                }
                if (defaultSceneID != string.Empty)
                {
                    beamManager.SetCurrentScene(defaultSceneID);
                    outstandingSetDefaultSceneRequest = defaultSceneID;
                }
            }
            processedSerializedProperties = true;
        }

        private static bool CheckForOutStandingRequestsCompleted()
        {
            bool outstandingRequestsCompleted = false;
            List<string> groupsToRemove = new List<string>();
            if (outstandingSetDefaultSceneRequest == string.Empty)
            {
                foreach (string groupID in outstandingCreateGroupsRequests)
                {
                    foreach (BeamGroup group in BeamManager.SingletonInstance.Groups)
                    {
                        if (group.GroupID == groupID)
                        {
                            groupsToRemove.Add(groupID);
                        }
                    }
                }
                foreach (string groupID in groupsToRemove)
                {
                    outstandingCreateGroupsRequests.Remove(groupID);
                }
            }
            else
            {
                foreach (BeamGroup group in BeamManager.SingletonInstance.Groups)
                {
                    if (group.GroupID == DEFAULT_GROUP_ID &&
                        group.SceneID == outstandingSetDefaultSceneRequest)
                    {
                        outstandingSetDefaultSceneRequest = string.Empty;
                        break;
                    }
                }
            }

            if (outstandingCreateGroupsRequests.Count == 0 &&
                outstandingSetDefaultSceneRequest == string.Empty)
            {
                outstandingRequestsCompleted = true;
            }
            return outstandingRequestsCompleted;
        }

#if WINDOWS_UWP
    private static async Task<string> GetXTokenAsync()
    {
        string token = string.Empty;
        // Get an XToken
        // Find the account provider using the signed in user.
        // We always use the 1st signed in user, because we just need a valid token. It doesn't
        // matter who's it is.
        Windows.System.User currentUser;
        WebTokenRequest request;
        var users = await Windows.System.User.FindAllAsync();
        currentUser = users[0];
        WebAccountProvider xboxProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://xsts.auth.xboxlive.com", "", currentUser);

        // Build the web token request using the account provider.
        // Url = URL of the service we are getting a token for - for example https://apis.mycompany.com/something. 
        // As this is a sample just use xboxlive.com
        // Target & Policy should always be set to "xboxlive.signin" and "DELEGATION"
        // For this call to succeed your console needs to be in the XDKS.1 sandbox
        request = new Windows.Security.Authentication.Web.Core.WebTokenRequest(xboxProvider);
        request.Properties.Add("Url", "https://xboxlive.com");
        request.Properties.Add("Target", "xboxlive.signin");
        request.Properties.Add("Policy", "DELEGATION");

        // Request a token - correct pattern is to call getTokenSilentlyAsync and if that 
        // fails with WebTokenRequestStatus.userInteractionRequired then call requestTokenAsync
        // to get the token and prompt the user if required.
        // getTokenSilentlyAsync can be called on a background thread.
        WebTokenRequestResult tokenResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request);
        //If we got back a token call our service with that token 
        if (tokenResult.ResponseStatus == WebTokenRequestStatus.Success)
        {
            token = tokenResult.ResponseData[0].Token;
        }
        else if (tokenResult.ResponseStatus == WebTokenRequestStatus.UserInteractionRequired)
        { // WebTokenRequestStatus.userInteractionRequired = 3
          // If user interaction is required then call requestTokenAsync instead - this will prompt for user permission if required
          // Note: RequestTokenAsync cannot be called on a background thread.
            WebTokenRequestResult tokenResult2 = await WebAuthenticationCoreManager.RequestTokenAsync(request);
            //If we got back a token call our service with that token 
            if (tokenResult2.ResponseStatus == WebTokenRequestStatus.Success)
            {
                token = tokenResult.ResponseData[0].Token;
            }
            else if (tokenResult2.ResponseStatus == WebTokenRequestStatus.UserCancel)
            { 
                // No-op
            }
        }
        return token;
    }
#endif

        public struct BeamSettings
        {
            public bool runInBackgroundIfInteractive;
        }

        void OnDestroy()
        {
            ResetInternalState();
        }
    }
}