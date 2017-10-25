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
                    if (participant.groupID == GroupID)
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
            InteractivityManager.SingletonInstance.SetCurrentSceneInternal(this, sceneID);
        }

        internal string etag;

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
            InteractivityManager.SingletonInstance.SendCreateGroupsMessage(GroupID, InteractivityManager.WS_MESSAGE_VALUE_DEFAULT_SCENE_ID);
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
            InteractivityManager.SingletonInstance.SendCreateGroupsMessage(GroupID, SceneID);
        }

        internal InteractiveGroup(string newEtag, string sceneID, string groupID)
        {
            etag = newEtag;
            SceneID = sceneID;
            GroupID = groupID;
        }
    }
}
