/*
 * Mixer Unity SDK
 *
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
 * to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
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
