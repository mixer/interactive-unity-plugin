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
namespace Microsoft.Mixer
{
    /// <summary>
    /// Enum representing the current state of the interactivity service.
    /// The state transitions are:
    /// NotInitialized -> Initializing
    /// Initializing -> ShortCodeRequired
    /// ShortCodeRequired -> InteractivityPending
    /// InteractivityPending -> InteractivityEnabled
    /// InteractivityEnabled -> InteractivityDisabled
    /// </summary>
    public enum InteractivityState
    {
        /// <summary>
        /// The InteractivityManager is not initalized.
        /// </summary>
        NotInitialized,

        /// <summary>
        /// The InteractivityManager is in the process of initializing.
        /// </summary>
        Initializing,

        /// <summary>
        /// The InteractivityManager needs the user to enter a short code on the website.
        /// in order to authenticate with the service.
        /// </summary>
        ShortCodeRequired,

        /// <summary>
        /// The InteractivityManager is initialized.
        /// </summary>
        Initialized,

        /// <summary>
        /// The InteractivityManager is initialized, but interactivity is not enabled.
        /// </summary>
        InteractivityDisabled,

        /// <summary>
        /// Currently connecting to the interactivity service.
        /// </summary>
        InteractivityPending,

        /// <summary>
        /// Interactivity enabled
        /// </summary>
        InteractivityEnabled
    }
}
