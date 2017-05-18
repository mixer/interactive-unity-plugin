#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN && !UNITY_WSA_10_0 && !UNITY_XBOXONE
using System;
using System.Collections.Generic;

namespace Xbox.Services.Beam
{
    public partial class BeamManager : IDisposable
    {
        public delegate void OnErrorEventHandler(object sender, BeamEventArgs e);
        public event OnErrorEventHandler OnError;

        public delegate void OnInteractivityStateChangedHandler(object sender, BeamInteractivityStateChangedEventArgs e);
        public event OnInteractivityStateChangedHandler OnInteractivityStateChanged;

        public delegate void OnParticipantStateChangedHandler(object sender, BeamParticipantStateChangedEventArgs e);
        public event OnParticipantStateChangedHandler OnParticipantStateChanged;

        public delegate void OnBeamButtonEventHandler(object sender, BeamButtonEventArgs e);
        public event OnBeamButtonEventHandler OnBeamButtonEvent;

        public delegate void OnBeamJoystickControlEventHandler(object sender, BeamJoystickEventArgs e);
        public event OnBeamJoystickControlEventHandler OnBeamJoystickControlEvent;

        private static BeamManager _singletonInstance;

        public static BeamManager SingletonInstance
        {
            get
            {
                if (_singletonInstance == null)
                {
                    _singletonInstance = new BeamManager();
                }
                return _singletonInstance;
            }
        }

        public BeamLoggingLevel LoggingLevel
        {
            get;
            set;
        }

        public string ProjectVersionID
        {
            get;
            private set;
        }

        public string AppID
        {
            get;
            private set;
        }

        public BeamInteractivityState InteractivityState
        {
            get;
            private set;
        }

        public IList<BeamGroup> Groups
        {
            get;
            private set;
        }

        public IList<BeamScene> Scenes
        {
            get;
            private set;
        }

        public IList<BeamParticipant> Participants
        {
            get;
            private set;
        }

        public IList<BeamControl> Controls
        {
            get;
            private set;
        }

        public IList<BeamButtonControl> Buttons
        {
            get;
            private set;
        }

        public IList<BeamJoystickControl> Joysticks
        {
            get;
            private set;
        }

        public string ShortCode
        {
            get;
            private set;
        }

        public BeamScene GetScene(string sceneID)
        {
            return null;
        }

        public BeamGroup GetGroup(string groupID)
        {
            return null;
        }

        public void Initialize(bool goInteractive = true)
        {
        }

        public void TriggerCooldown(string controlID, int cooldown)
        {
        }

        public void StartInteractive()
        {
        }

        public void StopInteractive()
        {
        }

        public void DoWork()
        {
        }

        public void Dispose()
        {
        }

        public void SendMockWebSocketMessage(string rawText)
        {
        }

        public BeamButtonControl GetButton(string controlID)
        {
            return new BeamButtonControl(controlID, false, string.Empty, string.Empty, string.Empty);
        }

        public BeamJoystickControl GetJoystick(string controlID)
        {
            return new BeamJoystickControl(controlID, true, "", "", "");
        }

        public string GetCurrentScene()
        {
            return string.Empty;
        }

        public void SetCurrentScene(string sceneID)
        {
        }

        // For MockData
        public static bool useMockData = false;
    }
}
#endif