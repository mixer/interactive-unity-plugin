//*********************************************************
//
//    Copyright (c) Microsoft. All rights reserved.
//    This code is licensed under the Microsoft Public License.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
namespace Microsoft.Mixer
{
    /// <summary>
    /// Represents a custom message event.
    /// </summary>
    public class InteractiveMessageEventArgs : InteractiveEventArgs
    {
        /// <summary>
        /// The raw contents of the message.
        /// </summary>
        public string Message
        {
            get;
            private set;
        }

        internal InteractiveMessageEventArgs(string message)
        {
            Message = message;
        }
    }
}
