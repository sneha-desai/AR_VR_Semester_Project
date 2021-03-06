﻿using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
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
		private static GameObject _canvas;
		
		private static Slider _timeSlider;
		private static Slider _accuSlider;

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

		private static bool _errorThresholdSet;
		private static float _errorThresholdValue;	// Radius for OBJECT centric and theta (in degrees) for ZONE centric
		private static GameObject _errorThresholdSphere;
		private static GameObject _errorThresholdParentSphere;
		
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
			_canvas = GameObject.Find("Canvas");
			_accuSlider = GameObject.Find("Accuracy").GetComponent<Slider>();
			_timeSlider = GameObject.Find("Time").GetComponent<Slider>();
			_errorThresholdSphere = GameObject.Find("ErrorThresholdSphere");
			_errorThresholdParentSphere = GameObject.Find("ErrorThresholdParentSphere");
			
			
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
				"To start, press <b><size=16>LEFT</size></b> on Touchpad for Zone Centric or <b><size=16>RIGHT</size></b> for Object Centric Experiment";

			_errorThresholdSet = false;
		}

		private void Awake()
		{
			ControllerObject = GetComponent<SteamVR_TrackedObject>();
		}

		private static void SetColor(Slider obj, float threshold, Color right, Color wrong)
		{
			var colors = obj.fillRect.GetComponent<Image>();
			colors.color = obj.value > threshold ? right : wrong;
		}

		private static void UpdateSliders()
		{
			_timer -= Time.deltaTime;
			_timeSlider.value = _timer/TimeLimit;
			_accuSlider.value = (float) _numCorrect / _numAttempted;
			SetColor(_accuSlider, 0.9f, Color.green, Color.red);
			SetColor(_timeSlider, 0.25f, Color.blue, Color.red);
		}
		
		private static GameObject DrawSphere(Vector3 center, Transform parentTransform = null, string tag = "")
		{
			var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			if (parentTransform)
				sphere.transform.parent = parentTransform;
			if (!string.IsNullOrEmpty(tag))
				sphere.tag = tag;
			sphere.transform.position = center;
			sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

			sphere.GetComponent<Renderer>().material.color = Color.magenta;
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

		private static int GetNearestZone(Vector3 clickedPoint)
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

			return nearestIndex;
		}

		private static bool CheckNearestZone(Vector3 clickedPoint)
		{
			int nearestPointIndex = GetNearestZone(clickedPoint);
			Vector3 nearestPoint = _objectCentricZones[nearestPointIndex];
			return (_currentSubZone == nearestPointIndex && (Vector3.Distance(clickedPoint, nearestPoint) < _errorThresholdValue));
		}

		private static void ResetExperiment()
		{
			_levelInProgress = false;
			_experimentInProgress = false;
			_errorThresholdSet = false;
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
			_canvas.SetActive(false);
			_avatar.SetActive(false);

		}

		private static bool CheckPointInZone(Vector3 cartesianPoint)
		{
			List<float> _subzones = _zoneCentricZones[_currentLevelIndex].SubZones;
			float midPolar = (_subzones[4 * _currentSubZone + 0] + _subzones[4 * _currentSubZone + 1]) / 2;
			float midElevation = (_subzones[4 * _currentSubZone + 2] + _subzones[4 * _currentSubZone + 3]) / 2;
			float minPolar = Mathf.Max(_subzones[4 * _currentSubZone + 0], midPolar - _errorThresholdValue);
			float maxPolar = Mathf.Min(_subzones[4 * _currentSubZone + 1], midPolar + _errorThresholdValue);
			float minElevation = Mathf.Max(_subzones[4 * _currentSubZone + 2], midElevation - _errorThresholdValue);
			float maxElevation = Mathf.Min(_subzones[4 * _currentSubZone + 3], midElevation + _errorThresholdValue);

			Debug.Log("midpolar: " + midPolar + Space + "minPolar " + minPolar + Space + "maxpolar " + Space + maxPolar + Space  + "midelevation " + midElevation + Space + "minElevation " + minElevation + Space + "maxelevation " + maxElevation);
			
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
			_canvas.SetActive(false);
			_errorThresholdParentSphere.SetActive(false);
			_avatar.SetActive(false);
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
			_canvas.SetActive(true);
		}

		private static void RestartLevel()
		{
			_levelInProgress = false;
			DestroySphereWithTag(AvatarSphereTag);
			DestroySphereWithTag(TestSphereTag);
			_objectCentricZones.Clear();
			_avatarSpheres.Clear();
			_ifRestart = true;
			_lifeRemaining--;
			_canvas.SetActive(false);
		}
		
		private static void NextLevel()
		{
			_currentLevelIndex++;
			_levelInProgress = false;
			DestroySphereWithTag(AvatarSphereTag);
			DestroySphereWithTag(TestSphereTag);
			_objectCentricZones.Clear();
			_avatarSpheres.Clear();
			_lifeRemaining = MaxRetry;
			_ifRestart = false;
			_canvas.SetActive(false);
		}

		private static Vector3 GenerateSpherePlacement(int currentLevelIndex)
		{
			var subzones = _zoneCentricZones[currentLevelIndex].SubZones;
			var rndIndex = Rnd.Next(subzones.Count / 4);
			while (rndIndex == _currentSubZone)
			{
				rndIndex = Rnd.Next(subzones.Count / 4);
			}

			_currentSubZone = rndIndex;
			var rndFloatPolar = (float) Rnd.NextDouble();
			var rndFloatElevation = (float) Rnd.NextDouble();
			var polarRange = subzones[4 * _currentSubZone + 1] - subzones[4 * _currentSubZone + 0];
			var elevationRange = subzones[4 * _currentSubZone + 3] - subzones[4 * _currentSubZone + 2];
			var polar = (subzones[4*_currentSubZone + 0] + 0.45f*polarRange) + (0.1f*polarRange)*rndFloatPolar;
			var elevation = (subzones[4*_currentSubZone + 2] + 0.45f*elevationRange) + (0.1f*elevationRange)*rndFloatElevation;
			return PolarCoordinates.ToCartesian(new PolarCoordinates(0.5f, PolarCoordinates.DegToRad(polar), PolarCoordinates.DegToRad(elevation)));
		}

		private void SetPrecision()
		{
			Instruction.text = "Press <b><size=16>UP</size></b> and <b><size=16>DOWN</size></b> on the <b><size=16>Touchpad</size></b> to set the acceptable size of the zone." + Environment.NewLine +"Once finished, press the <b><size=16>Trigger.</size></b>";
				
			_errorThresholdParentSphere.SetActive(true);
			if (Controller.GetPress(TouchpadButton))
			{
				var touchpad = (Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
				if (touchpad.y > 0f)
				{
//						_errorThresholdRadius += 0.05f;
					_errorThresholdSphere.transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
				}
				else
				{
//						_errorThresholdRadius -= 0.05f;
					_errorThresholdSphere.transform.localScale -= new Vector3(0.05f, 0.05f, 0.05f);
				}
			}

			if (!Controller.GetHairTriggerDown()) return;
			_errorThresholdSet = true;
			_errorThresholdParentSphere.SetActive(false);
			_avatar.SetActive(true);

			switch (_experiment)
			{
				case Experiment.OBJECT:
					_errorThresholdValue = _errorThresholdSphere.GetComponent<Renderer>().bounds.extents.magnitude;
					break;
				case Experiment.ZONE:
					Debug.Log(_errorThresholdSphere.GetComponent<Renderer>().bounds.extents.magnitude + Space + _zoneSphere.GetComponent<Renderer>().bounds.extents.magnitude);
					_errorThresholdValue = PolarCoordinates.RadToDeg(
						Mathf.Asin(
							Mathf.Clamp(
						_errorThresholdSphere.GetComponent<Renderer>().bounds.extents.magnitude/_zoneSphere.GetComponent<Renderer>().bounds.extents.magnitude, -1f, 1f)
							)
						);
					break;
				case Experiment.NONE:
					Debug.Log("Experiment NONE detected!");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			Debug.Log("Error Threshold Set!");
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
			switch (_experiment)
			{
				case Experiment.OBJECT:
					_currentSubZone = Rnd.Next(_objectCentricZones.Count);
					_currentSphere = _avatarSpheres[_currentSubZone];
					_currentSphere.GetComponent<Renderer>().enabled = true;
					break;
				case Experiment.ZONE:
					_currentSphere.transform.position = _zoneSphere.transform.TransformPoint(GenerateSpherePlacement(_currentLevelIndex));
					break;
				case Experiment.NONE:
					Debug.Log("Experiment NONE detected inside NextZone function");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}



		private void ObjectCentricExperiment()
		{
		
			if (!_levelInProgress)
			{
				if (_objectCentricZones.Count == _currentLevelIndex)
				{
					Instruction.text = @"<b><size=16>Yay! Zones created!</size></b>" + Environment.NewLine + "Press the <b><size=16>Touchpad</size></b> to start the level.";
				}
				else
				{
					if (_objectCentricZones.Count == 0)
					{
						Instruction.text = "Press the <b><size=16>Trigger</size></b> at the point where you wish to create the zone.";
						if (_ifRestart)
						{
							Instruction.text = "<b><size=16>Sorry! Try again.</size></b>" + Environment.NewLine + Instruction.text;
						}
						else
						{
							if(_currentLevelIndex > 5)
								Instruction.text = "<b><size=16>Yay! Level Cleared!!!</size></b>"+ Environment.NewLine + Instruction.text;
						}
					}
					else
					{
						Instruction.text = "<b><size=16>" + (_currentLevelIndex - _objectCentricZones.Count) + "</size></b>" + " more to go...";
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
				UpdateSliders();
				Instruction.text = "";
				
				if (_timer > 0)
				{
					if (!Controller.GetHairTriggerDown()) return;
					var clickedPoint = ControllerObject.transform.position;
					Debug.Log("clickedPoint: " + clickedPoint);
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
							Instruction.text = "<b><size=16>GAME OVER!</size></b> To restart, press <b><size=16>LEFT</size></b> for ZoneCentric and <b><size=16>RIGHT</size></b> for ObjectCentric on Touchpad";
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

				if (_ifRestart)
				{
					Instruction.text = "<b><size=16>Sorry! Try Again.</size></b> Press the <b><size=16>Touchpad</size></b> to start the level!";
				}
				else
				{
					if (_currentLevelIndex == 0) {
						Instruction.text = "Press the <b><size=16>Touchpad</size></b> to start the level!";
					}
					else if(_currentLevelIndex < _zoneCentricZones.Count)
					{
						Instruction.text = "<b><size=16>Congratulations!</size></b> You are moving on to the next level! Press the <b><size=16>Touchpad</size></b> to start.";
					}
					else
					{
						Instruction.text = "<b><size=16>Congrats! Experiment Completed!</size></b> Press <b><size=16>LEFT</size></b> for ZoneCentric and <b><size=16>RIGHT</size></b> for ObjectCentric on Touchpad";
					}
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
				UpdateSliders();
				Instruction.text = "";

				if (_timer > 0)
				{
					if (!Controller.GetHairTriggerDown()) return;
					var clickedPoint = _user.transform.InverseTransformPoint(ControllerObject.transform.position);
					IsCorrect(clickedPoint);
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
							Instruction.text = "<i>GAME OVER!</i> To restart, press <b><size=16>LEFT</size></b> for ZoneCentric and <b><size=16>RIGHT</size></b> for ObjectCentric on Touchpad";
							Debug.Log("Game Over");
							ResetExperiment();
							return;
						}
						Debug.Log("Restart Level");		
					}					
				}
				
				
//				if (IfQualify())
//				{
//					Debug.Log("YOU QUALIFY!!!!!!!!!!!!!!");
//					NextLevel();
//					DestroySphereWithTag(TestSphereTag);
//				}
//				else
//				{
//					if (!Controller.GetHairTriggerDown()) return;
//					var clickedPoint = _user.transform.InverseTransformPoint(ControllerObject.transform.position);
//					IsCorrect(clickedPoint);
//					_currentSphere.transform.position = _zoneSphere.transform.TransformPoint(GenerateSpherePlacement(_currentLevelIndex));
//				}
			}
		}
		
		// Update is called once per frame
		private void Update ()
		{
			if (!_experimentInProgress)
			{
//				Instruction.text = "Welcome to Proprioceptive Experiment!";
				_experiment = ChooseExperiment(); 
			}

			if (_experimentInProgress && _user.IsCalibrated())
			{
				if (!_errorThresholdSet)
				{
					// set threshold and return
					SetPrecision();
					return;
				}
				
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
					string.Format("{0} Centric Experiment selected. Put the controller just below the neck and press the <b><size=16>Trigger</size></b> button.",
						_experiment);
				if (!Controller.GetHairTriggerDown()) return;
				_user.Calibrate(new Vector3(_cameraGameObject.transform.position.x, ControllerObject.transform.position.y, _cameraGameObject.transform.position.z));
				_experimentInProgress = true;

				Debug.Log("Calibration Done!");

			}
			
			
			
//			// Object centric error threshold
//			if (_user.IsCalibrated() && !_errorThresholdSet && _experiment == Experiment.OBJECT)
//			{
//				
//
//			}
//			
			
//			// Zone centric error threshold
//			if (_user.IsCalibrated() && !_errorThresholdSet && _experiment == Experiment.ZONE)
//			{
//			}
			
			
			
			
			
		}
	}
}