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
using System;
using System.Collections.Generic;

namespace Microsoft.Mixer
{
    /// <summary>
    /// Class representing a user currently viewing an interactive stream.
    /// </summary>
#if !WINDOWS_UWP
    [System.Serializable]
#endif
    public class InteractiveParticipant
    {
        /// <summary>
        /// The user's Mixer level.
        /// </summary>
        public uint Level
        {
            get;
            internal set;
        }

        /// <summary>
        /// The the Mixer user id associated with this participant.
        /// </summary>
        public uint UserID
        {
            get;
            internal set;
        }

        /// <summary>
        /// The user's name on Mixer.
        /// </summary>
        public string UserName
        {
            get;
            internal set;
        }

        /// <summary>
        /// UTC time at which this participant connected to this stream.
        /// </summary>
        public DateTime ConnectedAt
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the group that this participant is assigned to. This
        /// also updates the participant list of the referenced group.
        /// </summary>
        public InteractiveGroup Group
        {
            get
            {
                List<InteractiveGroup> allGroups = InteractivityManager.SingletonInstance.Groups as List<InteractiveGroup>;
                foreach (InteractiveGroup group in allGroups)
                {
                    if (group.GroupID == groupID)
                    {
                        return group;
                    }
                }
                return new InteractiveGroup("default");
            }
            set
            {
                if (value == null)
                {
                    InteractivityManager.SingletonInstance.LogError("Error: You cannot assign 'null' as the group value.");
                    return;
                }
                groupID = value.GroupID;
                InteractivityManager.SingletonInstance.SendUpdateParticipantsMessage(this);
            }
        }

        /// <summary>
        /// UTC time at which this participant last had interactive control input.
        /// </summary>
        public DateTime LastInputAt
        {
            get;
            internal set;
        }

        /// <summary>
        /// Set to true if the user's input has been disabled.
        /// </summary>
        public bool InputDisabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// Returns an array button objects this participant has interacted with.
        /// </summary>
        public IList<InteractiveButtonControl> Buttons
        {
            get
            {
                List<InteractiveButtonControl> buttonsForParticipant = new List<InteractiveButtonControl>();
                Dictionary<string, InternalButtonState> buttonState;
                bool participantEntryExists = InteractivityManager._buttonStatesByParticipant.TryGetValue(UserID, out buttonState);
                if (participantEntryExists)
                {
                    var buttonStatesByParticipantKeys = buttonState.Keys;
                    foreach (string key in buttonStatesByParticipantKeys)
                    {
                        var allButtons = InteractivityManager.SingletonInstance.Buttons;
                        foreach (InteractiveButtonControl button in allButtons)
                        {
                            if (key == button.ControlID)
                            {
                                buttonsForParticipant.Add(button);
                                break;
                            }
                        }
                    }
                }
                return buttonsForParticipant;
            }
        }

        /// <summary>
        /// Returns an array of control IDs of the joysticks this participant has interacted with.
        /// </summary>
        public IList<InteractiveJoystickControl> Joysticks
        {
            get
            {
                List<InteractiveJoystickControl> joysticksForParticipant = new List<InteractiveJoystickControl>();
                Dictionary<string, InternalJoystickState> joystickByParticipant;
                bool participantEntryExists = InteractivityManager._joystickStatesByParticipant.TryGetValue(UserID, out joystickByParticipant);
                if (participantEntryExists)
                {
                    var joystickStatesByParticipantKeys = joystickByParticipant.Keys;
                    foreach (string key in joystickStatesByParticipantKeys)
                    {
                        var allJoysticks = InteractivityManager.SingletonInstance.Joysticks;
                        foreach (InteractiveJoystickControl joystick in allJoysticks)
                        {
                            if (key == joystick.ControlID)
                            {
                                joysticksForParticipant.Add(joystick);
                                break;
                            }
                        }
                    }
                }
                return joysticksForParticipant;
            }
        }

        /// <summary>
        /// The participant's current state.
        /// </summary>
        public InteractiveParticipantState State
        {
            get;
            internal set;
        }

        internal string etag;
        internal string groupID;
        internal string sessionID;

        internal InteractiveParticipant(string newSessionID, string newEtag, uint userID, string newGroupID, string userName, uint level, DateTime lastInputAt, DateTime connectedAt, bool inputDisabled, InteractiveParticipantState state)
        {
            sessionID = newSessionID;
            UserID = userID;
            UserName = userName;
            Level = level;
            LastInputAt = lastInputAt;
            ConnectedAt = connectedAt;
            InputDisabled = inputDisabled;
            State = state;
            groupID = newGroupID;
            etag = newEtag;
        }
    }
}