using UnityEngine;

namespace Microsoft.Mixer
{
    public class InteractiveMouseButtonEventArgs : InteractiveEventArgs
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

        public bool IsPressed
        {
            get;
            private set;
        }

        public Vector3 Position
        {
            get;
            private set;
        }

        internal InteractiveMouseButtonEventArgs(
            string id,
            InteractiveParticipant participant,
            bool isPressed,
            Vector3 position) : base(InteractiveEventType.MouseButton)
        {
            ControlID = id;
            Participant = participant;
            IsPressed = isPressed;
            Position = position;
        }
    }
}