using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public bool created = false;
    public double timestamp;


    // Use this for initialization
    void Start()
    {
        CapsuleCollider collider = GetComponent<CapsuleCollider>();

        DateTime date = DateTime.Now;
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        TimeSpan span = (date - epoch);
        timestamp = span.TotalSeconds;
        Debug.Log(timestamp);
    }

    private void OnTriggerEnter(Collider other)
    {
//		Debug.Log("OnTriggerEnter");

//		Debug.Log(other.name.Contains("Tree"));
        if (other.name.Contains("Tree"))
        {
            Tree otherTree = other.GetComponent<Tree>();
//			Destroy(gameObject);
//			Destroy(other);
            if (timestamp > otherTree.timestamp)
            {
                Destroy(gameObject);
            }
            else
            {
                Destroy(other.gameObject);
                GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
//		Debug.Log("OnTriggerStay");
//		Debug.Log(other);
//		Debug.Log(other.name.Contains("Tree"));
    }

    private void OnTriggerExit(Collider other)
    {
//		Debug.Log("OnTriggerExit");
//		Debug.Log(other);
    }
}