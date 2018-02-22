namespace Microsoft.Mixer
{
    /// <summary>
    /// Triggered when a participant joins the channel.
    /// </summary>
    public class ParticipantJoinEventArgs : InteractiveEventArgs
    {
        /// <summary>
        /// Participant who has just joined the channel.
        /// </summary>
        public InteractiveParticipant Participant
        {
            get;
            private set;
        }

        internal ParticipantJoinEventArgs(InteractiveParticipant participant): base(InteractiveEventType.ParticipantStateChanged)
        {
            Participant = participant;
        }
    }
}
