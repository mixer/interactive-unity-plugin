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
    /// Triggered when a participant joins or leaves the channel.
    /// </summary>
    public class InteractiveParticipantStateChangedEventArgs : InteractiveEventArgs
    {
        /// <summary>
        /// Participant who has just joined the channel
        /// </summary>
        public InteractiveParticipant Participant
        {
            get;
            private set;
        }

        /// <summary>
        /// The participant's current state.
        /// </summary>
        public InteractiveParticipantState State
        {
            get;
            private set;
        }

        internal InteractiveParticipantStateChangedEventArgs(InteractiveEventType type, InteractiveParticipant participant, InteractiveParticipantState state) : base(type)
        {
            Participant = participant;
            State = state;
        }
    }
}
