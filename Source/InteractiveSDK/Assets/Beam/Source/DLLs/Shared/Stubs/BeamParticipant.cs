#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN && !UNITY_WSA_10_0 && !UNITY_XBOXONE
using System;
using System.Collections.Generic;

namespace Xbox.Services.Beam
{
    public class BeamParticipant
    {
        public uint BeamLevel
        {
            get;
            private set;
        }

        public uint BeamID
        {
            get;
            private set;
        }

        public string BeamUserName
        {
            get;
            private set;
        }

        public DateTime ConnectedAt
        {
            get;
            private set;
        }

        public BeamGroup Group
        {
            get
            {
                return new BeamGroup("default");
            }
            set
            {
                Group = value;
            }
        }

        public DateTime LastInputAt
        {
            get;
            internal set;
        }

        public bool InputDisabled
        {
            get;
            private set;
        }

        public IList<BeamButtonControl> Buttons
        {
            get
            {
                return new List<BeamButtonControl>();
            }
        }

        public IList<BeamJoystickControl> Joysticks
        {
            get
            {
                return new List<BeamJoystickControl>();
            }
        }

        public BeamParticipantState State
        {
            get;
            internal set;
        }
    }
}
#endif