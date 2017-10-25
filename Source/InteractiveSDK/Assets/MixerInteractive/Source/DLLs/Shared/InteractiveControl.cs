namespace Microsoft.Mixer
{
    /// Base class for Interactivity controls. All controls are created and 
    /// configured using Interactive Studio.
    /// </summary>
#if !WINDOWS_UWP
    [System.Serializable]
#endif
    public class InteractiveControl
    {
        /// <summary>
        /// Unique string identifier for this control
        /// </summary>
        public string ControlID
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates if control is enabled or disabled.
        /// </summary>
        public bool Disabled
        {
            get;
            private set;
        }

        /// <summary>
        /// The string of text that displays when a stream viewer (InteractiveParticipant) hovers over the control.
        /// </summary>
        public string HelpText
        {
            get;
            private set;
        }

        internal string ETag;
        internal string SceneID;

        /// <summary>
        /// Allow game client to disable a control.
        /// </summary>
        /// <param name="disabled">If "true", disables this control. If "false", enables this control</param>
        public void SetDisabled(bool disabled)
        {
            Disabled = disabled;
            InteractivityManager.SingletonInstance.SendSetButtonControlProperties(
                ControlID, 
                InteractivityManager.WS_MESSAGE_VALUE_DISABLED, 
                disabled,
                0,
                string.Empty,
                0);
        }

        internal InteractiveControl(string controlID, bool disabled, string helpText, string eTag, string sceneID)
        {
            ControlID = controlID;
            Disabled = disabled;
            HelpText = helpText;
            ETag = eTag;
            SceneID = sceneID;
        }
    }
}
