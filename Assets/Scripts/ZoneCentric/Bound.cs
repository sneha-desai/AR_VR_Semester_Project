using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bound : MonoBehaviour {
	Bounds b;
	// Use this for initialization
	void Start () {
		b = new Bounds(new Vector3(0, 0, 0), new Vector3(5, 5, 5));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
