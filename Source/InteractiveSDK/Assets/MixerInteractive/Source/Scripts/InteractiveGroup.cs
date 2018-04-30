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
    /// Represents group of participants. Groups can be used to segment
    /// the audience, with each group having a scene displayed.
    /// Each group can have an arbitrary number of participants assigned.
    /// Each group can have exactly one scene assigned. A given scene can be
    /// assigned to any number of groups. A participant can be in at most
    /// one group. There will always be a default group, if no groups are
    /// specified.
    /// </summary>
#if !WINDOWS_UWP
    [System.Serializable]
#endif
    public class InteractiveGroup
    {
        /// <summary>
        /// Unique string identifier for this group.
        /// </summary>
        public string GroupID
        {
            get;
            internal set;
        }

        /// <summary>
        /// Returns a shared pointer to the scene assigned to this group.
        /// </summary>
        public List<InteractiveParticipant> Participants
        {
            get
            {
                List<InteractiveParticipant> participantsInGroup = new List<InteractiveParticipant>();
                List<InteractiveParticipant> allParticipants = InteractivityManager.SingletonInstance.Participants as List<InteractiveParticipant>;
                foreach (InteractiveParticipant participant in allParticipants)
                {
                    if (participant._groupID == GroupID)
                    {
                        participantsInGroup.Add(participant);
                    }
                }
                return participantsInGroup;
            }
        }

        /// <summary>
        /// Returns a shared pointer to the scene assigned to this group.
        /// </summary>
        public string SceneID
        {
            get;
            internal set;
        }

        /// <summary>
        /// Sets the scene assigned for this group.
        /// </summary>
        public void SetScene(string sceneID)
        {
            SceneID = sceneID;
            InteractivityManager.SingletonInstance._SetCurrentSceneInternal(this, sceneID);
        }

        internal string _etag;

        /// <summary>
        /// Function to construct a InteractiveGroup object.
        /// </summary>
        public InteractiveGroup(string groupID)
        {
            if (InteractivityManager.SingletonInstance.InteractivityState != InteractivityState.InteractivityEnabled &&
                InteractivityManager.SingletonInstance.InteractivityState != InteractivityState.Initialized)
            {
                throw new Exception("Error: The InteractivityManager must be initialized and connected to the service to create new groups.");
            }

            GroupID = groupID;
            InteractivityManager.SingletonInstance._SendCreateGroupsMessage(GroupID, InteractivityManager._WS_MESSAGE_VALUE_DEFAULT_SCENE_ID);
        }

        /// <summary>
        /// Function to construct a InteractiveGroup object.
        /// </summary>
        public InteractiveGroup(string groupID, string sceneID)
        {
            if (InteractivityManager.SingletonInstance.InteractivityState != InteractivityState.InteractivityEnabled &&
                InteractivityManager.SingletonInstance.InteractivityState != InteractivityState.Initialized)
            {
                throw new Exception("Error: The InteractivityManager must be initialized and connected to the service to create new groups.");
            }

            GroupID = groupID;
            SceneID = sceneID;
            InteractivityManager.SingletonInstance._SendCreateGroupsMessage(GroupID, SceneID);
        }

        internal InteractiveGroup(string newEtag, string sceneID, string groupID)
        {
            _etag = newEtag;
            SceneID = sceneID;
            GroupID = groupID;
        }
    }
}
