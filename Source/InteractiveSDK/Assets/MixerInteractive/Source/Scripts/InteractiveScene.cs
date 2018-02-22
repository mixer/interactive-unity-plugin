using System.Collections.Generic;

namespace Microsoft.Mixer
{
    /// <summary>
    /// Represents an interactive scene. These scenes are configured using
    /// Interactive Studio.
    /// </summary>
#if !WINDOWS_UWP
    [System.Serializable]
#endif
    public class InteractiveScene
    {
        /// <summary>
        /// Unique string identifier for this scene
        /// </summary>
        public string SceneID
        {
            get;
            internal set;
        }

        /// <summary>
        /// Retrieve a List of all of the buttons in the scene. May be empty.
        /// </summary>
        public IList<InteractiveButtonControl> Buttons
        {
            get
            {
                List<InteractiveButtonControl> buttonsInScene = new List<InteractiveButtonControl>();
                List<InteractiveButtonControl> allButtons = InteractivityManager.SingletonInstance.Buttons as List<InteractiveButtonControl>;
                foreach (InteractiveButtonControl button in allButtons)
                {
                    if (button.SceneID == SceneID)
                    {
                        buttonsInScene.Add(button);
                    }
                }
                return buttonsInScene;
            }
        }

        /// <summary>
        /// Retrieve a List of all of the joysticks in the scene. May be empty.
        /// </summary>
        public IList<InteractiveJoystickControl> Joysticks
        {
            get
            {
                List<InteractiveJoystickControl> joysticksInScene = new List<InteractiveJoystickControl>();
                List<InteractiveJoystickControl> allJoysticks = InteractivityManager.SingletonInstance.Buttons as List<InteractiveJoystickControl>;
                foreach (InteractiveJoystickControl joystick in allJoysticks)
                {
                    if (joystick.SceneID == SceneID)
                    {
                        joysticksInScene.Add(joystick);
                    }
                }
                return joysticksInScene;
            }
        }

        /// <summary>
        /// Retrieve a list of IDs of all of the groups assigned to the scene. May be empty.
        /// </summary>
        public IList<InteractiveGroup> Groups
        {
            get
            {
                List<InteractiveGroup> groupsAssignedToScene = new List<InteractiveGroup>();
                List<InteractiveGroup> allGroups = InteractivityManager.SingletonInstance.Groups as List<InteractiveGroup>;
                foreach (InteractiveGroup group in allGroups)
                {
                    if (group.SceneID == SceneID)
                    {
                        groupsAssignedToScene.Add(group);
                    }
                }
                return groupsAssignedToScene;
            }
        }

        /// <summary>
        /// Retrieves a reference to the specified button, if it exists.
        /// </summary>
        public InteractiveButtonControl GetButton(string controlID)
        {
            return InteractivityManager.SingletonInstance.GetButton(controlID);
        }

        /// <summary>
        /// Retrieve a vector of all of the joysticks in the scene. May be empty.
        /// </summary>
        public InteractiveJoystickControl GetJoystick(string controlID)
        {
            return InteractivityManager.SingletonInstance.GetJoystick(controlID);
        }

        internal string etag;

        internal InteractiveScene(string sceneID = "", string newEtag = "")
        {
            SceneID = sceneID;
            etag = newEtag;
        }
    }
}
