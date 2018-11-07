using System;
using UnityEngine;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class Draggable : MonoBehaviour
{
	public bool fixX;
	public bool fixY;
	public Transform thumb;	
	bool dragging;
	
	public SteamVR_TrackedObject _trackedObj;
	
	private SteamVR_Controller.Device Controller
	{

		get
		{
			return SteamVR_Controller.Input((int)_trackedObj.index);
		}
	}
		
	void Awake()
	{
		Debug.Log(_trackedObj);
	}


	void FixedUpdate()
	{
		if (Controller.GetHairTriggerDown())
		{
			dragging = false;
			Ray ray = new Ray(_trackedObj.transform.position, _trackedObj.transform.forward);
//			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity)) {
				dragging = true;
			}
		}
		if (Controller.GetHairTriggerUp()) dragging = false;
		if (dragging && Controller.GetHairTrigger()) {
			Ray ray = new Ray(_trackedObj.transform.position, _trackedObj.transform.forward);
			RaycastHit hit;
			if (GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
			{
				var point = hit.point;
				point = GetComponent<Collider>().ClosestPointOnBounds(point);
				SetThumbPosition(point);
				SendMessage("OnDrag", Vector3.one - (thumb.position - GetComponent<Collider>().bounds.min) / GetComponent<Collider>().bounds.size.x);
			}	
		}
	}

	void SetDragPoint(Vector3 point)
	{
		point = (Vector3.one - point) * GetComponent<Collider>().bounds.size.x + GetComponent<Collider>().bounds.min;
		SetThumbPosition(point);
	}

	void SetThumbPosition(Vector3 point)
	{
		thumb.position = new Vector3(fixX ? thumb.position.x : point.x, fixY ? thumb.position.y : point.y, thumb.position.z);
	}
}
