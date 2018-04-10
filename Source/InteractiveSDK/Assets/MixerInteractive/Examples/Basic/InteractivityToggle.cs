using Microsoft.Mixer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractivityToggle : MonoBehaviour {

    private Toggle toggle;

	// Use this for initialization
	void Start () {
        toggle = GetComponent<Toggle>();
        //Add listener for when the state of the Toggle changes, to take action
        toggle.onValueChanged.AddListener(delegate {
            if (toggle.isOn)
            {
                MixerInteractive.StartInteractive();
            }
            else
            {
                MixerInteractive.StopInteractive();
            }
        });
    }

    // Update is called once per frame
    void Update () {
        // Update the state of the toggle to show if interactivity
        // is currently enabled or not.
        if (MixerInteractive.InteractivityState == InteractivityState.InteractivityDisabled ||
            MixerInteractive.InteractivityState == InteractivityState.NotInitialized)
        {
            toggle.isOn = false;
        }
        else
        {
            toggle.isOn = true;
        }
	}
}
