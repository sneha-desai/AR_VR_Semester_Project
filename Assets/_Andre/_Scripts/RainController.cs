using DigitalRuby.RainMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class RainController : MonoBehaviour {
    private SteamVR_TrackedObject _trackedObj;
    public static GameObject rain;
    private Vector2 _axis;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)_trackedObj.index); }
    }

    void Awake()
    {
        _trackedObj = GetComponent<SteamVR_TrackedObject>();
    }
    
    // Update is called once per frame
    void FixedUpdate () {
        _axis = Controller.GetAxis();

        if (Controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            Debug.Log(_axis);
            rain.GetComponent<RainScript>().RainIntensity = _axis.y;
        }
    }
}
