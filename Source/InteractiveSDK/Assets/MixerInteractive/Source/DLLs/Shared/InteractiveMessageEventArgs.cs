namespace Microsoft.Mixer
{
    /// <summary>
    /// Represents a custom message event.
    /// </summary>
    public class InteractiveMessageEventArgs : InteractiveEventArgs
    {
        /// <summary>
        /// The raw contents of the message.
        /// </summary>
        public string Message
        {
            get;
            private set;
        }

        internal InteractiveMessageEventArgs(string message)
        {
            Message = message;
        }
    }
}
