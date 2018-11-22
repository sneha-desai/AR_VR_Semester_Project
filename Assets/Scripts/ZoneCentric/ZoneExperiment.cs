using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Random = System.Random;

namespace ZoneCentric
{
	public class ZoneExperiment : MonoBehaviour {
		private static bool _experimentInProgress;
		private static bool _levelInProgress;
		private static Experiment _experiment;
		private static bool _ifRestart;
		public static bool RotateAvatar = true;

		public SteamVR_TrackedObject ControllerObject;
		private const EVRButtonId TouchpadButton = EVRButtonId.k_EButton_SteamVR_Touchpad;
		private const EVRButtonId GripButton = EVRButtonId.k_EButton_Grip;

		private static User _user;
		private static GameObject _userGameObject;
		private static GameObject _cameraGameObject;
		private static GameObject _zoneSphere;
		private static GameObject _currentSphere;
		private static GameObject _avatar;

		private static List<Zones> _zoneCentricZones;
		private static List<GameObject> _avatarSpheres;
		private static List<Vector3> _objectCentricZones;
		public TextMesh Instruction;

		private static int _currentLevelIndex;
		private static int _currentSubZone;
		private static int _numCorrect;
		private static int _numAttempted;
		private static int _lifeRemaining;
		private static float _timer;
		private static readonly Random Rnd = new Random();

		private const int Threshold = 10;
		private const int MaxRetry = 2;
		private const string Space = " ";
		private const string AvatarSphereTag = "AvatarSpheres";
		private const string TestSphereTag = "TestSpheres";
		private const float TimeLimit = 40.0f;

		private enum Experiment
		{
			NONE,
			ZONE,
			OBJECT
		};

		private SteamVR_Controller.Device Controller
		{

			get
			{
				return SteamVR_Controller.Input((int)ControllerObject.index);
			}
		}


		
		// Use this for initialization
		private void Start () {
			_userGameObject = GameObject.Find("User");
			_zoneSphere = GameObject.Find("ZoneSphere");
			Instruction = GameObject.Find("Instruction").GetComponent<TextMesh>();
			_cameraGameObject = GameObject.Find("Camera (eye)");
			_avatar = GameObject.Find("Avatar");
			_user = _userGameObject.GetComponent<User>();

			_objectCentricZones = new List<Vector3>();
			_avatarSpheres = new List<GameObject>();
			_zoneCentricZones = Interface.GetZones();

			_currentLevelIndex = 0;
			_currentSubZone = 0;
			_timer = TimeLimit;
			_numCorrect = 0;
			_numAttempted = 0;
			_experiment = Experiment.NONE;
			_lifeRemaining = MaxRetry;
			
			Instruction.text =
				"To start, press LEFT on Touchpad for Zone Centric and RIGHT for Object Centric Experiment";
		}

		private void Awake()
		{
			ControllerObject = GetComponent<SteamVR_TrackedObject>();
		}

		private static GameObject DrawSphere(Vector3 center, Transform parentTransform = null, string tag = "")
		{
			GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			if (parentTransform)
				sphere.transform.parent = parentTransform;
			if (!string.IsNullOrEmpty(tag))
				sphere.tag = tag;
			sphere.transform.position = center;
			sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

			sphere.GetComponent<Renderer>().material.color = Color.Lerp(Color.magenta, Color.yellow, center.y);
			return sphere;
		}

		private static void RotateObject(GameObject obj)
		{
			obj.transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), 1.0f);
		}

		
		private static void DestroySphereWithTag(string tag)
		{
			foreach (var obj in GameObject.FindGameObjectsWithTag(tag))
			{
				Destroy(obj);				
			}
		}

		private static bool IfQualify()
		{
			if (_numAttempted.Equals(0))
				return false;
			
			var one = (_numCorrect/(float)_numAttempted) >= 0.9f;
			var two = _numCorrect >= Threshold;
			var three = _timer <= 0.0f;

			return one && two && three;
		}

		private static void IsCorrect(Vector3 clickedPoint)
		{
			switch (_experiment)
			{
				case Experiment.ZONE:
				{
					if (CheckPointInZone(clickedPoint))
					{
						_numCorrect += 1;
					}

					break;
				}
				case Experiment.OBJECT:
				{
					if (CheckNearestZone(clickedPoint))
					{
						_numCorrect += 1;
					}

					break;
				}
				case Experiment.NONE:
					Debug.Log("IsCorrect: Experiment.NONE detected!");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			_numAttempted += 1;
		}

		private static bool CheckNearestZone(Vector3 clickedPoint)
		{
			var minDist = float.PositiveInfinity;
			var idx = 0;
			var nearestIndex = 0;

			foreach (var t in _objectCentricZones)
			{
				var dist = Vector3.Distance(t, clickedPoint);
				if (dist < minDist)
				{
					minDist = dist;
					nearestIndex = idx;
				}

				idx += 1;
			}
			return _currentSubZone == nearestIndex;
		}

		private static void ResetExperiment()
		{
			_levelInProgress = false;
			_experimentInProgress = false;
			_ifRestart = false;
			_currentLevelIndex = 0;
			_user.Reset();
			_experiment = Experiment.NONE;
			_objectCentricZones.Clear();
			_avatarSpheres.Clear();
			_lifeRemaining = MaxRetry;
			_currentSubZone = 0;
			_timer = TimeLimit;
			_numCorrect = 0;
			_numAttempted = 0;
			DestroySphereWithTag(AvatarSphereTag);
			DestroySphereWithTag(TestSphereTag);
		}

		private static bool CheckPointInZone(Vector3 cartesianPoint)
		{
			List<float> _subzones = _zoneCentricZones[_currentLevelIndex].SubZones;
			float minPolar = _subzones[4 * _currentSubZone + 0];
			float maxPolar = _subzones[4 * _currentSubZone + 1];
			float minElevation = _subzones[4 * _currentSubZone + 2];
			float maxElevation = _subzones[4 * _currentSubZone + 3];

			var polarPoint = PolarCoordinates.FromCartesian(cartesianPoint);
			
			var polar = PolarCoordinates.RadToDeg(polarPoint.Polar);
			var elevation = PolarCoordinates.RadToDeg(polarPoint.Elevation);

			
			// Condition when subzone traverses from fourth quadrant to first quadrant;
			if (maxPolar < minPolar)
			{
				minPolar -= 360f;
				if (polar >= 0f && polar <= maxPolar)
					return (elevation >= minElevation) && (elevation <= maxElevation);

				polar -= 360f;
				if (polar >= minPolar && polar <= 0f)
				{
					return (elevation >= minElevation) && (elevation <= maxElevation);
				}
				return false;
			}
			
			
			return (polar >= minPolar) && (polar <= maxPolar) && (elevation >= minElevation) && (elevation <= maxElevation);
		}

		private Experiment ChooseExperiment()
		{
			if (!Controller.GetPressDown(TouchpadButton)) return _experiment;
			var touchpad = (Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
			if (touchpad.x < 0)
			{
				_currentLevelIndex = 0;
				return Experiment.ZONE;
			}
			else
			{
				_currentLevelIndex = 5;
				return Experiment.OBJECT;
			}
		}
		
		private static void StartLevel()
		{
			_timer = TimeLimit;
			_levelInProgress = true;
			_numCorrect = 0;
			_numAttempted = 0;
		}

		private static void RestartLevel()
		{
			_levelInProgress = false;
			DestroySphereWithTag(AvatarSphereTag);
			_objectCentricZones.Clear();
			_avatarSpheres.Clear();
			_ifRestart = true;
			_lifeRemaining--;
		}
		
		private static void NextLevel()
		{
			_currentLevelIndex++;
			_levelInProgress = false;
			DestroySphereWithTag(AvatarSphereTag);
			_objectCentricZones.Clear();
			_avatarSpheres.Clear();
			_lifeRemaining = MaxRetry;
			_ifRestart = false;
		}

		private static Vector3 GenerateSpherePlacement(int currentLevelIndex)
		{
			var subzones = _zoneCentricZones[currentLevelIndex].SubZones;
			_currentSubZone = Rnd.Next(subzones.Count/4);
			var rndFloatPolar = (float) Rnd.NextDouble();
			var rndFloatElevation = (float) Rnd.NextDouble();
			var polarRange = subzones[4 * _currentSubZone + 1] - subzones[4 * _currentSubZone + 0];
			var elevationRange = subzones[4 * _currentSubZone + 3] - subzones[4 * _currentSubZone + 2];
			var polar = (subzones[4*_currentSubZone + 0] + 0.45f*polarRange) + (0.1f*polarRange)*rndFloatPolar;
			var elevation = (subzones[4*_currentSubZone + 2] + 0.45f*elevationRange) + (0.1f*elevationRange)*rndFloatElevation;
			return PolarCoordinates.ToCartesian(new PolarCoordinates(0.5f, PolarCoordinates.DegToRad(polar), PolarCoordinates.DegToRad(elevation)));
		}

		private static void CreateZones(Vector3 clickedPoint)
		{
			var polarCoordinates = PolarCoordinates.FromCartesian(_user.transform.InverseTransformPoint(clickedPoint));
			polarCoordinates.Radius = 0.5f;
			_objectCentricZones.Add(clickedPoint);
			_avatarSpheres.Add(DrawSphere(_zoneSphere.transform.TransformPoint(PolarCoordinates.ToCartesian(polarCoordinates)), _zoneSphere.transform, AvatarSphereTag));

			//Debug
//			DrawSphere(clickedPoint).GetComponent<Renderer>().material.color = Color.black;
		}

		private static void NextZone()
		{
			_currentSubZone = Rnd.Next(_objectCentricZones.Count);
			_currentSphere = _avatarSpheres[_currentSubZone];
			_currentSphere.GetComponent<Renderer>().enabled = true;
		}



		private void ObjectCentricExperiment()
		{
			if (!_levelInProgress)
			{
				if (_objectCentricZones.Count == _currentLevelIndex)
				{
					Instruction.text = "Yay! Zones created! Press the touchpad to start the level.";
				}
				else
				{
					if (_objectCentricZones.Count == 0)
					{
						Instruction.text = "Press the trigger at any point where you wish to place an object.";
						if (_ifRestart)
						{
							Instruction.text = "Sorry! Try again. " + Instruction.text;
						}
						else
						{
							if(_currentLevelIndex > 5)
								Instruction.text = "Yay! Level Cleared!!! " + Instruction.text;
						}
					}
					else
					{
						Instruction.text = (_currentLevelIndex - _objectCentricZones.Count) + " more to go...";
					}
				}
				
				if (_objectCentricZones.Count == _currentLevelIndex)
				{
					if (!Controller.GetPressDown(TouchpadButton)) return;
					foreach (var variable in GameObject.FindGameObjectsWithTag(AvatarSphereTag))
					{
						variable.GetComponent<Renderer>().enabled = false;
					}
					StartLevel();
					NextZone();
				}
				else
				{
					if (!Controller.GetHairTriggerDown()) return;
					CreateZones(ControllerObject.transform.position);
				}
			}
			else
			{
				_timer -= Time.deltaTime;
				Instruction.text = string.Format("Time: {0:0.00} Num Correct: {1} Num Attempted: {2}", _timer, _numCorrect, _numAttempted);
				
				if (_timer > 0)
				{
					if (!Controller.GetHairTriggerDown()) return;
					var clickedPoint = ControllerObject.transform.position;
					IsCorrect(clickedPoint);
					_currentSphere.GetComponent<Renderer>().enabled = false;
					NextZone();
				}
				else
				{
					if (IfQualify())
					{
						Debug.Log("YOU QUALIFY!!!!!!!!!!!!!!");
						NextLevel();
					}
					else
					{
						RestartLevel();
						if (_lifeRemaining <= 0)
						{
							Instruction.text = "GAME OVER! To restart, press LEFT for ZoneCentric and RIGHT for ObjectCentric on Touchpad";
							Debug.Log("Game Over");
							ResetExperiment();
							return;
						}
						Debug.Log("Restart Level");		
					}					
				}
			}
			
			
			
		}
		
		private void ZoneCentricExperiment()
		{
			if (!_levelInProgress)
			{
				// Purpose of if-else condition is to display the appropriate message
				if (_currentLevelIndex == 0) {
					Instruction.text = "Press the touchpad to start the level!";
				}
				else if(_currentLevelIndex < _zoneCentricZones.Count)
				{
					Instruction.text = "Congratulations! You are moving on to next level! Press the touchpad to start.";
				}
				else
				{
					Instruction.text = "Congrats! Experiment Completed! Press LEFT for ZoneCentric and RIGHT for ObjectCentric on Touchpad";
				}

				if (_currentLevelIndex < _zoneCentricZones.Count)
				{
					if (!Controller.GetPressDown(TouchpadButton)) return;
					StartLevel();
					_currentSphere = DrawSphere(_zoneSphere.transform.TransformPoint(GenerateSpherePlacement(_currentLevelIndex)), _zoneSphere.transform, TestSphereTag);
//							DrawSphere(PolarCoordinates.ToCartesian(new PolarCoordinates(0.5f, 90f, 0f)));
				}
				else
				{
					ResetExperiment();						
				}
			}
			else
			{
				_timer -= Time.deltaTime;
				Instruction.text = string.Format("Time: {0:0.00} Num Correct: {1} Num Attempted: {2}", _timer, _numCorrect, _numAttempted);

				if (IfQualify())
				{
					Debug.Log("YOU QUALIFY!!!!!!!!!!!!!!");
					NextLevel();
					DestroySphereWithTag(TestSphereTag);
				}
				else
				{
					if (!Controller.GetHairTriggerDown()) return;
					var clickedPoint = _user.transform.InverseTransformPoint(ControllerObject.transform.position);
					IsCorrect(clickedPoint);
					_currentSphere.transform.position = _zoneSphere.transform.TransformPoint(GenerateSpherePlacement(_currentLevelIndex));
				}
			}
		}
		
		// Update is called once per frame
		private void Update ()
		{
//			if (RotateAvatar) RotateObject(_avatar);

			if (!_experimentInProgress)
			{
//				Instruction.text = "Welcome to Proprioceptive Experiment!";
				_experiment = ChooseExperiment(); 
			}

			if (_experimentInProgress && _user.IsCalibrated())
			{
				switch (_experiment)
				{
					case Experiment.ZONE:
					{
						ZoneCentricExperiment();
						break;
					}
					case Experiment.OBJECT:
					{
						ObjectCentricExperiment();
						break;
					}
					case Experiment.NONE:
					{
						Debug.Log("Switch Case for None Experiment!!!");
						break;						
					}
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			
			if (!_user.IsCalibrated() && (_experiment != Experiment.NONE))
			{
				Instruction.text =
					string.Format("{0} Centric Experiment selected. Use trigger to calibrate the center of shoulders",
						_experiment);
				if (!Controller.GetHairTriggerDown()) return;
				_user.Calibrate(new Vector3(_cameraGameObject.transform.position.x, ControllerObject.transform.position.y, _cameraGameObject.transform.position.z));
				_experimentInProgress = true;
				Debug.Log("Calibration Done!");
			}
		}
	}
}