using Microsoft;
using UnityEngine;
using UnityEngine.UI;
using Xbox.Services.Beam;

namespace BeamExamples
{
    public class ConnectionStatus : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            // Register for OnInteractivityStateChanged to get notified when the BeamManager's
            // InteractivityState property changes.
            Beam.OnInteractivityStateChanged += OnInteractivityStateChanged;
        }

        private void OnInteractivityStateChanged(object sender, BeamInteractivityStateChangedEventArgs e)
        {
            // When the BeamManager's InteractivityState property is InteractivityEnabled
            // that means your game is fully connected to the Beam service and able to 
            // recieve interactive input from the audience.
            if (Beam.InteractivityState == BeamInteractivityState.InteractivityEnabled)
            {
                var connectionStatus = GetComponent<Text>();
                connectionStatus.text = "Success: Connected!";
            }
        }
    }
}
