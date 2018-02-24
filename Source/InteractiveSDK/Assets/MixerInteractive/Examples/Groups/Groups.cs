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
    public class Groups : MonoBehaviour
    {

        public Text group1Text;
        public Text group2Text;

        // Use this for initialization
        void Start()
        {
            // You can listen for the OnParticipantStateChanged event which will tell you when a participant
            // has joined or left. Participants are people viewing the broadcast.
            MixerInteractive.OnParticipantStateChanged += OnParticipantStateChanged;
            MixerInteractive.GoInteractive();
        }

        private void OnParticipantStateChanged(object sender, InteractiveParticipantStateChangedEventArgs e)
        {
            // Every time a new participant joins randomly assign them to a new group. Groups can
            // be used to show different sets of controls to your audience.
            InteractiveParticipant participant = e.Participant;
            if (participant.State == InteractiveParticipantState.Joined &&
                MixerInteractive.Groups.Count >= 2)
            {
                int group = Mathf.CeilToInt(Random.value * 2);
                if (group == 1)
                {
                    participant.Group = MixerInteractive.GetGroup("Group1");
                    group1Text.text += "\n" + participant.UserName;
                }
                else
                {
                    participant.Group = MixerInteractive.GetGroup("Group2");
                    group2Text.text += "\n" + participant.UserName;
                }
            }
        }
    }
}
