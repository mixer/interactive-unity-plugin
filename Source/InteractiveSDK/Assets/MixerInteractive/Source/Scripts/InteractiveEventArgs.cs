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
using System;

namespace Microsoft.Mixer
{
    /// <summary>
    /// Interactivity is an event-driven service. This class is the base
    /// class for all interactivity events.
    /// </summary>
    public class InteractiveEventArgs : EventArgs
    {
        /// <summary>
        /// Function to construct a InteractiveEventArgs.
        /// </summary>
        public InteractiveEventArgs()
        {
            Time = DateTime.UtcNow;
            ErrorCode = 0;
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// The time (in UTC) when this event is triggered.
        /// </summary>
        public DateTime Time
        {
            get;
            private set;
        }

        /// <summary>
        /// The error code indicating the result of the operation.
        /// </summary>
        public int ErrorCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns call specific error message with debug information.
        /// Message is not localized as it is meant to be used for debugging only.
        /// </summary>
        public string ErrorMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Type of the event triggered.
        /// </summary>
        public InteractiveEventType EventType
        {
            get;
            private set;
        }

        internal InteractiveEventArgs(InteractiveEventType type)
        {
            Time = DateTime.UtcNow;
            ErrorCode = 0;
            ErrorMessage = string.Empty;
            EventType = type;
        }

        internal InteractiveEventArgs(InteractiveEventType type, int errorCode, string errorMessage)
        {
            Time = DateTime.UtcNow;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            EventType = type;
        }
    }
}
