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
using System;

namespace Microsoft.Mixer
{
    /// <summary>
    /// Defines values used to indicate the interactive event types.
    /// </summary>
    public enum InteractiveControlProperty
    {
        /// <summary>
        /// The display test of the control.
        /// </summary>
        Text,

        /// <summary>
        /// The background color of the control.
        /// </summary>
        BackgroundColor,

        /// <summary>
        /// The background image of the control.
        /// </summary>
        BackgroundImage,

        /// <summary>
        /// The text color of the display text for the control.
        /// </summary>
        TextColor,

        /// <summary>
        /// The text size of the display text for the control.
        /// </summary>
        TextSize,

        /// <summary>
        /// The border color of the control.
        /// </summary>
        BorderColor,

        /// <summary>
        /// The focus color of the control.
        /// </summary>
        FocusColor,

        /// <summary>
        /// The accent color. Where this color shows up will depend on the control.
        /// </summary>
        AccentColor
    }
}