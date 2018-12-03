using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneDictionary : MonoBehaviour {
    private static Dictionary<int, Transform[]> zoneToPrefabs = new Dictionary<int, Transform[]> {
            { 0, new Transform[] { Resources.Load<Transform>("Trees/Poplar_tree"), Resources.Load<Transform>("Trees/Oak_tree"), Resources.Load<Transform>("Trees/Palm_tree"), Resources.Load<Transform>("Trees/Fir_tree") } },
            { 1, new Transform[] { Resources.Load<Transform>("Trees/Poplar_tree"), Resources.Load<Transform>("Trees/Oak_tree"), Resources.Load<Transform>("Trees/Palm_tree"), Resources.Load<Transform>("Trees/Fir_tree") }  },
            { 2, new Transform[] { Resources.Load<Transform>("Trees/Poplar_tree"), Resources.Load<Transform>("Trees/Oak_tree"), Resources.Load<Transform>("Trees/Palm_tree"), Resources.Load<Transform>("Trees/Fir_tree") }  },
            { 3, new Transform[] { Resources.Load<Transform>("Trees/Poplar_tree"), Resources.Load<Transform>("Trees/Oak_tree"), Resources.Load<Transform>("Trees/Palm_tree"), Resources.Load<Transform>("Trees/Fir_tree") }  }
        };

    private static Dictionary<int, string> zoneToTypes = new Dictionary<int, string> {
            { 0, "drawable" },
            { 1, "drawable" },
            { 2, "environment" },
            { 3, "environment" }
        };

    public static Transform[] GetPrefabArrayForZone (int zone)
    {
        if (zoneToPrefabs.ContainsKey(zone) == true)
        {
            return zoneToPrefabs[zone];
        }

        return null;
    }

    public static string GetTypeForZone (int zone)
    {
        if (zoneToTypes.ContainsKey(zone) == true)
        {
            return zoneToTypes[zone];
        }

        return null;
    }
}
