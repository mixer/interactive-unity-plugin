namespace Microsoft.Mixer
{
    /// <summary>
    /// The object returned from the interactive text APIs.
    /// </summary>
    public struct InteractiveTextResult
    {
        /// <summary>
        /// The participant who entered the text.
        /// </summary>
        public InteractiveParticipant Participant
        {
            get;
            set;
        }

        /// <summary>
        /// The value of the text the participant entered.
        /// </summary>
        public string Text
        {
            get;
            set;
        }
    }
}
