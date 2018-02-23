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
    /// Triggered when a participant leaves the channel.
    /// </summary>
    public class ParticipantLeaveEventArgs : InteractiveEventArgs
    {
        /// <summary>
        /// Participant who has just left the channel.
        /// </summary>
        public InteractiveParticipant Participant
        {
            get;
            private set;
        }

        internal ParticipantLeaveEventArgs(InteractiveParticipant participant)
        {
            Participant = participant;
        }
    }
}
