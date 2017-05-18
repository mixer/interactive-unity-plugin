using System.Collections.Generic;
using UnityEngine;
using Xbox.Services.Beam;

internal class BeamHelper
{

    internal bool runInBackgroundIfInteractive = true;
    internal string defaultSceneID;
    internal Dictionary<string, string> groupSceneMapping = new Dictionary<string, string>();

    private static BeamHelper _singletonInstance;
    internal static BeamHelper SingletonInstance
    {
        get
        {
            if (_singletonInstance == null)
            {
                _singletonInstance = new BeamHelper();
            }
            return _singletonInstance;
        }
    }
}
