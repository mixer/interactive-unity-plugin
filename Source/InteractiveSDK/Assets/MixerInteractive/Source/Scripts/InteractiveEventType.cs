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
namespace Microsoft.Mixer
{
    /// <summary>
    /// Defines values used to indicate the interactive event types.
    /// </summary>
    public enum InteractiveEventType
    {
        /// <summary>
        /// A default value used all messages.
        /// </summary>
        Unknown,

        /// <summary>
        /// This event is triggered when the service or manager encounters an error.
        /// The Error and ErrorMessage members will contain pertinent info.
        /// </summary>
        Error,

        /// <summary>
        /// The interactivity state of the InteractivityManager has changed.
        /// </summary>
        InteractivityStateChanged,

        /// <summary>
        /// Event fired when a participant's state changes, e.g., joins, leaves,
        /// has input disabled, etc.
        /// </summary>
        ParticipantStateChanged,

        /// <summary>
        /// Events representing button input.
        /// </summary>
        Button,

        /// <summary>
        /// Events representing joystick input.
        /// </summary>
        Joystick
    }
}
