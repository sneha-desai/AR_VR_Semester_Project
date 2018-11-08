using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motion : MonoBehaviour {
	public Rigidbody m_Rigidbody;
	float m_Speed;

	void Start(){
//		Debug.Log("Start");
		m_Rigidbody = GetComponent<Rigidbody>();
		//Set the speed of the GameObject
		m_Speed = 10.0f;
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.UpArrow))
		{
			//Move the Rigidbody forwards constantly at speed you define (the blue arrow axis in Scene view)
			m_Rigidbody.velocity = transform.forward * m_Speed;
		}

		if (Input.GetKey(KeyCode.DownArrow))
		{
			//Move the Rigidbody backwards constantly at the speed you define (the blue arrow axis in Scene view)
			m_Rigidbody.velocity = -transform.forward * m_Speed;
		}

		if (Input.GetKey(KeyCode.RightArrow))
		{
			//Rotate the sprite about the Y axis in the positive direction
			transform.Rotate(new Vector3(0, 1, 0) * Time.deltaTime * m_Speed, Space.World);
		}

		if (Input.GetKey(KeyCode.LeftArrow))
		{
			//Rotate the sprite about the Y axis in the negative direction
			transform.Rotate(new Vector3(0, -1, 0) * Time.deltaTime * m_Speed, Space.World);
		}
	}
}
