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
    /// A list of possible levels of logging from the Interactive SDK.
    /// </summary>
    public enum LoggingLevel
    {
        /// <summary>
        /// No debug output.
        /// </summary>
        None,

        /// <summary>
        /// Only errors and warnings.
        /// </summary>
        Minimal,

        /// <summary>
        /// All events, including every websocket and HTTP message.
        /// </summary>
        Verbose
    }
}
