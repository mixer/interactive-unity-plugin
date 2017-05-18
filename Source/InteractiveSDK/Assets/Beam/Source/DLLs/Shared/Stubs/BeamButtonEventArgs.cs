#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN && !UNITY_WSA_10_0 && !UNITY_XBOXONE
namespace Xbox.Services.Beam
{
    public class BeamButtonEventArgs : BeamEventArgs
    {
        public string ControlID
        {
            get;
            private set;
        }

        public BeamParticipant Participant
        {
            get;
            private set;
        }
        
        public bool IsPressed
        {
            get;
            private set;
        }
    }
}
#endif