using System.Collections.Generic;

namespace Microsoft.Mixer
{
    /// <summary>
    /// Arguments for a button event.
    /// </summary>
    public class InteractiveButtonEventArgs : InteractiveEventArgs
    {
        /// <summary>
        /// Unique string identifier for this control
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
        /// Boolean to indicate if the button is pressed down or not.
        /// Returns TRUE if button is pressed down.
        /// </summary
        public bool IsPressed
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
        /// Spark cost assigned to the button control.
        /// </summary>
        public uint Cost
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

        internal InteractiveButtonEventArgs(
            InteractiveEventType type, 
            string id, 
            InteractiveParticipant participant, 
            bool isPressed,
            uint cost,
            string transactionID) : base(type)
        {
            ControlID = id;
            Participant = participant;
            Cost = cost;
            IsPressed = isPressed;
            TransactionID = transactionID;
        }
    }
}
