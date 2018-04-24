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

        internal InteractiveMouseButtonEventArgs(
            string id,
            InteractiveParticipant participant,
            bool isPressed) : base(InteractiveEventType.MouseButton)
        {
            ControlID = id;
            Participant = participant;
            IsPressed = isPressed;
        }
    }
}