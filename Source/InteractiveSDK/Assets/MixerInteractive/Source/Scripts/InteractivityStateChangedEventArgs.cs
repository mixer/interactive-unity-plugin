//*********************************************************
//
//    Copyright (c) Microsoft. All rights reserved.
//    This code is licensed under the Microsoft Public License.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System.Collections.Generic;

namespace Microsoft.Mixer
{
    /// <summary>
    /// Contains the new interactivity state of the InteractivityManager.
    /// </summary>
    public class InteractivityStateChangedEventArgs : InteractiveEventArgs
    {
        /// <summary>
        /// Unique string identifier for this control.
        /// </summary>
        public InteractivityState State
        {
            get;
            private set;
        }

        internal InteractivityStateChangedEventArgs(InteractiveEventType type, InteractivityState state) : base(type)
        {
            State = state;
        }
    }
}
