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
