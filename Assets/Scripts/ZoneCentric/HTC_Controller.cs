using Valve.VR;
using UnityEngine;

namespace ZoneCentric
{
    public class HTC_Controller : MonoBehaviour {
		public SteamVR_TrackedObject _trackedObj;
		private const EVRButtonId TouchpadButton = EVRButtonId.k_EButton_SteamVR_Touchpad;
	
	
		private SteamVR_Controller.Device Controller
		{
	
			get
			{
				return SteamVR_Controller.Input((int)_trackedObj.index);
			}
		}
	    private void Awake()
	    {
			_trackedObj = GetComponent<SteamVR_TrackedObject>();
	    }
    }
}