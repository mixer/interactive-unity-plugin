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
    /// Represents a joystick control event. These events are sent down at an
    /// interval frequency configured with Interactive Studio.
    /// </summary>
    public class InteractiveJoystickEventArgs : InteractiveEventArgs
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
        /// The X coordinate of the joystick, in the range of [-1, 1].
        /// </summary>
        public double X
        {
            get;
            private set;
        }

        /// <summary>
        /// The Y coordinate of the joystick, in the range of [-1, 1].
        /// </summary>
        public double Y
        {
            get;
            private set;
        }

        /// <summary>
        /// Participant whose action this event represents
        /// </summary>
        public InteractiveParticipant Participant
        {
            get;
            private set;
        }

        internal InteractiveJoystickEventArgs(InteractiveEventType type, string id, InteractiveParticipant participant, double x, double y) : base(type)
        {
            ControlID = id;
            Participant = participant;
            X = x;
            // We invert the y-axis to match Unity conventions. In Unity up is positive and down is negative.
            Y = -1 * y;
        }
    }
}
