using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ServerInterface : MonoBehaviour {

	string url = "https://us-central1-damsel-cb632.cloudfunctions.net/widgets/test";

	private void Start()
	{
//		SendGet();
		SendUserData("123","{test: true, name: \"Andre\"");
	}

	public void SendGet()
	{
		StartCoroutine(SendPostCoroutine());
	}

	public void SendUserData(String uid, String data)
	{
		StartCoroutine(SendUserDataCoroutine(uid, data));
	}

	IEnumerator SendUserDataCoroutine(String uid, String data)
	{
		WWWForm form = new WWWForm();
		form.AddField("uid", uid);
		form.AddField("data", data);

		using (UnityWebRequest www = UnityWebRequest.Post(url, form))
		{
			yield return www.Send();

			if (www.isNetworkError)
			{
				Debug.Log(www.error);
			}
			else
			{
				Debug.Log("GeT successful!");
				StringBuilder sb = new StringBuilder();
				foreach (System.Collections.Generic.KeyValuePair<string, string> dict in www.GetResponseHeaders())
				{
					sb.Append(dict.Key).Append(": \t[").Append(dict.Value).Append("]\n");
				}

				// Print Headers
				Debug.Log(sb.ToString());

				// Print Body
				Debug.Log(www.downloadHandler.text);
			}
		}        
	}

	IEnumerator SendPostCoroutine()
	{
		WWWForm form = new WWWForm();
		form.AddField("valueOne", "some stuff");

		using (UnityWebRequest www = UnityWebRequest.Get(url))
		{
			yield return www.Send();

			if (www.isNetworkError)
			{
				Debug.Log(www.error);
			}
			else
			{
				Debug.Log("GeT successful!");
				StringBuilder sb = new StringBuilder();
				foreach (System.Collections.Generic.KeyValuePair<string, string> dict in www.GetResponseHeaders())
				{
					sb.Append(dict.Key).Append(": \t[").Append(dict.Value).Append("]\n");
				}

				// Print Headers
				Debug.Log(sb.ToString());

				// Print Body
				Debug.Log(www.downloadHandler.text);
			}
		}        
	}
}
