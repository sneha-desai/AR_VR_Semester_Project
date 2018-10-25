using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data {
	public int x;
	public int y;
	public int z;
	public int userId;
}

public class NetworkTest : MonoBehaviour {
	// Use this for initialization
	void Start () {
		NetworkManager networkManager = gameObject.AddComponent<NetworkManager>();
		
		var obj = new Data{
			x = 1,
			y = 2,
			z = 3,
			userId = 1234
		};

        networkManager.uploadData("/uploadUserData", JsonUtility.ToJson(obj));
	}
}
