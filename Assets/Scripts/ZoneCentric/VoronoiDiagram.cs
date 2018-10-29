using UnityEngine;

namespace ZoneCentric
{
	public class VoronoiDiagram : MonoBehaviour {
		private SteamVR_TrackedObject _trackedObj;
	
		private SteamVR_Controller.Device Controller
		{

			get
			{
				return SteamVR_Controller.Input((int)_trackedObj.index);
			}
		}

		void Start()
		{
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.transform.position = new Vector3(0, 0.5f, 0);
		}
	
		void Awake()
		{
			_trackedObj = GetComponent<SteamVR_TrackedObject>();
		}

		void OnGUI ()
		{
			Rect textBox = new Rect(10, 120, 500, 100);
			Vector3 pos = _trackedObj.transform.position;
		
			GUI.Label (textBox, "(X, Y, Z): (" + pos.x + " " + pos.y + " " + pos.z + ")");
		}

		// Update is called once per frame
		void Update () {
			
			// Objects appear to init position 
			if (Controller.GetHairTriggerDown())
			{
				Vector3 pos = _trackedObj.transform.position;
				Debug.Log("(X, Y, Z): (" + pos.x + " " + pos.y + " " + pos.z + ")");
			}

		}
	
	}
}
