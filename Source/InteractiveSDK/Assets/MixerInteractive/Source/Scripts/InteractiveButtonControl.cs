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
    /// Represents an interactive button control. All controls are created and 
    /// configured using Interactive Studio.
    /// </summary>
#if !WINDOWS_UWP
    [System.Serializable]
#endif
    public class InteractiveButtonControl : InteractiveControl
    {
        /// <summary>
        /// Text displayed on this button control.
        /// </summary>
        public string ButtonText
        {
            get;
            private set;
        }

        /// <summary>
        /// Spark cost assigned to this button control.
        /// </summary>
        public uint Cost
        {
            get;
            private set;
        }

        /// <summary>
        /// Time remaining (in milliseconds) before this button can be triggered again.
        /// </summary>
        public int RemainingCooldown
        {
            get
            {
                Int64 now = (Int64)DateTime.UtcNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                int timeRemaining = (int)(cooldownExpirationTime - now);
                if (timeRemaining < 0)
                {
                    timeRemaining = 0;
                }
                return timeRemaining;
            }
        }

        /// <summary>
        /// Current progress of this button control.
        /// </summary>
        public float Progress
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether the button has transitioned from up to down since the last call to DoWork().
        /// </summary>
        public bool ButtonDown
        {
            get
            {
                bool isDown = false;
                if (ControlID != null)
                {
                    InternalButtonCountState ButtonCountState;
                    if (InteractivityManager._buttonStates.TryGetValue(ControlID, out ButtonCountState))
                    {
                        isDown = ButtonCountState.CountOfButtonDownEvents > 0;
                    }
                }
                return isDown;
            }
        }

        /// <summary>
        /// Whether the button is pressed.
        /// </summary>
        public bool ButtonPressed
        {
            get
            {
                bool isPressed = false;
                if (ControlID != null)
                {
                    InternalButtonCountState ButtonCountState;
                    if (InteractivityManager._buttonStates.TryGetValue(ControlID, out ButtonCountState))
                    {
                        isPressed = ButtonCountState.CountOfButtonPressEvents > 0;
                    }
                }
                return isPressed;
            }
        }

        /// <summary>
        /// Whether the button has transitioned from down to up since the last call to DoWork().
        /// </summary>
        public bool ButtonUp
        {
            get
            {
                bool isUp = false;
                if (ControlID != null)
                {
                    InternalButtonCountState ButtonCountState;
                    if (InteractivityManager._buttonStates.TryGetValue(ControlID, out ButtonCountState))
                    {
                        isUp = ButtonCountState.CountOfButtonUpEvents > 0;
                    }
                }
                return isUp;
            }
        }

        /// <summary>
        /// The number of button downs since the last call to DoWork().
        /// </summary>
        public uint CountOfButtonDowns
        {
            get
            {
                uint countOfButtonDownEvents = 0;
                if (ControlID != null)
                {
                    InternalButtonCountState ButtonCountState;
                    if (InteractivityManager._buttonStates.TryGetValue(ControlID, out ButtonCountState))
                    {
                        countOfButtonDownEvents = ButtonCountState.CountOfButtonDownEvents;
                    }
                }
                return countOfButtonDownEvents;
            }
        }

        /// <summary>
        /// The number of button presses since the last call to DoWork().
        /// </summary>
        public uint CountOfButtonPresses
        {
            get
            {
                uint countOfButtonPressEvents = 0;
                if (ControlID != null)
                {
                    InternalButtonCountState ButtonCountState;
                    if (InteractivityManager._buttonStates.TryGetValue(ControlID, out ButtonCountState))
                    {
                        countOfButtonPressEvents = ButtonCountState.CountOfButtonPressEvents;
                    }
                }
                return countOfButtonPressEvents;
            }
        }

        /// <summary>
        /// The number of button ups since the last call to DoWork().
        /// </summary>
        public uint CountOfButtonUps
        {
            get
            {
                uint countOfButtonPressEvents = 0;
                if (ControlID != null)
                {
                    InternalButtonCountState ButtonCountState;
                    if (InteractivityManager._buttonStates.TryGetValue(ControlID, out ButtonCountState))
                    {
                        countOfButtonPressEvents = ButtonCountState.CountOfButtonUpEvents;
                    }
                }
                return countOfButtonPressEvents;
            }
        }

        /// <summary>
        /// Function to update the progress value for this button control.
        /// </summary>
        /// <param name="progress">Value from 0.0 to 1.0.</param>
        public void SetProgress(float progress)
        {
            InteractivityManager.SingletonInstance.SendSetButtonControlProperties(
                ControlID,
                InteractivityManager.WS_MESSAGE_KEY_PROGRESS,
                false,
                progress,
                string.Empty,
                0);
        }

        /// <summary>
        /// Function to update the text for this button control.
        /// </summary>
        /// <param name="text">String to display on the button.</param>
        public void SetText(string text)
        {
            InteractivityManager.SingletonInstance.SendSetButtonControlProperties(
                ControlID,
                InteractivityManager.WS_MESSAGE_KEY_TEXT,
                false,
                0,
                text,
                0);
        }

        /// <summary>
        /// Function to update the Spark cost value for this button control.
        /// </summary>
        /// <param name="cost">The number of sparks for this action.</param>
        public void SetCost(uint cost)
        {
            InteractivityManager.SingletonInstance.SendSetButtonControlProperties(
                ControlID,
                InteractivityManager.WS_MESSAGE_KEY_COST,
                false,
                0,
                string.Empty,
                cost);
        }

        /// <summary>
        /// Whether a given user triggered a button down since the last call to DoWork().
        /// </summary>
        /// <param name="userID">The ID of the user who used the input control.</param>
        public bool GetButtonDown(uint userID)
        {
            return InteractivityManager.SingletonInstance.GetButtonDown(ControlID, userID);
        }

        /// <summary>
        /// Whether a given user triggered a button press since the last call to DoWork().
        /// </summary>
        /// <param name="userID">The ID of the user who used the input control.</param>
        public bool GetButtonPressed(uint userID)
        {
            return InteractivityManager.SingletonInstance.GetButtonPressed(ControlID, userID);
        }

        /// <summary>
        /// Whether a given user triggered a button up since the last call to DoWork().
        /// </summary>
        /// <param name="userID">The ID of the user who used the input control.</param>
        public bool GetButtonUp(uint userID)
        {
            return InteractivityManager.SingletonInstance.GetButtonUp(ControlID, userID);
        }

        /// <summary>
        /// The number of button downs from a given user since the last call to DoWork().
        /// </summary>
        /// <param name="userID">The ID of the user who used the input control.</param>
        public uint GetCountOfButtonDowns(uint userID)
        {
            return InteractivityManager.SingletonInstance.GetCountOfButtonDowns(ControlID, userID);
        }

        /// <summary>
        /// The number of button presses from a given user since the last call to DoWork().
        /// </summary>
        /// <param name="userID">The ID of the user who used the input control.</param>
        public uint GetCountOfButtonPresses(uint userID)
        {
            return InteractivityManager.SingletonInstance.GetCountOfButtonPresses(ControlID, userID);
        }

        /// <summary>
        /// The number of button ups from a given user since the last call to DoWork().
        /// </summary>
        /// <param name="userID">The ID of the user who used the input control.</param>
        public uint GetCountOfButtonUps(uint userID)
        {
            return InteractivityManager.SingletonInstance.GetCountOfButtonUps(ControlID, userID);
        }

        /// <summary>
        /// Trigger a cooldown, disabling this control for a period of time.
        /// </summary>
        /// <param name="cooldown">Duration (in milliseconds) required between triggers.</param>
        public void TriggerCooldown(int cooldown)
        {
            InteractivityManager.SingletonInstance.TriggerCooldown(ControlID, cooldown);
        }

        internal Int64 cooldownExpirationTime;

        public InteractiveButtonControl(string controlID, bool disabled, string helpText, uint cost, string eTag, string sceneID) : base(controlID, disabled, helpText, eTag, sceneID)
        {
            Cost = cost;
        }
    }
}
