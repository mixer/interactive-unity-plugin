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
        /// Set to true if this user is the broadcaster.
        /// </summary>
        public bool IsBroadcaster
        {
            get
            {
                if (channelGroups.Contains("Owner"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
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
        private List<string> channelGroups;

        internal InteractiveParticipant(string newSessionID, string newEtag, uint userID, string newGroupID, string userName, List<string> newChannelGroups, uint level, DateTime lastInputAt, DateTime connectedAt, bool inputDisabled, InteractiveParticipantState state)
        {
            sessionID = newSessionID;
            UserID = userID;
            UserName = userName;
            channelGroups = newChannelGroups;
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