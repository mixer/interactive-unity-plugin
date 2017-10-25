using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Mixer;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //MixerInteractive.GoInteractive();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButton("Fire1"))
        {
            if (InteractivityManager.SingletonInstance.InteractivityState != InteractivityState.InteractivityEnabled)
            {
                MixerInteractive.GoInteractive();
            }
            else
            {
                MixerInteractive.TriggerCooldown("giveHealth", 7);
            }
        }
	}
}
