using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour {
	
	public float MovementSpeed = 0.3f;
	public float SpeedH = 2.0f;
	public float SpeedV = 2.0f;


	private float _yaw = 0.0f;
	private float _pitch = 0.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.W))
		{
			transform.Translate(Vector3.forward * MovementSpeed);
		}

		if (Input.GetKey(KeyCode.S))
		{
			transform.Translate(Vector3.back * MovementSpeed);
		}

		if (Input.GetKey(KeyCode.A))
		{
			transform.Translate(Vector3.left * MovementSpeed);
		}

		if (Input.GetKey(KeyCode.D))
		{
			transform.Translate(Vector3.right * MovementSpeed);
		}
		_yaw += SpeedH * Input.GetAxis("Mouse X");
		_pitch -= SpeedV * Input.GetAxis("Mouse Y");
		transform.eulerAngles = new Vector3(_pitch, _yaw, 0.0f);
	}
}
