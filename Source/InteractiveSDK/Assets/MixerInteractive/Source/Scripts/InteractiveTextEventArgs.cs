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

        /// <summary>
        /// Unique string identifier for the spark transaction associated with this control event.
        /// </summary>
        public string TransactionID
        {
            get;
            private set;
        }

        /// <summary>
        /// Captures a given interactive event transaction, charging the sparks to the appropriate Participant.
        /// </summary>
        public void CaptureTransaction()
        {
            InteractivityManager.SingletonInstance.CaptureTransaction(TransactionID);
        }

        internal InteractiveTextEventArgs(
            InteractiveEventType type, 
            string id, 
            InteractiveParticipant participant,
            string text,
            string transactionID) : base(type)
        {
            ControlID = id;
            Participant = participant;
            Text = text;
            TransactionID = transactionID;
        }
    }
}
