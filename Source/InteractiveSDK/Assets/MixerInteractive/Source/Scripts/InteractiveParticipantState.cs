using System;

namespace Microsoft.Mixer
{

    /// <summary>
    /// Enum representing the current state of the participant
    /// </summary>
    public enum InteractiveParticipantState
    {
        /// <summary>
        /// The participant joined the channel
        /// </summary>
        Joined,

        /// <summary>
        /// The participant's input is disabled
        /// </summary>
        InputDisabled,

        /// <summary>
        /// The participant left the channel
        /// </summary>
        Left
    }
}
