#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN && !UNITY_WSA && !UNITY_XBOXONE
using System;

namespace Microsoft.Mixer
{
    public enum InteractiveParticipantState
    {
        Joined,

        InputDisabled,

        Left
    }
}
#endif