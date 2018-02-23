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
using UnityEngine;

namespace MixerInteractiveExamples
{
    public class GoInteractiveButton : MonoBehaviour
    {

        public void GoInteractive()
        {
            // The GoInteractive function tells the Mixer SDK to connect to the Mixer service
            // so that you can start recieving interactive events.
            MixerInteractive.GoInteractive();
        }
    }
}
