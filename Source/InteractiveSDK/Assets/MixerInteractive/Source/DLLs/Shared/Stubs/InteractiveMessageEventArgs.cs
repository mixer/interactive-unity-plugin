#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN && !UNITY_WSA && !UNITY_XBOXONE
namespace Microsoft.Mixer
{
    public class InteractiveMessageEventArgs : InteractiveEventArgs
    {
        public string Message
        {
            get;
            private set;
        }

        internal InteractiveMessageEventArgs(string message)
        {
        }
    }
}
#endif