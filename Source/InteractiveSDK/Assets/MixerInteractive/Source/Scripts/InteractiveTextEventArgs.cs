using System.Collections.Generic;

namespace Microsoft.Mixer
{
    /// <summary>
    /// Arguments for when the text on an interactive text entry control changes.
    /// </summary>
    public class InteractiveTextEventArgs : InteractiveEventArgs
    {
        /// <summary>
        /// Unique string identifier for this control.
        /// </summary>
        public string ControlID
        {
            get;
            private set;
        }

        /// <summary>
        /// The participant who triggered this event.
        /// </summary>
        public InteractiveParticipant Participant
        {
            get;
            private set;
        }

        /// <summary>
        /// The new text value of the control.
        /// </summary>
        public string Text
        {
            get;
            private set;
        }

        internal InteractiveTextEventArgs(
            InteractiveEventType type, 
            string id, 
            InteractiveParticipant participant,
            string text) : base(type)
        {
            ControlID = id;
            Participant = participant;
            Text = text;
        }
    }
}
