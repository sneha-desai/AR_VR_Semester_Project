using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour {
    public string serverRoot = "http://localhost:1337/";

	public void uploadData(string uri, string serializedData) {
		StartCoroutine(UploadData(uri, serializedData));
	}

	IEnumerator UploadData(string uri, string serializedData) {
		string url = serverRoot + uri;
        WWWForm formData = new WWWForm();
        formData.AddField("data", serializedData);
        UnityWebRequest uwrq = UnityWebRequest.Post(url, formData);
        // uwrq.SetRequestHeader("Content-Type", "application/json");
        Debug.Log(formData);
        yield return uwrq.SendWebRequest();
        if (uwrq.isNetworkError || uwrq.isHttpError) {
        	Debug.Log("HTTP/Network error");
        	Debug.Log(uwrq.error);
        } else {
        	Debug.Log(uwrq.downloadHandler.text);
        }
    }
}
