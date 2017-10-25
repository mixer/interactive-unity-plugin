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
