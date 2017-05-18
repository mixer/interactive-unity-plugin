#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN && !UNITY_WSA_10_0 && !UNITY_XBOXONE
namespace Xbox.Services.Beam
{
    public class ParticipantLeaveEventArgs : BeamEventArgs
    {
        public BeamParticipant Participant
        {
            get;
            private set;
        }
    }
}
#endif
