using System;
using System.Collections.Generic;

namespace Microsoft.Mixer
{
    /// <summary>
    /// Represents a control that allows a viewer to enter text. All controls are created and 
    /// configured using Interactive Studio.
    /// </summary>
#if !WINDOWS_UWP
    [System.Serializable]
#endif
    public class InteractiveTextControl : InteractiveControl
    {
        /// <summary>
        /// Text from the text entry control.
        /// </summary>
        public IList<InteractiveTextResult> TextResults
        {
            get
            {
                return InteractivityManager.SingletonInstance._GetText(ControlID);
            }
        }

        public InteractiveTextControl(string controlID, InteractiveEventType type, bool disabled, string helpText, string eTag, string sceneID, Dictionary<string, object> metaproperties) : 
            base(controlID, InteractivityManager._CONTROL_KIND_TEXTBOX, type, disabled, helpText, eTag, sceneID, metaproperties)
        {
        }
    }
}
