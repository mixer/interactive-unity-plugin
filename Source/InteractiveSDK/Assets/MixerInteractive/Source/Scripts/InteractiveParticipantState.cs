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

namespace Microsoft.Mixer
{

    /// <summary>
    /// Enum representing the current state of the participant
    /// </summary>
    public enum InteractiveParticipantState
    {
        /// <summary>
        /// The participant joined the channel
        /// </summary>
        Joined,

        /// <summary>
        /// The participant's input is disabled
        /// </summary>
        InputDisabled,

        /// <summary>
        /// The participant left the channel
        /// </summary>
        Left
    }
}
