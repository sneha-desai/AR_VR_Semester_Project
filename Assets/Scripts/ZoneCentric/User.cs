using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZoneCentric;

public class User : MonoBehaviour
{

	public Vector3 origin = new Vector3();
	private bool _isCalibrated = false;
	private SteamVR_Controller.Device _left;
	private SteamVR_Controller.Device _right;
	
	public bool IsCalibrated()
	{
		return _isCalibrated;
	}

	public void Calibrate(Vector3 point)
	{
		origin = point;
		transform.position = point;
		_isCalibrated = true;
	}

	public void Reset()
	{
		_isCalibrated = false;
	}
	
	// Use this for initialization
	void Start () {
	}

	private void Awake()
	{
	}

	// Update is called once per frame
	void Update () {
	}
}
