//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class DrawLineManager : MonoBehaviour
{

	public SteamVR_TrackedObject _trackedObj;
	private LineRenderer currLine;
	private int numClicks = 0;
	
	private SteamVR_Controller.Device Controller
	{

		get
		{
			return SteamVR_Controller.Input((int)_trackedObj.index);
		}
	}
		
	void Awake()
	{
		_trackedObj = GetComponent<SteamVR_TrackedObject>();				
	}

	void Update()
	{
		if (Controller.GetHairTriggerDown())
		{
			GameObject go = new GameObject();
			currLine = go.AddComponent<LineRenderer>();
			currLine.SetWidth(0.01f, 0.01f);
			numClicks = 0;
		} else if (Controller.GetHairTrigger())
		{
			currLine.SetVertexCount(numClicks + 1);
			currLine.SetPosition(numClicks, _trackedObj.transform.position);
			numClicks++;
		}
	}
}
