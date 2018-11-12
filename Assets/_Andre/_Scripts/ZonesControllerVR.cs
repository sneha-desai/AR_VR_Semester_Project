using UnityEngine;

namespace _Andre._Scripts
{
	public class ZonesControllerVR : MonoBehaviour {
		public Color OriginalColor = new Color(1,1,1);
		public Color HighlightColor = new Color(1,0.3f,0.3f);

		public int SelectedCount = 0;
	
		// Use this for initialization
		void Start () {
			ZoneVR[] zones = FindObjectsOfType(typeof(ZoneVR)) as ZoneVR[];
			foreach (ZoneVR zone in zones)
			{
				zone.OriginalColor = OriginalColor;
				zone.HighlightColor = HighlightColor;
				zone.OnSelect += ZoneSelected;
//			zone.OnClick += ZoneSelected;
			}
		}
	
		// Update is called once per frame
		void Update () {
		
		}

		void ZoneSelected(Zone zone)
		{
			Debug.Log("ZoneSelected");
			SelectedCount++;
			Debug.Log(zone.transform);
		}
	}
}
