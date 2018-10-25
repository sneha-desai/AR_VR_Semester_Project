using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerGrabObject : MonoBehaviour {
	private SteamVR_TrackedObject trackedObj;
	private GameObject collidingObject; 
	private GameObject objectInHand;
	private Vector3 position;
	private EVRButtonId touchpadButton = EVRButtonId.k_EButton_SteamVR_Touchpad;
	private EVRButtonId gripButton = EVRButtonId.k_EButton_Grip;
	private Dictionary<int, Vector3> initPos = new Dictionary<int, Vector3>();
	private Dictionary<int, Vector3> finalPos = new Dictionary<int, Vector3>();
	private bool evalPhase;
	private List<GameObject> allObjects = new List<GameObject>();
	private List<GameObject> objectsPicked = new List<GameObject>();
	private int correctPos = -1;	// -1 Nothing, 0 is False, 1 is True
	
	private SteamVR_Controller.Device Controller
	{

		get
		{
			return SteamVR_Controller.Input((int)trackedObj.index);
		}
	}
	
	private void SetCollidingObject(Collider col)
	{
		if (collidingObject || !col.GetComponent<Rigidbody>())
		{
			return;
		}
		collidingObject = col.gameObject;
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
		if (!collidingObject)
		{
			return;
		}

		collidingObject = null;
	}
	
	private void GrabObject()
	{
		objectInHand = collidingObject;
		collidingObject = null;
		
		var joint = AddFixedJoint();
		joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
	}

	private FixedJoint AddFixedJoint()
	{
		FixedJoint fx = gameObject.AddComponent<FixedJoint>();
		fx.breakForce = 20000;
		fx.breakTorque = 20000;
		return fx;
	}

	void Awake()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject>();
		evalPhase = false;
		
		// Initial Positions of the PickupObjects
		foreach (GameObject obj in FindObjectsOfType<GameObject>())
		{
			if (obj.CompareTag("PickupObjects"))
			{
				Debug.Log(obj.GetInstanceID());
				allObjects.Add(obj);
				initPos.Add(obj.GetInstanceID(), obj.transform.position);		
			}
		}				
	}

	void OnGUI ()
	{
		Rect statusBox = new Rect(10, 120, 500, 100);
		Rect evalPhaseStatus = new Rect(130, 240, 500, 100);
		if (correctPos == 1)
		{
			GUI.Label (statusBox, "You are TRUE");
		}
		else if(correctPos == 0)
		{
			GUI.Label (statusBox, "You are FALSE");
		}
		else
		{
			GUI.Label(statusBox, "Press the TouchPad Button!");
		}

		if (evalPhase)
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
			
			objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
			objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
		}
		objectInHand = null;
	}

	private void DeactivateObject()
	{
		if (finalPos.ContainsKey(objectInHand.GetInstanceID()))
		{
			finalPos[objectInHand.GetInstanceID()] = objectInHand.transform.position;
		}
		else
		{
			finalPos.Add(objectInHand.GetInstanceID(), objectInHand.transform.position);
		}
		objectsPicked.Add(objectInHand);
		objectInHand.SetActive(false);

		Debug.Log("Object Released!");
		
		//log
	}

	// Update is called once per frame
	void Update () {

		if (evalPhase & objectInHand & Controller.GetPress(touchpadButton))
		{
			float minDist = Single.PositiveInfinity;
			int nearestObjId = 0;
			float dist = 0f;
			GameObject plane = GameObject.Find("Plane");

			foreach (GameObject gameObject in objectsPicked)
			{
				dist = Vector3.Distance(finalPos[gameObject.GetInstanceID()], objectInHand.transform.position);
				if (dist < minDist)
				{
					minDist = dist;
					nearestObjId = gameObject.GetInstanceID();
				}
			}

			if (nearestObjId == objectInHand.GetInstanceID())
			{
				correctPos = 1;
				plane.GetComponent<Renderer>().material.color = Color.green;
			}
			else
			{
				correctPos = 0;
				plane.GetComponent<Renderer>().material.color = Color.magenta;
			}

		}

		if (Controller.GetPressUp(touchpadButton))
		{
			correctPos = -1;
		}
		
		// Appear the objects 
		if (Controller.GetPressDown(gripButton))
		{
			foreach (GameObject gameObject in allObjects)
			{
				Vector3 tempPos = initPos[gameObject.GetInstanceID()];
				if (!gameObject.activeSelf)
				{
					gameObject.SetActive(true);
				}
				gameObject.transform.position = tempPos;

			}

			if (evalPhase)
			{
				evalPhase = false;
				objectsPicked.Clear();
			}
			else
			{
				evalPhase = true;
				
			}
		}
		
		if (Controller.GetHairTriggerDown())
		{
			if (collidingObject)
			{
				GrabObject();
			}
		}

		if (Controller.GetHairTriggerUp())
		{
			if (objectInHand)
			{
				ReleaseObject();

			}
		}
		
		if (objectInHand & Controller.GetPress(touchpadButton))
		{
			if(!evalPhase)
			{
				DeactivateObject();
			}
		}

	}
	
}
