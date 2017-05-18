#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN && !UNITY_WSA_10_0 && !UNITY_XBOXONE
using System;

namespace Xbox.Services.Beam
{
    public class BeamEventArgs: EventArgs
    {
        public BeamEventArgs()
        {
        }

        public DateTime Time
        {
            get;
            private set;
        }

        public int ErrorCode
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public BeamEventType EventType
        {
            get;
            private set;
        }
    }
}
#endif