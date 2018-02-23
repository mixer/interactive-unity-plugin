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
using Microsoft;
using UnityEngine;
using UnityEngine.UI;

namespace MixerInteractiveExamples
{
    public class HealthValue : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            // Call GoInteractive to connect to the Mixer service so you can start
            // recieving input.
            MixerInteractive.GoInteractive();
        }

        // Update is called once per frame
        void Update()
        {
            // If the button with an ID of "giveHealth" is pressed, then increase
            // the player's health by 1.
            if (MixerInteractive.GetButton("giveHealth"))
            {
                var healthText = GetComponent<Text>();
                int currentHealth = int.Parse(healthText.text);
                currentHealth++;
                healthText.text = currentHealth.ToString();
            }
        }
    }
}
