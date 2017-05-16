using Microsoft;
using UnityEngine;
using Xbox.Services.Beam;

namespace BeamExamples
{
    public class ControlCharacterWithBeam : MonoBehaviour
    {

        public float speed;

        private uint participantID;

        // Use this for initialization
        void Start()
        {
            Beam.GoInteractive();
        }

        // Update is called once per frame
        void Update()
        {
            // Chose a participant to control this character.
            if (Beam.Participants.Count > 0)
            {
                // For this example, we'll choose the 1st participant.
                participantID = Beam.Participants[0].BeamID;
            }

            // Allow the audience to control the in game character.
            // Note: If participantID is empty it is fine. There won't be errors, we 
            // just won't recieve input recieved until a participant joins the stream.
            if (BeamManager.SingletonInstance.GetJoystick("move").GetX(participantID) < 0)
            {
                transform.position += new Vector3(-1 * speed, 0, 0);
            }
            else if (BeamManager.SingletonInstance.GetJoystick("move").GetX(participantID) > 0)
            {
                transform.position += new Vector3(speed, 0, 0);
            }
            if (BeamManager.SingletonInstance.GetJoystick("move").GetY(participantID) < 0)
            {
                transform.position += new Vector3(0, -1 * speed, 0);
            }
            else if (BeamManager.SingletonInstance.GetJoystick("move").GetY(participantID) > 0)
            {
                transform.position += new Vector3(0, speed, 0);
            }

            // Allow the audience to make the player spin.
            if (BeamManager.SingletonInstance.GetButton("spin").GetButtonPressed(participantID)) {
                transform.Rotate(0, 0, 10f);
            }
        }
    }
}
