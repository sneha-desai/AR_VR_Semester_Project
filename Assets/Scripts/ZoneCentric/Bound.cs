using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Random = System.Random;

namespace ZoneCentric
{
	public class PolarCoordinates
	{ 
		public float Radius;
		public float Polar;
		public float Elevation;

		public static PolarCoordinates FromCartesian(Vector3 cartesianCoordinate)
		{
			PolarCoordinates temp = new PolarCoordinates {Radius = cartesianCoordinate.magnitude};

			if (cartesianCoordinate.x == 0f)
				cartesianCoordinate.x = Mathf.Epsilon;

			temp.Polar = Mathf.Atan(cartesianCoordinate.z / cartesianCoordinate.x);

			if (cartesianCoordinate.x < 0f)
				temp.Polar += Mathf.PI;

			if ((cartesianCoordinate.z < 0f) && (cartesianCoordinate.x > 0f))
				temp.Polar += Mathf.PI * 2f;
            
			temp.Elevation = Mathf.Asin(Mathf.Clamp(cartesianCoordinate.y / temp.Radius, -1f, 1f));

			return temp;
		}

	}

	public class Bound : MonoBehaviour {
	
		private static GameObject _avatar;
		private static GameObject _zoneSphere;
		private static GameObject _camera;
		private static float _radius;
		public static float MinTempPolar = 0f;
		public static  float MaxTempPolar = 360f;
		public static float MinTempElevation = -80f;
		public static float MaxTempElevation = 80f;
		public SteamVR_TrackedObject _trackedObj;
		public const EVRButtonId TouchpadButton = EVRButtonId.k_EButton_SteamVR_Touchpad;
		public const EVRButtonId GripButton = EVRButtonId.k_EButton_Grip;
		public LineRenderer lineRenderer;
		public static bool RotateAvatar = true;
		public static bool DebugMode = false;
		public static List<Zones> ZONES;
		private static GameObject _human;
		private static Human HUMAN;
		public TextMesh Instruction;

		// Index in the ZONES array. It signifies the index to the level currently being experimented
		private static int _currentLevelIndex;

		// i^th subzone from _randZones being experimented for _currentLevelIndex^th level. If there are two subzones then it can take values {0, 1}
		private static int _currentRandZonesIndex;

		private static bool _levelInProgress;
		private static bool _ExperimentInProgress;
		
		// Contains the shuffled list of possible values of _currentRandZonesIndex
		private static int[] _randZones;
		
		private static Mesh _sphereMesh;

		private SteamVR_Controller.Device Controller
		{

			get
			{
				return SteamVR_Controller.Input((int)_trackedObj.index);
			}
		}

		private void Start()
		{
			_avatar = GameObject.Find("Avatar");
			_zoneSphere = GameObject.Find("ZoneSphere");
			_camera = GameObject.Find("Camera (eye)");
			_radius = _zoneSphere.GetComponent<Renderer>().bounds.extents.magnitude;
		
			_sphereMesh = _zoneSphere.GetComponent<MeshFilter>().mesh;
		
			// Points wrt to the center of the sphere but scaled down to the scaling factor of the parent (i.e. HumanFigure)
			Vector3[] vertices = _sphereMesh.vertices;
			Debug.Log("Vertices Length: " + vertices.Length);
		
//		zoneSphere.transform.position = sc.toCartesian + avatar.transform.position;

			Debug.Log("Radius is: " + _zoneSphere.GetComponent<Renderer>().bounds.extents.magnitude);
		
//		// create new colors array where the colors will be created.
//		Color[] colors = new Color[vertices.Length];
//
//		for (int i = 0; i < vertices.Length; i = i + 1)
//		{
//			// assign the array of colors to the Mesh.
//			colors[i] = Color.Lerp(Color.red, Color.green, vertices[i].y);
//		}
//		mesh.colors = colors;
			ZONES = Interface.GetZones();
			_currentLevelIndex = -1;
			_levelInProgress = false;
//			_randZones = InitRandSubZoneArray();
			_human = GameObject.Find("Human");
			HUMAN = _human.GetComponent<Human>();
			Instruction = GameObject.Find("Instruction").GetComponent<TextMesh>();

		}

		string PointToString(Vector3 point)
		{
			return (point.x + " " + point.y + " " + point.z);
		}
	
		void DrawSphere(Vector3 center)
		{
			GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			sphere.transform.position = _zoneSphere.transform.TransformPoint(center);
			sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			sphere.GetComponent<Renderer>().material.color = Color.Lerp(Color.magenta, Color.yellow, center.y);
		
		}

		private void DrawTriangle(Vector3 pt1, Vector3 pt2, Vector3 pt3)
		{
			Debug.DrawLine(pt1, pt2, Color.green, 10000f);
			Debug.DrawLine(pt2, pt3, Color.green, 10000f);
			Debug.DrawLine(pt3, pt1, Color.green, 10000f);		
		}

		private void DrawTriangles(Mesh mesh)
		{
			for (var i = 0; i < mesh.triangles.Length; i = i + 3)
			{
				var p0 = _zoneSphere.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 0]]);
				var p1 = _zoneSphere.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 1]]);
				var p2 = _zoneSphere.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 2]]);
			
				Debug.DrawLine(p0, p1, Color.green, 10000f);
				Debug.DrawLine(p1, p2, Color.green, 10000f);
				Debug.DrawLine(p2, p0, Color.green, 10000f);
			}

		}
		
		private void Awake()
		{
			_trackedObj = GetComponent<SteamVR_TrackedObject>();
		}


		private static void RotateObject(GameObject obj)
		{
			obj.transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), 1.0f);
		}

		private static float RadianToDegree(float radian)
		{
			return radian * (360 / (Mathf.PI * 2f));
		}

		private static float DegreeToRadian(float degree)
		{
			return degree * ((Mathf.PI * 2f)/ 360f);
		}

		private static bool PointInSector(PolarCoordinates point, float minPolar, float maxPolar, float minElevation, float maxElevation)
		{
			var polar = RadianToDegree(point.Polar);
			var elevation = RadianToDegree(point.Elevation);
//			Debug.Log(polar + " " + elevation);
			return (polar >= minPolar) && (polar <= maxPolar) && (elevation >= minElevation) && (elevation <= maxElevation);
		}

		private static bool TriangleInSector(PolarCoordinates sc1, PolarCoordinates sc2, PolarCoordinates sc3, float minPolar, float maxPolar, float minElevation, float maxElevation)
		{
			var one = PointInSector(sc1, minPolar, maxPolar, minElevation, maxElevation);
			var two = PointInSector(sc1, minPolar, maxPolar, minElevation, maxElevation);
			var three = PointInSector(sc1, minPolar, maxPolar, minElevation, maxElevation);

			var cond = one ^ two ? three : one;
		
			return cond;
		}

		private void HighlightSector(Mesh mesh, float minPolar, float maxPolar, float minElevation, float maxElevation)
		{
			var points = new List<int>();
			for (var i = 0; i < mesh.triangles.Length; i = i + 3)
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
			
				var pt1 = mesh.vertices[mesh.triangles[i + 0]];
				var pt2 = mesh.vertices[mesh.triangles[i + 1]];
				var pt3 = mesh.vertices[mesh.triangles[i + 2]];
			
				var sc1 = PolarCoordinates.FromCartesian(4f*pt1);
				var sc2 = PolarCoordinates.FromCartesian(4f*pt2);
				var sc3 = PolarCoordinates.FromCartesian(4f*pt3);

				if (TriangleInSector(sc1, sc2, sc3, minPolar, maxPolar, minElevation, maxElevation))
				{
					points.Add(mesh.triangles[i + 0]);
					points.Add(mesh.triangles[i + 1]);
					points.Add(mesh.triangles[i + 2]);
				
//				Debug.Log("####################Vertex: " + pos.x + " " + pos.y + " " + pos.z + " Spherical Coords: " + temp + "###########");
					var sc4 = PolarCoordinates.FromCartesian(4f*(pt1));
					var sc5 = PolarCoordinates.FromCartesian(4f*(pt2));
					var sc6 = PolarCoordinates.FromCartesian(4f*(pt3));
//				Debug.Log("Spherical Coordinates (" +  (i+0) + " " +  (i+1) + " " +  (i+2) + "): ("+ RadianToDegree(sc4.polar) + " " + RadianToDegree(sc4.elevation) + ") (" +  RadianToDegree(sc5.polar) + " " + RadianToDegree(sc5.elevation) + ") (" + RadianToDegree(sc6.polar) + " " + RadianToDegree(sc6.elevation) + ")");

//				Debug.Log("Cartesian Points: " + PointToString(4f*pt1) + " " + PointToString(4f*pt2) + " " + PointToString(4f*pt3));

				}
			}
			mesh.subMeshCount = 2;
			mesh.SetTriangles(points, 1);
			var materials = new Material[2];
			materials[0] = _zoneSphere.GetComponent<MeshRenderer>().materials[0];
			var tempMaterial = new Material(Shader.Find("Sprites/Default")) {color = Color.cyan};

			materials[1] = tempMaterial;
			_zoneSphere.GetComponent<MeshRenderer>().materials = materials;
		}

		private void StartLevel()
		{
			_currentRandZonesIndex = 0;
			_levelInProgress = true;
		}

		private float[] GetRangeOfZone()
		{
			Debug.Log("Current Level Index : " + _currentLevelIndex);
			Debug.Log("Current Rand Zones Index : " + _currentRandZonesIndex);

			var subZone = ZONES[_currentLevelIndex].SubZones;
			var index = 4 * _randZones[_currentRandZonesIndex];
			var range = new float[] {subZone[index + 0], subZone[index + 1], subZone[index + 2], subZone[index + 3]};
			return range;
		}
		
		private void DisplaySubZone()
		{
			var range = GetRangeOfZone();
			Debug.Log(range[0] + " " + range[1] + " " + range[2] + " " + range[3]);
			HighlightSector(_sphereMesh, range[0], range[1], range[2], range[3]);
		}

		private void ChangeSubZone()
		{
			_currentRandZonesIndex += 1;
		}

		private int[] InitRandSubZoneArray()
		{
			var randArray = Enumerable.Range(0, ZONES[_currentLevelIndex].NumSubZones).ToArray();
			randArray = ShuffleArray(randArray);
			return randArray;
		}

		private static int[] ShuffleArray(int[] array)
		{
			var r = new Random();
			for (var i = array.Length; i > 0; i--)
			{
				var j = r.Next(i);
				array[i - 1] = array[j] + array[i - 1] - (array[j] = array[i - 1]);
			}
			return array;
		
		}

		private void ResetExperiment()
		{
			_currentRandZonesIndex = 0;
			_currentLevelIndex = -1;
			_levelInProgress = false;
			_randZones	= new int[] {};
		}
		
		private void EndExperiment()
		{
			Debug.Log("CONGRATULATIONS! You are free to go!");
			Instruction.text = "CONGRATULATIONS! You are free to go!";
			_ExperimentInProgress = false;
		}

		private void NextLevel()
		{
			_currentLevelIndex += 1;
		}

		// Update is called once per frame
		private void Update () 
		{
			if (RotateAvatar) RotateObject(_avatar);
//			Debug.Log(HUMAN.is);
			if (HUMAN.IsCalibrated())
			{
				if (!_ExperimentInProgress)
				{
					ResetExperiment();
				}

			}
			
			if (Controller.GetHairTriggerDown())
			{
				if (_ExperimentInProgress)
				{
					if (_currentRandZonesIndex != _randZones.Length)
					{
						var range = GetRangeOfZone();

						// correction function
						var transformedClickedPoint =
							_human.transform.InverseTransformPoint(_trackedObj.transform.position);
						var isCorrect = PointInSector(PolarCoordinates.FromCartesian(transformedClickedPoint),
							range[0], range[1], range[2], range[3]);
						if (isCorrect)
						{
							Debug.Log("IsCorrect");
							Instruction.text = "You are Correct";
						}
						else
						{
							Debug.Log("Watch your step");
							Instruction.text = "Watch your step";
						}
						
					}
					
					ChangeSubZone();

					if (!(_currentRandZonesIndex < _randZones.Length))
					{
						if (_levelInProgress)
						{
							_levelInProgress = false;
						}
						else
						{
							_randZones = InitRandSubZoneArray();
							StartLevel();
						}
					}

					if (_levelInProgress)
					{
						DisplaySubZone();					
						Debug.Log("Level: " + _currentLevelIndex + " SubZone " + _currentRandZonesIndex);
					}
					else
					{
						Debug.Log("Press TouchPad to move to next level. Press Trigger to restart the current level.");
						Instruction.text = "Press TouchPad to move to next level." + Environment.NewLine + "Press Trigger to restart the current level.";
					}
					
				}
				else
				{
//						Debug.Log("Camera position wrt human: " + _human.transform.InverseTransformPoint(_camera.transform.position));
//						Debug.Log("Human Position wrt human: " + _human.transform.InverseTransformPoint(_human.transform.position));
//						Debug.Log("transformed point: " + _human.transform.InverseTransformPoint(_trackedObj.transform.position));
						HUMAN.Calibrate(new Vector3(_camera.transform.position.x, _trackedObj.transform.position.y, _camera.transform.position.z));
				}

			}

			if (Controller.GetPressDown(TouchpadButton))
			{
				if (!_ExperimentInProgress) _ExperimentInProgress = true;
				
				if (DebugMode)
				{
					GameObject[] allObjects = GameObject.FindGameObjectsWithTag("testSphere");
					foreach (GameObject obj in allObjects)
					{
						Destroy(obj);
					}					
				}
				else
				{
					if (_currentRandZonesIndex < _randZones.Length)
					{
						if (_levelInProgress)
						{
							Debug.Log("Please complete the current level");
							Instruction.text = "Please complete the current level";
						}
						else
						{
							Debug.Log("Level not in progress.");
							Instruction.text = "Level not in progress.";
						}
					}
					else
					{
						if ((_currentLevelIndex + 1 ) < ZONES.Count)
						{
							NextLevel();
							StartLevel();
							_randZones = InitRandSubZoneArray();
							DisplaySubZone();
							Debug.Log("Moved to next level! " + _currentLevelIndex);
//							Instruction.text = "Moved to next level! " + _currentLevelIndex;
						}
						else
						{
							EndExperiment();
						}				
					}
				}
			}

			if (Controller.GetPressDown(GripButton) && DebugMode)
			{
				Vector3[] vertices = _sphereMesh.vertices;
				int count = 0;
				for (int i = 0; i < vertices.Length; i = i + 1)
				{
					var temp = PolarCoordinates.FromCartesian(4f*(vertices[i]));
					var pos = _zoneSphere.transform.TransformPoint(vertices[i]);
					var polar = RadianToDegree(temp.Polar);
					var elevation = RadianToDegree(temp.Elevation);
//					Debug.Log(polar + " " + elevation);
					if ((polar >= MinTempPolar) && (polar <= MaxTempPolar) && (elevation >= MinTempElevation) && (elevation <= MaxTempElevation))
					{
						GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						sphere.tag = "testSphere";
						sphere.transform.position = _zoneSphere.transform.TransformPoint(vertices[i]);
						sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
						sphere.GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.green, vertices[i].y);
						count = count + 1;
					}
					else
					{
					Debug.Log("Points not shown: " + polar + " " + elevation + " Coordinates: " + 4f*vertices[i].x + " " + 4f*vertices[i].y + " " + 4f*vertices[i].z);
						PolarCoordinates.FromCartesian(4f*(vertices[i]));
					}
				}
				Debug.Log("Points shown: " + count);
			}
		}
	}
}