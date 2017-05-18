#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN && !UNITY_WSA_10_0 && !UNITY_XBOXONE
namespace Xbox.Services.Beam
{

    public class BeamControl
    {
        public string ControlID
        {
            get;
            private set;
        }

        public bool Disabled
        {
            get;
            private set;
        }

        public string HelpText
        {
            get;
            private set;
        }

        public void SetDisabled(bool disabled)
        {
        }

        internal BeamControl(string controlID, bool enabled, string helpText, string eTag, string sceneID)
        {
        }
    }
}
#endif