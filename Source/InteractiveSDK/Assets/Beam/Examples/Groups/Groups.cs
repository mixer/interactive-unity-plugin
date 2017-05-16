using Microsoft;
using UnityEngine;
using UnityEngine.UI;
using Xbox.Services.Beam;

namespace BeamExamples
{
    public class Groups : MonoBehaviour
    {

        public Text group1Text;
        public Text group2Text;

        // Use this for initialization
        void Start()
        {
            // You can listen for the OnParticipantStateChanged event which will tell you when a participant
            // has joined or left. Participants are people viewing the broadcast.
            Beam.OnParticipantStateChanged += OnParticipantStateChanged;
            Beam.GoInteractive();
        }

        private void OnParticipantStateChanged(object sender, BeamParticipantStateChangedEventArgs e)
        {
            // Every time a new participant joins randomly assign them to a new group. Groups can
            // be used to show different sets of controls to your audience.
            BeamParticipant participant = e.Participant;
            if (participant.State == BeamParticipantState.Joined &&
                Beam.Groups.Count >= 2)
            {
                BeamGroup group1 = Beam.Groups[1];
                BeamGroup group2 = Beam.Groups[2];
                int group = Mathf.CeilToInt(Random.value * 2);
                if (group == 1)
                {
                    participant.Group = group1;
                    group1Text.text += "\n" + participant.BeamUserName;
                }
                else if (group == 2)
                {
                    participant.Group = group2;
                    group2Text.text += "\n" + participant.BeamUserName;
                }
            }
        }
    }
}
