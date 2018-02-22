using Microsoft.Mixer;
using UnityEngine;
using UnityEngine.UI;

namespace MixerInteractiveExamples
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
            MixerInteractive.OnParticipantStateChanged += OnParticipantStateChanged;
            MixerInteractive.GoInteractive();
        }

        private void OnParticipantStateChanged(object sender, InteractiveParticipantStateChangedEventArgs e)
        {
            // Every time a new participant joins randomly assign them to a new group. Groups can
            // be used to show different sets of controls to your audience.
            InteractiveParticipant participant = e.Participant;
            if (participant.State == InteractiveParticipantState.Joined &&
                MixerInteractive.Groups.Count >= 2)
            {
                int group = Mathf.CeilToInt(Random.value * 2);
                if (group == 1)
                {
                    participant.Group = MixerInteractive.GetGroup("Group1");
                    group1Text.text += "\n" + participant.UserName;
                }
                else
                {
                    participant.Group = MixerInteractive.GetGroup("Group2");
                    group2Text.text += "\n" + participant.UserName;
                }
            }
        }
    }
}
