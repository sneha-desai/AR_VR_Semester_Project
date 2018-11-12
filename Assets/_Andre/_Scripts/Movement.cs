using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

	public float sensitivity = 10.0f;

	// Use this for initialization
	void Start ()
	{
		CapsuleCollider collider = GetComponent<CapsuleCollider>();
	}

//	private void OnTriggerEnter(Collider other)
//	{
//		Debug.Log("message: ",other);
//	}
//
//	private void OnTriggerStay(Collider other)
//	{
//		Debug.Log("message: ",other);
//
//	}
//
//	private void OnTriggerExit(Collider other)
//	{
//		Debug.Log("message: ",other);
//
//	}

	// Update is called once per frame
	void Update ()
	{
		transform.Translate(Vector3.forward * Time.deltaTime);
//		if (Input.GetKey(KeyCode.W))
//		{
//			transform.Translate(Vector3.forward * sensitivity);
//		}
//
//		if (Input.GetKey(KeyCode.S))
//		{
//			transform.Translate(Vector3.back * sensitivity);
//		}
//
//		if (Input.GetKey(KeyCode.A))
//		{
//			transform.Translate(Vector3.left * sensitivity);
//		}
//
//		if (Input.GetKey(KeyCode.D))
//		{
//			transform.Translate(Vector3.right * sensitivity);
//		}
		
	}
}
