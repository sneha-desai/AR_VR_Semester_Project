using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class resetCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Valve.VR.OpenVR.System.ResetSeatedZeroPose();
        Valve.VR.OpenVR.Compositor.SetTrackingSpace(
        Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated);
	}
}
