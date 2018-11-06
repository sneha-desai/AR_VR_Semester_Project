using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;
using ZoneCentric;
using Object = UnityEngine.Object;
using Random = System.Random;

public class PolarCoordinates
{
	public float radius;
	public float polar;
	public float elevation;
	
	public static PolarCoordinates FromCartesian(Vector3 cartesianCoordinate)
	{
		PolarCoordinates temp = new PolarCoordinates();
		temp.radius = cartesianCoordinate.magnitude;
            
		if (cartesianCoordinate.x == 0f)
			cartesianCoordinate.x = Mathf.Epsilon;

		temp.polar = Mathf.Atan(cartesianCoordinate.z / cartesianCoordinate.x);

		if (cartesianCoordinate.x < 0f)
			temp.polar += Mathf.PI;
            
		temp.elevation = Mathf.Asin(Mathf.Clamp(cartesianCoordinate.y / temp.radius, -1f, 1f));

		return temp;
	}

}

public class Bound : MonoBehaviour {
	
	public SphericalCoordinates sc;
	private GameObject avatar;
	private GameObject zoneSphere;
	private float radius;
	public float minTempPolar = 0f;
	public float maxTempPolar = 360f;
	public float minTempElevation = -80f;
	public float maxTempElevation = 80f;
	private SteamVR_TrackedObject _trackedObj;
	private const EVRButtonId TouchpadButton = EVRButtonId.k_EButton_SteamVR_Touchpad;
	public LineRenderer lineRenderer;
	public bool RotateAvatar = true;
	public Dictionary<int, Zones> ZONES;
	private int _currentNumPartition;
	private int _currentSubZone;
	private bool _ifInProcess;
	

	private SteamVR_Controller.Device Controller
	{

		get
		{
			return SteamVR_Controller.Input((int)_trackedObj.index);
		}
	}

	void Start()
	{
		avatar = GameObject.Find("HumanFigure");
		zoneSphere = GameObject.Find("ZoneSphere");
		radius = zoneSphere.GetComponent<Renderer>().bounds.extents.magnitude;
		
		Mesh mesh = zoneSphere.GetComponent<MeshFilter>().mesh;
		
		// Points wrt to the center of the sphere but scaled down to the scaling factor of the parent (i.e. HumanFigure)
		Vector3[] vertices = mesh.vertices;
		Debug.Log("Vertices Length: " + vertices.Length);
		
//		zoneSphere.transform.position = sc.toCartesian + avatar.transform.position;

		Debug.Log("Radius is: " + zoneSphere.GetComponent<Renderer>().bounds.extents.magnitude);
		Debug.Log(sc.toCartesian + " " + avatar.transform.position + " " + zoneSphere.transform.position + " " + zoneSphere.transform.TransformPoint(zoneSphere.transform.position));
		
//		// create new colors array where the colors will be created.
//		Color[] colors = new Color[vertices.Length];
//
//		for (int i = 0; i < vertices.Length; i = i + 1)
//		{
//			// assign the array of colors to the Mesh.
//			colors[i] = Color.Lerp(Color.red, Color.green, vertices[i].y);
//		}
//		mesh.colors = colors;

		HighlightSector(zoneSphere.GetComponent<MeshFilter>().mesh, 0f, 180f, 0f, 90f);
		ZONES = Interface.GetZones();
	}

	string PointToString(Vector3 point)
	{
		return (point.x + " " + point.y + " " + point.z);
	}
	
	void DrawLine()
	{
		
	}

	void DrawSphere(Vector3 center)
	{
		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.position = zoneSphere.transform.TransformPoint(center);
		sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		sphere.GetComponent<Renderer>().material.color = Color.Lerp(Color.magenta, Color.yellow, center.y);
		
	}

	void DrawTriangle(Vector3 pt1, Vector3 pt2, Vector3 pt3)
	{
		Debug.DrawLine(pt1, pt2, Color.green, 10000f);
		Debug.DrawLine(pt2, pt3, Color.green, 10000f);
		Debug.DrawLine(pt3, pt1, Color.green, 10000f);		
	}
	
	void DrawTriangles()
	{
		Mesh mesh = zoneSphere.GetComponent<MeshFilter>().mesh;

		for (int i = 0; i < mesh.triangles.Length; i = i + 3)
		{
			Vector3 p0 = zoneSphere.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 0]]);
			Vector3 p1 = zoneSphere.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 1]]);
			Vector3 p2 = zoneSphere.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 2]]);
			
			var temp0 = PolarCoordinates.FromCartesian(4f*p0);
			var temp1 = PolarCoordinates.FromCartesian(4f*p1);
			var temp2 = PolarCoordinates.FromCartesian(4f*p2);

			Debug.DrawLine(p0, p1, Color.green, 10000f);
			Debug.DrawLine(p1, p2, Color.green, 10000f);
			Debug.DrawLine(p2, p0, Color.green, 10000f);
		}

	}
		
	private void Awake()
	{
		_trackedObj = GetComponent<SteamVR_TrackedObject>();

	}

	
	void RotateObject(GameObject obj)
	{
		float temp = obj.transform.rotation.y;
		temp += 1;
//		avatar.transform.rotation.y = temp;
		obj.transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), 1.0f);
		
	}

	float RadianToDegree(float radian)
	{
		return radian * (360 / (Mathf.PI * 2f));
	}

	float DegreeToRadian(float degree)
	{
		return degree * ((Mathf.PI * 2f)/ 360f);
	}
	
	bool PointInSector(PolarCoordinates point, float minPolar, float maxPolar, float minElevation, float maxElevation)
	{
		float polar = RadianToDegree(point.polar);
		float elevation = RadianToDegree(point.elevation);
		Debug.Log(polar + " " + elevation);
		if ((polar >= minPolar) && (polar <= maxPolar) && (elevation >= minElevation) && (elevation <= maxElevation))
		{
			return true;
		}

		return false;
	}

	bool TriangleInSector(PolarCoordinates sc1, PolarCoordinates sc2, PolarCoordinates sc3, float minPolar, float maxPolar, float minElevation, float maxElevation)
	{
		bool one = PointInSector(sc1, minPolar, maxPolar, minElevation, maxElevation);
		bool two = PointInSector(sc1, minPolar, maxPolar, minElevation, maxElevation);
		bool three = PointInSector(sc1, minPolar, maxPolar, minElevation, maxElevation);

		bool cond = one ^ two ? three : one;
		
		if (cond)
		{
			Debug.Log("Spherical Coordinates (" + one + " " + two + " " + three + "): "+ RadianToDegree(sc1.polar) + " " + RadianToDegree(sc1.elevation) + " " +  RadianToDegree(sc2.polar) + " " + RadianToDegree(sc2.elevation) + " " + RadianToDegree(sc3.polar) + " " + RadianToDegree(sc3.elevation));
			return true;			
		}
		return false;
	}

	void HighlightSector(Mesh mesh, float minPolar, float maxPolar, float minElevation, float maxElevation)
	{
		List<int> points = new List<int>();
		for (int i = 0; i < mesh.triangles.Length; i = i + 3)
		{
//			Vector3 pt1 = zoneSphere.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 0]]);
//			Vector3 pt2 = zoneSphere.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 1]]);
//			Vector3 pt3 = zoneSphere.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 2]]);
//			
//			SphericalCoordinates sc1 = sc.FromCartesian(4f*(pt1));
//			SphericalCoordinates sc2 = sc.FromCartesian(4f*(pt2));
//			SphericalCoordinates sc3 = sc.FromCartesian(4f*(pt3));
//			
//			if (TriangleInSector(sc1, sc2, sc3, minPolar, maxPolar, minElevation, maxElevation))
//				DrawTriangle(pt1, pt2, pt3);
			
			
			Vector3 pt1 = mesh.vertices[mesh.triangles[i + 0]];
			Vector3 pt2 = mesh.vertices[mesh.triangles[i + 1]];
			Vector3 pt3 = mesh.vertices[mesh.triangles[i + 2]];
			
			PolarCoordinates sc1 = PolarCoordinates.FromCartesian(4f*pt1);
			PolarCoordinates sc2 = PolarCoordinates.FromCartesian(4f*pt2);
			PolarCoordinates sc3 = PolarCoordinates.FromCartesian(4f*pt3);
			
//			Debug.Log(PointToString(pt1) + " " + sc1.polar+ " " + RadianToDegree(sc1.polar) + " " + RadianToDegree(sc1.elevation));
//			Debug.Log(PointToString(pt2) + " " + sc2.polar+ " " + RadianToDegree(sc2.polar) + " " + RadianToDegree(sc2.elevation));
//			Debug.Log(PointToString(pt3) + " " + sc3.polar+ " " + RadianToDegree(sc3.polar) + " " + RadianToDegree(sc3.elevation));

//			Debug.Log(PointToString(pt1) + " " + sc1.polar + " " + sc1.elevation);
//			Debug.Log(PointToString(pt2) + " " + sc2.polar + " " + sc2.elevation);
//			Debug.Log(PointToString(pt3) + " " + sc3.polar + " " + sc3.elevation);

			
//			pt1 = (sc1.toCartesian)/4f;
//			pt2 = sc2.toCartesian/4f;
//			pt3 = sc3.toCartesian/4f;

			if (TriangleInSector(sc1, sc2, sc3, minPolar, maxPolar, minElevation, maxElevation))
			{
//				DrawTriangle(zoneSphere.transform.TransformPoint(pt1), zoneSphere.transform.TransformPoint(pt2), zoneSphere.transform.TransformPoint(pt3));
//				points.Add(zoneSphere.transform.TransformPoint(pt1));
//				points.Add(zoneSphere.transform.TransformPoint(pt2));
//				points.Add(zoneSphere.transform.TransformPoint(pt3));
				Debug.Log(PointToString(pt1) + " " + sc1.polar + " " + sc1.elevation);
				Debug.Log(PointToString(pt2) + " " + sc2.polar + " " + sc2.elevation);
				Debug.Log(PointToString(pt3) + " " + sc3.polar + " " + sc3.elevation);

				points.Add(mesh.triangles[i + 0]);
				points.Add(mesh.triangles[i + 1]);
				points.Add(mesh.triangles[i + 2]);
				
//				DrawSphere(pt1);
//				DrawSphere(pt2);
//				DrawSphere(pt3);
				
//				Debug.Log("####################Vertex: " + pos.x + " " + pos.y + " " + pos.z + " Spherical Coords: " + temp + "###########");
				PolarCoordinates sc4 = PolarCoordinates.FromCartesian(4f*(pt1));
				PolarCoordinates sc5 = PolarCoordinates.FromCartesian(4f*(pt2));
				PolarCoordinates sc6 = PolarCoordinates.FromCartesian(4f*(pt3));
				Debug.Log("Spherical Coordinates (" +  (i+0) + " " +  (i+1) + " " +  (i+2) + "): ("+ RadianToDegree(sc4.polar) + " " + RadianToDegree(sc4.elevation) + ") (" +  RadianToDegree(sc5.polar) + " " + RadianToDegree(sc5.elevation) + ") (" + RadianToDegree(sc6.polar) + " " + RadianToDegree(sc6.elevation) + ")");

				Debug.Log("Cartesian Points: " + PointToString(4f*pt1) + " " + PointToString(4f*pt2) + " " + PointToString(4f*pt3));

			}
		}
		mesh.subMeshCount = 2;
		mesh.SetTriangles(points, 1);
		Material[] materials = new Material[2];
		materials[0] = zoneSphere.GetComponent<MeshRenderer>().materials[0];
		Material tempMaterial = new Material(Shader.Find("Sprites/Default"));
		tempMaterial.color = Color.cyan;
		
		materials[1] = tempMaterial;
		zoneSphere.GetComponent<MeshRenderer>().materials = materials;
		
		Debug.Log("Submesh count: " + mesh.subMeshCount);
		Debug.Log("Materials length: " + zoneSphere.GetComponent<MeshRenderer>().materials.Count());
	
		foreach (int index in mesh.GetTriangles(1))
		{
			Debug.Log(PointToString(mesh.vertices[index]));
		}
	}

	void StartExperiment()
	{
		foreach (int numPartition in ZONES.Keys)
		{
			
		}
	}

	void ChangeZone()
	{
		Random rnd = new Random();
		int r = rnd.Next(ZONES[_currentNumPartition].Count);
		
	}

	void EndExperiment()
	{
		
	}

	void ChangePartition()
	{
		if ((_currentNumPartition + 1) < ZONES.Count) 
		{
          	_currentNumPartition += 1;
		}
		else
		{
			EndExperiment();
		}
	}

	// Update is called once per frame
	void Update () 
	{
		if (RotateAvatar) RotateObject(avatar);
		if (Controller.GetHairTriggerDown())
		{
			Mesh mesh = zoneSphere.GetComponent<MeshFilter>().mesh;
			Vector3[] vertices = mesh.vertices;
			int count = 0;
			for (int i = 0; i < vertices.Length; i = i + 1)
			{
				var temp = sc.FromCartesian(4f*(vertices[i]));
				var pos = zoneSphere.transform.TransformPoint(vertices[i]);
				var polar = RadianToDegree(temp.polar);
				var elevation = RadianToDegree(temp.elevation);
				if ((polar >= minTempPolar) && (polar <= maxTempPolar) && (elevation >= minTempElevation) && (elevation <= maxTempElevation))
				{
					GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					sphere.tag = "testSphere";
					sphere.transform.position = zoneSphere.transform.TransformPoint(vertices[i]);
					sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
					sphere.GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.green, vertices[i].y);
					count = count + 1;
				}
				else
				{
//					Debug.Log("Points not shown: " + radius + " " + polar + " " + elevation + " Coordinates: " + 4f*vertices[i].x + " " + 4f*vertices[i].y + " " + 4f*vertices[i].z);
					sc.FromCartesian(4f*(vertices[i]));
				}
			}
			Debug.Log("Points shown: " + count);

			ChangeZone();

		}

		if (Controller.GetPressDown(TouchpadButton))
		{
//			GameObject[] allObjects = GameObject.FindGameObjectsWithTag("testSphere");
//			foreach (GameObject obj in allObjects)
//			{
//				Destroy(obj);
//			}
//			if (_ifInProcess)
//			{
			ChangeNumPartition();
//			}
//			else
//			{
//				StartExperiment();				
//			}
		}
//		Mesh mesh = zoneSphere.GetComponent<MeshFilter>().mesh;
//		Vector3[] vertices = mesh.vertices;
//
//		for (int i = 0; i < vertices.Length; i = i + 1)
//		{
//			// assign the array of colors to the Mesh.
//			var temp = sc.FromCartesian(zoneSphere.transform.TransformPoint(vertices[i]));
//			var pos = zoneSphere.transform.TransformPoint(vertices[i]);
//			var polar = temp.polar * (360f / (Mathf.PI * 2f));
//			var elevation = temp.elevation * (360f / (Mathf.PI * 2f));
//			if ((polar >= minTempPolar) && (polar <= maxTempPolar) && (elevation >= minTempElevation) &&
//			    (elevation <= maxTempElevation))
//			{
//				Debug.Log("####################Vertex: " + pos.x + " " + pos.y + " " + pos.z + " Spherical Coords: " +
//				          temp + "###########");
//				GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//				sphere.tag = "testSphere";
//				sphere.transform.position = zoneSphere.transform.TransformPoint(vertices[i]);
//				sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
//				sphere.GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.green, vertices[i].y);
//			}
//		}
	}
}
