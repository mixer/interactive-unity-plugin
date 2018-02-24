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
using Microsoft.Mixer;
using UnityEngine;
using UnityEngine.UI;

namespace MixerInteractiveExamples
{
    public class ConnectionStatus : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            // Register for OnInteractivityStateChanged to get notified when the
            // InteractivityState property changes.
            MixerInteractive.OnInteractivityStateChanged += OnInteractivityStateChanged;
        }

        private void OnInteractivityStateChanged(object sender, InteractivityStateChangedEventArgs e)
        {
            // When the InteractivityState property is InteractivityEnabled
            // that means your game is fully connected to the Mixer service and able to 
            // recieve interactive input from the audience.
            if (MixerInteractive.InteractivityState == InteractivityState.InteractivityEnabled)
            {
                var connectionStatus = GetComponent<Text>();
                connectionStatus.text = "Success: Connected!";
            }
        }

        void Update()
        {
            if (Input.GetButton("Fire1"))
            {
                MixerInteractive.GoInteractive();
            }
        }
    }
}
