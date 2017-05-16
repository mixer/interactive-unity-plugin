using Microsoft;
using UnityEngine;

namespace BeamExamples
{
    public class GoInteractiveButton : MonoBehaviour
    {

        public void GoInteractive()
        {
            // The GoInteractive function tells the Beam SDK to connect to the Beam service
            // so that you can start recieving interactive events.
            Beam.GoInteractive();
        }
    }
}
