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
using Microsoft.Mixer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixerInteractiveExamples
{
    public class SetProgressLogic : MonoBehaviour
    {
        public void SetProgress()
        {
            // Get a reference to the button.
            var button = InteractivityManager.SingletonInstance.GetButton("giveHealth");

            // Calling SetProgress, will show a progress indicator on the button. The method takes a value
            // between 0 and 1, where 0 is 0% and 1 is 100%.
            button.SetProgress(0.5f);
        }
    }
}
