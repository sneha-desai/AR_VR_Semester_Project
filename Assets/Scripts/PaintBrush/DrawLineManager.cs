//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class DrawLineManager : MonoBehaviour
{

	private SteamVR_TrackedObject _trackedObj;
	private LineRenderer currLine;
	private int numClicks = 0;
	private Material lMat;
	
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

	void Start()
	{
		this.lMat = new Material(Shader.Find("Sprites/Default"));
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

		if (currLine != null)
		{
			Debug.Log("Changing Colors");
			Debug.Log(Color.cyan);
			Debug.Log(ColorManager.Instance.GetCurrentColor());
			currLine.material = new Material(this.lMat);
			currLine.SetColors(ColorManager.Instance.GetCurrentColor(), ColorManager.Instance.GetCurrentColor());
		}
	}
}
