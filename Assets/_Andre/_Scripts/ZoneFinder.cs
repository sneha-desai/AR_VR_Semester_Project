using System.Collections;
using System.Collections.Generic;
using ZoneCentric;
using UnityEngine;
using Valve.VR;
using DigitalRuby.RainMaker;

public class ZoneFinder : MonoBehaviour {
    private SteamVR_TrackedObject _trackedObj;
    private Transform[] PrefabArray;
    private string PrefabType;
    //ZoneCentric
    private static List<Zones> _zoneCentricZones;
    private static int _currentLevelIndex;
    private GameObject _reference;
    private GameObject _cameraObject;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)_trackedObj.index); }
    }

    void Awake()
    {
        _trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    private int GetZone()
    {
        int zone = -1;
        Vector3 clickedPoint = _cameraObject.transform.InverseTransformPoint(_trackedObj.transform.position);
        //Debug.Log("controller point in world space " + _trackedObj.transform.position.x + " " + _trackedObj.transform.position.y + " " + _trackedObj.transform.position.z);
        //Debug.Log("controller point in sphere space " + _trackedObj.transform.InverseTransformPoint(_trackedObj.transform.position).x*10f + " " + (_trackedObj.transform.InverseTransformPoint(_trackedObj.transform.position).y + 0.32f)*10f + " " + _trackedObj.transform.InverseTransformPoint(_trackedObj.transform.position).z*10f);

        var polarPoint = PolarCoordinates.FromCartesian(clickedPoint);

        var polar = PolarCoordinates.RadToDeg(polarPoint.Polar);
        var elevation = PolarCoordinates.RadToDeg(polarPoint.Elevation);

        Debug.Log("in sphere space as per function " + clickedPoint.x + " " + clickedPoint.y + " " + clickedPoint.z + " " + polar + " " + elevation);


        //Vector3 clickedPoint = transform.InverseTransformPoint(_trackedObj.transform.position);

        //            print("ClickedPoint: " + clickedPoint);
        List<float> _subzones = _zoneCentricZones[_currentLevelIndex].SubZones;
        for (int i = 0; i < _subzones.Count / 4; i++)
        {
            //Debug.Log("Checking subzone: " + i);
            if (Interface.CheckPointInZone(clickedPoint, _zoneCentricZones, _currentLevelIndex, i))
            {
                zone = i;
            }
        }

        //print("Cartesian ClickedPoint: " + clickedPoint + " Polar ClickedPoint: polar " + polar + " elevation " + elevation +" zone: " + zone);

        return zone;
    }

    private void initializer(string type, Transform[] prefabs)
    {
        type = "storm";
        Debug.Log("The type of is " + type);
        if (type == "drawable")
        {
            _Andre._Scripts.FloorPaintVr floorPaintVr = _trackedObj.gameObject.AddComponent<_Andre._Scripts.FloorPaintVr>();
            floorPaintVr.PrefabArray = prefabs;
            return;
        }
        if (type == "rain")
        {
            GameObject rainPrefab = Resources.Load<GameObject>("RainPrefab");
            GameObject _rainInstance = Instantiate(rainPrefab);
            RainController rainController = _trackedObj.gameObject.AddComponent<RainController>();
            RainController.rain = _rainInstance;
        }
        if (type == "storm")
        {
            GameObject stormPrefab = Resources.Load<GameObject>("Storm");
            GameObject _stormInstance = Instantiate(stormPrefab);
            _stormInstance.transform.position = new Vector3(transform.position.x, transform.position.y - 3, transform.position.z);
            //RainController rainController = _trackedObj.gameObject.AddComponent<RainController>();
            //RainController.rain = _rainInstance;
        }
    }

    // Use this for initialization
    void Start () {
        //ZoneCentric Code
        _zoneCentricZones = Interface.GetZones();
        _currentLevelIndex = 1;
        _reference = GameObject.Find("Reference");
        _cameraObject = GameObject.Find("Camera (eye)");
    }
	
	// Update is called once per frame
	void Update () {
        if (Controller.GetPressDown(EVRButtonId.k_EButton_Grip))
        {
            int zone = GetZone();
            Debug.Log("zone: " + zone);
            PrefabArray = ZoneDictionary.GetPrefabArrayForZone(zone);
            PrefabType = ZoneDictionary.GetTypeForZone(zone);
            initializer(PrefabType, PrefabArray);
        }
    }
}
