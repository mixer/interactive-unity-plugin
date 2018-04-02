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
                    _InternalJoystickState joystickState;
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
                    _InternalJoystickState joystickState;
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
            return InteractivityManager.SingletonInstance._GetJoystickX(ControlID, userID);
        }

        /// <summary>
        /// The Y coordinate of the joystick for a given participant since the last call to DoWork().
        /// </summary>
        /// <param name="userID">The ID of the participant who used the input control.</param>
        public double GetY(uint userID)
        {
            return InteractivityManager.SingletonInstance._GetJoystickY(ControlID, userID);
        }

        public InteractiveJoystickControl(string controlID, InteractiveEventType type, bool enabled, string helpText, string eTag, string sceneID) : base(controlID, InteractivityManager._CONTROL_TYPE_JOYSTICK, type, enabled, helpText, eTag, sceneID)
        {
        }

        internal uint _userID;

        private bool TryGetJoystickStateByParticipant(uint userID, string controlID, out _InternalJoystickState joystickState)
        {
            joystickState = new _InternalJoystickState();
            bool joystickExists = false;
            bool participantExists = false;
            Dictionary<string, _InternalJoystickState> participantControls;
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
