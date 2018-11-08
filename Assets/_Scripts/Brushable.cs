using System;
using UnityEngine;

public class Brushable : MonoBehaviour
{
    public double Timestamp;


    private void Awake()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    void Start()
    {

        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rBody = gameObject.AddComponent<Rigidbody>();
            rBody.useGravity = false;
        }

        DateTime date = DateTime.Now;
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        TimeSpan span = (date - epoch);
        Timestamp = span.TotalSeconds;
        Debug.Log(Timestamp);
        GetComponent<MeshRenderer>().enabled = true;

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
        if (other.gameObject.GetComponent<Brushable>())
        {
            Brushable otherTree = other.GetComponent<Brushable>();
            if (Timestamp > otherTree.Timestamp)
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
}