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
