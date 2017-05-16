using Microsoft;
using UnityEngine;

namespace BeamExamples
{
    public class Move : MonoBehaviour
    {

        public float speed;

        // Use this for initialization
        void Start()
        {
            // Call GoInteractive to connect to the Beam service so you can start
            // recieving input.
            Beam.GoInteractive();
        }

        // Update is called once per frame
        void Update()
        {
            // Respond to joystick input from the viewer by calling GetJoystickX and GetJoystickY
            // and moving the player.
            if (Beam.GetJoystickX("move") < 0)
            {
                transform.position += new Vector3(-1 * speed, 0, 0);
            }
            else if (Beam.GetJoystickX("move") > 0)
            {
                transform.position += new Vector3(speed, 0, 0);
            }
            if (Beam.GetJoystickY("move") < 0)
            {
                transform.position += new Vector3(0, -1 * speed, 0);
            }
            else if (Beam.GetJoystickY("move") > 0)
            {
                transform.position += new Vector3(0, speed, 0);
            }
        }
    }
}
