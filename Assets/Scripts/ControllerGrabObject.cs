using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Random = UnityEngine.Random;

public class ReleaseData
{
	public Vector3 coords;
	public string userName;
	public string objectName;
}

public class ZoneData
{
	public Vector3 coords;
	public string userName;
	public string objectName;
	public int isCorrect;
}

public class ControllerGrabObject : MonoBehaviour {
	private SteamVR_TrackedObject _trackedObj;
	private GameObject _collidingObject; 
	private GameObject _objectInHand;
	private Vector3 _position;

	private const EVRButtonId TouchpadButton = EVRButtonId.k_EButton_SteamVR_Touchpad;
	private const EVRButtonId GripButton = EVRButtonId.k_EButton_Grip;
	
	private static Dictionary<int, Vector3> _initPos = new Dictionary<int, Vector3>();
	private static Dictionary<int, Vector3> _finalPos = new Dictionary<int, Vector3>();
	private static bool _evalPhase;
	private static List<GameObject> _allObjects = new List<GameObject>();
	private static List<GameObject> _objectsPicked = new List<GameObject>();
	private static int _correctPos = -1;	// -1 Nothing, 0 is False, 1 is True
	public string UserName;

	
	private SteamVR_Controller.Device Controller
	{

		get
		{
			return SteamVR_Controller.Input((int)_trackedObj.index);
		}
	}
	
	private void SetCollidingObject(Collider col)
	{
		if (_collidingObject || !col.GetComponent<Rigidbody>())
		{
			return;
		}
		_collidingObject = col.gameObject;
	}
	
	public void OnTriggerEnter(Collider other)
	{
		SetCollidingObject(other);
	}

	public void OnTriggerStay(Collider other)
	{
		SetCollidingObject(other);			

	}

	public void OnTriggerExit(Collider other)
	{
		if (!_collidingObject)
		{
			return;
		}

		_collidingObject = null;
	}
	
	private void GrabObject()
	{
		_objectInHand = _collidingObject;
		_collidingObject = null;
		
		var joint = AddFixedJoint();
		joint.connectedBody = _objectInHand.GetComponent<Rigidbody>();
	}

	private FixedJoint AddFixedJoint()
	{
		FixedJoint fx = gameObject.AddComponent<FixedJoint>();
		fx.breakForce = 20000;
		fx.breakTorque = 20000;
		return fx;
	}

	void Start()
	{
		_evalPhase = false;
		Debug.Log("Initialize is called!");
		// Initial Positions of the PickupObjects
		foreach (GameObject obj in FindObjectsOfType<GameObject>())
		{
			if (obj.CompareTag("PickupObjects"))
			{
				if (!_allObjects.Contains(obj))
					_allObjects.Add(obj);
				if (!_initPos.ContainsKey(obj.GetInstanceID()))
					_initPos.Add(obj.GetInstanceID(), obj.transform.position);		
			}
		}
	}
	
	void Awake()
	{
		_trackedObj = GetComponent<SteamVR_TrackedObject>();
		foreach (GameObject obj in _allObjects)
		{
				Debug.Log(obj.GetInstanceID() + " " + obj.GetComponent(name));
		}				
	}

	void OnGUI ()
	{
		Rect statusBox = new Rect(10, 120, 500, 100);
		Rect evalPhaseStatus = new Rect(130, 240, 500, 100);
		if (_correctPos == 1)
		{
			GUI.Label (statusBox, "You are TRUE");
		}
		else if(_correctPos == 0)
		{
			GUI.Label (statusBox, "You are FALSE");
		}
		else
		{
			GUI.Label(statusBox, "Press the TouchPad Button!");
		}

		if (_evalPhase)
		{
			GUI.Label (evalPhaseStatus, "EvalPhase is TRUE");
		}
		else
		{
			GUI.Label (evalPhaseStatus, "EvalPhase is FALSE");
		}
	}

	private void ReleaseObject()
	{
		if (GetComponent<FixedJoint>())
		{
			GetComponent<FixedJoint>().connectedBody = null;
			Destroy(GetComponent<FixedJoint>());
			
			_objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
			_objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
		}
		_objectInHand = null;
	}

	private void DeactivateObject()
	{
		if (_finalPos.ContainsKey(_objectInHand.GetInstanceID()))
		{
			_finalPos[_objectInHand.GetInstanceID()] = _objectInHand.transform.position;
		}
		else
		{
			_finalPos.Add(_objectInHand.GetInstanceID(), _objectInHand.transform.position);
		}
		_objectsPicked.Add(_objectInHand);
		_objectInHand.SetActive(false);

		Debug.Log("Object Released!");
		Debug.Log(_objectInHand.name);
		var newData = new ReleaseData{
			coords = _objectInHand.transform.position,
			userName = UserName,
			objectName = _objectInHand.name
		};
		Debug.Log(JsonUtility.ToJson(newData));
		
		NetworkManager networkManager = gameObject.AddComponent<NetworkManager>();
		networkManager.uploadData("/uploadReleaseData", JsonUtility.ToJson(newData));
	}

	void IfZoneCorrect()
	{
		Vector3 currPos = _objectInHand.transform.position;
		int nearestObjId = FindNearestObjId(currPos);

		_correctPos = IsNearestCorrect(nearestObjId);
			
		GameObject plane = GameObject.Find("Plane");
			
		if (_correctPos == 1)
		{
			plane.GetComponent<Renderer>().material.color = Color.green;
		}
		else if(_correctPos == 0)
		{
			plane.GetComponent<Renderer>().material.color = Color.magenta;
		}
		
		var newData = new ZoneData{
			coords = _objectInHand.transform.position,
			userName = UserName,
			objectName = _objectInHand.name,
			isCorrect = _correctPos
		};
		
		Debug.Log(JsonUtility.ToJson(newData));
		
		NetworkManager networkManager = gameObject.AddComponent<NetworkManager>();
		networkManager.uploadData("/uploadZoneData", JsonUtility.ToJson(newData));
	}
	
	int FindNearestObjId(Vector3 currPos)
	{
		float minDist = Single.PositiveInfinity;
		int nearestObjId = 0;
		float dist = 0f;

		foreach (GameObject gameObject in _objectsPicked)
		{
			dist = Vector3.Distance(_finalPos[gameObject.GetInstanceID()], currPos);
			if (dist < minDist)
			{
				minDist = dist;
				nearestObjId = gameObject.GetInstanceID();
			}
		}

		return nearestObjId;
	}

	int IsNearestCorrect(int objId)
	{
		if (objId == _objectInHand.GetInstanceID())
		{
			return 1;			
		}
		return 0;
	}

	void PlaceObjInitPos()
	{
		foreach (GameObject gameObject in _allObjects)
		{
			Vector3 tempPos = _initPos[gameObject.GetInstanceID()];
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(true);
			}
			gameObject.transform.position = tempPos;

		}
	}

	void ResetEvalPhase()
	{
		if (_evalPhase)
		{
			_evalPhase = false;
			_objectsPicked.Clear();
		}
		else
		{
			_evalPhase = true;
				
		}
	}

	// Update is called once per frame
	void Update () {

		if (_evalPhase & _objectInHand & Controller.GetPress(TouchpadButton))
		{
			IfZoneCorrect();
		}

		if (Controller.GetPressUp(TouchpadButton))
		{
			_correctPos = -1;
		}
		
		// Objects appear to init position 
		if (Controller.GetPressDown(GripButton))
		{

			PlaceObjInitPos();
			ResetEvalPhase();

		}
		
		if (Controller.GetHairTriggerDown())
		{
			Debug.Log("#############################");
			if (_collidingObject)
			{
				GrabObject();
			}
		}

		if (Controller.GetHairTriggerUp())
		{
			if (_objectInHand)
			{
				ReleaseObject();

			}
		}
		
		if (_objectInHand & Controller.GetPress(TouchpadButton))
		{
			if(!_evalPhase)
			{
				DeactivateObject();
			}
		}

	}
	
}
