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
using UnityEngine.UI;

public class MixerInteractiveDialog : MonoBehaviour {

    public Text shortCodeElement;
    private Canvas _dialogCanvas;

	// Use this for initialization
	void Start () {
        _dialogCanvas = GetComponent<Canvas>();
    }
	
	// Update is called once per frame
	void Update () {
        if (_dialogCanvas != null &&
            _dialogCanvas.enabled && Input.GetButton("Cancel"))
        {
            Hide();
        }
	}

    public void Show(string shortCode)
    {
        RefreshShortCode(shortCode);
        if (_dialogCanvas != null)
        {
            _dialogCanvas.enabled = true;
        }
    }

    public void Hide()
    {
        if (_dialogCanvas != null)
        {
            _dialogCanvas.enabled = false;
        }
    }

    private void RefreshShortCode(string shortCode)
    {
        shortCodeElement.text = shortCode;
    }
}
