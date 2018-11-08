using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Cube : MonoBehaviour {

	// Use this for initialization
	void Start () {
		transform.DOShakeScale(1, Vector3.forward);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
