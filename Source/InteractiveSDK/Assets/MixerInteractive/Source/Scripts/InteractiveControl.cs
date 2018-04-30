/*
 * Mixer Unity SDK
 *
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
 * to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
using System.Collections.Generic;

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

        /// <summary>
        /// Returns a read only dictionary of name, value pairs for the control's metadata.
        /// </summary>
        public IDictionary<string, object> MetaProperties
        {
            get;
            private set;
        }

        internal string _eTag;
        internal string _sceneID;
        internal string _kind;
        internal InteractiveEventType _type;
        internal int participantID;

        /// <summary>
        /// Allow game client to disable a control.
        /// </summary>
        /// <param name="disabled">If "true", disables this control. If "false", enables this control</param>
        public void SetDisabled(bool disabled)
        {
            Disabled = disabled;
            InteractivityManager.SingletonInstance._SendSetButtonControlProperties(
                ControlID, 
                InteractivityManager._WS_MESSAGE_VALUE_DISABLED, 
                disabled,
                0,
                string.Empty,
                0);
        }

        /// <summary>
        /// This function allows you to set any property on a control.
        /// </summary>
        /// <param name="name">The name of the control property.</param>
        /// <param name="value">The value of the control property.</param>
        public void SetProperty(InteractiveControlProperty name, bool value)
        {
            SetPropertyImpl(
                InteractivityManager.SingletonInstance._InteractiveControlPropertyToString(name),
                value
                );
        }
        public void SetProperty(InteractiveControlProperty name, double value)
        {
            SetPropertyImpl(
                InteractivityManager.SingletonInstance._InteractiveControlPropertyToString(name),
                value
                );
        }
        public void SetProperty(InteractiveControlProperty name, string value)
        {
            SetPropertyImpl(
                InteractivityManager.SingletonInstance._InteractiveControlPropertyToString(name),
                value
                );
        }
        public void SetProperty(InteractiveControlProperty name, object value)
        {
            SetPropertyImpl(
                InteractivityManager.SingletonInstance._InteractiveControlPropertyToString(name), 
                value
                );
        }

        /// <summary>
        /// This function allows you to set any property on a control.
        /// </summary>
        /// <param name="name">The name of the control property.</param>
        /// <param name="value">The value of the control property.</param>
        public void SetProperty(string name, bool value)
        {
            SetPropertyImpl(name, value);
        }
        public void SetProperty(string name, double value)
        {
            SetPropertyImpl(name, value);
        }
        public void SetProperty(string name, string value)
        {
            SetPropertyImpl(name, value);
        }
        public void SetProperty(string name, object value)
        {
            SetPropertyImpl(name, value);
        }

        private void SetPropertyImpl(string name, bool value)
        {
            InteractivityManager.SingletonInstance._QueuePropertyUpdate(_sceneID, ControlID, name, value);
        }
        private void SetPropertyImpl(string name, double value)
        {
            InteractivityManager.SingletonInstance._QueuePropertyUpdate(_sceneID, ControlID, name, value);
        }
        private void SetPropertyImpl(string name, string value)
        {
            InteractivityManager.SingletonInstance._QueuePropertyUpdate(_sceneID, ControlID, name, value);
        }
        private void SetPropertyImpl(string name, object value)
        {
            InteractivityManager.SingletonInstance._QueuePropertyUpdate(_sceneID, ControlID, name, value);
        }

        internal InteractiveControl(string controlID, string kind, InteractiveEventType type, bool disabled, string helpText, string eTag, string sceneID, Dictionary<string, object> metaProperties)
        {
            ControlID = controlID;
            _kind = kind;
            _type = type;
            Disabled = disabled;
            HelpText = helpText;
            _eTag = eTag;
            _sceneID = sceneID;
            participantID = -1;
            MetaProperties = metaProperties;
        }
    }
}
