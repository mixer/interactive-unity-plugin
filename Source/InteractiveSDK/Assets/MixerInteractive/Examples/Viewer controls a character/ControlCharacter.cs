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
using Microsoft;
using UnityEngine;
using Microsoft.Mixer;

namespace MixerInteractiveExamples
{
    public class ControlCharacter : MonoBehaviour
    {

        public float speed;

        private uint participantID;

        // Use this for initialization
        void Start()
        {
            MixerInteractive.GoInteractive();
        }

        // Update is called once per frame
        void Update()
        {
            // Chose a participant to control this character.
            if (MixerInteractive.Participants.Count > 0)
            {
                // For this example, we'll choose the 1st participant.
                participantID = MixerInteractive.Participants[0].UserID;
            }

            // Allow the audience to control the in game character.
            // Note: If participantID is empty it is fine. There won't be errors, we 
            // just won't recieve input recieved until a participant joins the stream.
            if (InteractivityManager.SingletonInstance.GetJoystick("move").GetX(participantID) < 0)
            {
                transform.position += new Vector3(-1 * speed, 0, 0);
            }
            else if (InteractivityManager.SingletonInstance.GetJoystick("move").GetX(participantID) > 0)
            {
                transform.position += new Vector3(speed, 0, 0);
            }
            if (InteractivityManager.SingletonInstance.GetJoystick("move").GetY(participantID) < 0)
            {
                transform.position += new Vector3(0, -1 * speed, 0);
            }
            else if (InteractivityManager.SingletonInstance.GetJoystick("move").GetY(participantID) > 0)
            {
                transform.position += new Vector3(0, speed, 0);
            }

            // Allow the audience to make the player spin.
            if (InteractivityManager.SingletonInstance.GetButton("spin").GetButtonPressed(participantID)) {
                transform.Rotate(0, 0, 10f);
            }
        }
    }
}
