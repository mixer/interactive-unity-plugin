using System.Collections.Generic;

namespace Microsoft.Mixer
{
    /// <summary>
    /// Represents an interactivity joystick control. All controls are created and 
    /// configured using Interactive Studio.
    /// </summary>
#if !WINDOWS_UWP
    [System.Serializable]
#endif
    public class InteractiveJoystickControl : InteractiveControl
    {
        /// <summary>
        /// The current X coordinate of the joystick, in the range of [-1, 1].
        /// </summary>
        public double X
        {
            get
            {
                double x = 0;
                if (ControlID != null)
                {
                    InternalJoystickState joystickState;
                    if (InteractivityManager._joystickStates.TryGetValue(ControlID, out joystickState))
                    {
                        x = joystickState.X;
                    }
                }
                return x;
            }
        }

        /// <summary>
        /// The current Y coordinate of the joystick, in the range of [-1, 1].
        /// </summary>
        public double Y
        {
            get
            {
                double y = 0;
                if (ControlID != null)
                {
                    InternalJoystickState joystickState;
                    if (InteractivityManager._joystickStates.TryGetValue(ControlID, out joystickState))
                    {
                        y = joystickState.Y;
                    }
                }
                return y;
            }
        }

        /// <summary>
        /// The current [0,1] intensity of this joystick control
        /// </summary>
        public double Intensity
        {
            get;
            private set;
        }

        /// <summary>
        /// The X coordinate of the joystick for a given participant since the last call to DoWork().
        /// </summary>
        /// <param name="userID">The ID of the participant who used the input control.</param>
        public double GetX(uint userID)
        {
            return InteractivityManager.SingletonInstance.GetJoystickX(ControlID, userID);
        }

        /// <summary>
        /// The Y coordinate of the joystick for a given participant since the last call to DoWork().
        /// </summary>
        /// <param name="userID">The ID of the participant who used the input control.</param>
        public double GetY(uint userID)
        {
            return InteractivityManager.SingletonInstance.GetJoystickY(ControlID, userID);
        }

        public InteractiveJoystickControl(string controlID, bool enabled, string helpText, string eTag, string sceneID) : base(controlID, enabled, helpText, eTag, sceneID)
        {
        }

        internal uint UserID;

        private bool TryGetJoystickStateByParticipant(uint userID, string controlID, out InternalJoystickState joystickState)
        {
            joystickState = new InternalJoystickState();
            bool joystickExists = false;
            bool participantExists = false;
            Dictionary<string, InternalJoystickState> participantControls;
            participantExists = InteractivityManager._joystickStatesByParticipant.TryGetValue(userID, out participantControls);
            if (participantExists)
            {
                bool controlExists = false;
                controlExists = participantControls.TryGetValue(controlID, out joystickState);
                if (controlExists)
                {
                    joystickExists = true;
                }
            }
            return joystickExists;
        }
    }
}
