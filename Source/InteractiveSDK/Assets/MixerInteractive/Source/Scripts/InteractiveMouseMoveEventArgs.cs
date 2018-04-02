using UnityEngine;

namespace Microsoft.Mixer
{
    public class InteractiveCoordinatesChangedEventArgs : InteractiveEventArgs
    {
        public string ControlID
        {
            get;
            private set;
        }

        public InteractiveParticipant Participant
        {
            get;
            private set;
        }

        public Vector3 Position
        {
            get;
            private set;
        }

        internal InteractiveCoordinatesChangedEventArgs(
            string id,
            InteractiveParticipant participant,
            Vector3 position) : base(InteractiveEventType.Coordinates)
        {
            ControlID = id;
            Participant = participant;
            Position = position;
        }
    }
}