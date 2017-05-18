#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN && !UNITY_WSA_10_0 && !UNITY_XBOXONE
namespace Xbox.Services.Beam
{
    public class BeamButtonControl : BeamControl
    {
        public string ButtonText
        {
            get;
            private set;
        }

        public uint Cost
        {
            get;
            private set;
        }

        public int RemainingCooldown
        {
            get
            {
                return 0;
            }
        }

        public float Progress
        {
            get;
            private set;
        }

        public bool ButtonDown
        {
            get
            {
                return false;
            }
        }

        public bool ButtonPressed
        {
            get
            {
                return false;
            }
        }

        public bool ButtonUp
        {
            get
            {
                return false;
            }
        }

        public uint CountOfButtonDowns
        {
            get
            {
                return 0;
            }
        }

        public uint CountOfButtonPresses
        {
            get
            {
                return 0;
            }
        }

        public uint CountOfButtonUps
        {
            get
            {
                return 0;
            }
        }

        public void SetProgress(float progress)
        {
        }

        public bool GetButtonDown(uint beamID)
        {
            return false;
        }

        public bool GetButtonPressed(uint beamID)
        {
            return false;
        }

        public bool GetButtonUp(uint beamID)
        {
            return false;
        }

        public uint GetCountOfButtonDowns(uint beamID)
        {
            return 0;
        }

        public uint GetCountOfButtonPresses(uint beamID)
        {
            return 0;
        }

        public uint GetCountOfButtonUps(uint beamID)
        {
            return 0;
        }

        public void TriggerCooldown(int cooldown)
        {
        }

        public BeamButtonControl(string controlID, bool enabled, string helpText, string eTag, string sceneID) : base(controlID, enabled, helpText, eTag, sceneID)
        {
        }
    }
}
#endif