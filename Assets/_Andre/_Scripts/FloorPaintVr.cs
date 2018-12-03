using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Valve.VR;
using ZoneCentric;
using Random = UnityEngine.Random;

namespace _Andre._Scripts
{
    public class FloorPaintVr : MonoBehaviour
    {

        public float Cooldown = 1.0f;
        public Transform DrawPointTransform;
        public GameObject TeleportReticlePrefab;
        public float Step = 0.0f;
        public bool SingleMode = true;
        public Vector3 TeleportReticleOffset;
        public GameObject LaserPrefab;
        public Transform[] PrefabArray;

        private SteamVR_TrackedObject _trackedObj;
        private GameObject _laser;
        private Transform _laserTransform;
        private Vector3 _hitPoint;
        private float _distance;
        private ZoneVR _zoneHovered;
        private Transform _nextPrefab;
        private int _prefabIndex = 0;

        private GameObject _reticle;
        private Transform _teleportReticleTransform;

        private Vector3 _localScale;
        private float _scaleSpeed = 0.3f;
        private Vector3 _lastPoint = Vector3.zero;
        
        
        //ZoneCentric
        private static List<Zones> _zoneCentricZones;
        private static int _currentLevelIndex;
        private GameObject _reference;
        private GameObject _cameraObject;


        private SteamVR_Controller.Device Controller
        {
            get { return SteamVR_Controller.Input((int) _trackedObj.index); }
        }

        void Awake()
        {
            _trackedObj = GetComponent<SteamVR_TrackedObject>();
        }

        private void Start()
        {
            _laser = Instantiate(LaserPrefab);
            _laserTransform = _laser.transform;
            
            _reticle = Instantiate(TeleportReticlePrefab);
            _teleportReticleTransform = _reticle.transform;

            _nextPrefab = PrefabArray[_prefabIndex];
            Increment();
            DrawPointTransform = Instantiate(_nextPrefab);
            DrawPointTransform.parent = _teleportReticleTransform.parent;
//            DrawPointTransform.localPosition = _reticle.transform.localPosition;
            _localScale = DrawPointTransform.localScale;
            
            
            //ZoneCentric Code
            _zoneCentricZones = Interface.GetZones();
            _currentLevelIndex = 1;
            _reference = GameObject.Find("Reference");
            _cameraObject = GameObject.Find("Camera (eye)");
     
        }

        void Increment()
        {
            _prefabIndex++;
            if (_prefabIndex >= PrefabArray.Length)
            {
                _prefabIndex = 0;
            }
        }

        private void ShowLaser(float distance)
        {
            _laser.SetActive(true);
            _laserTransform.position = Vector3.Lerp(_trackedObj.transform.position, _hitPoint, .5f);
            _laserTransform.LookAt(_hitPoint);
            _laserTransform.localScale = new Vector3(_laserTransform.localScale.x, _laserTransform.localScale.y,
                distance);
        }

        private int GetZone()
        {
            int zone = -1;
            Vector3 clickedPoint = _cameraObject.transform.InverseTransformPoint(_trackedObj.transform.position);
            //Debug.Log("controller point in world space " + _trackedObj.transform.position.x + " " + _trackedObj.transform.position.y + " " + _trackedObj.transform.position.z);
            //Debug.Log("controller point in sphere space " + _trackedObj.transform.InverseTransformPoint(_trackedObj.transform.position).x*10f + " " + (_trackedObj.transform.InverseTransformPoint(_trackedObj.transform.position).y + 0.32f)*10f + " " + _trackedObj.transform.InverseTransformPoint(_trackedObj.transform.position).z*10f);

            var polarPoint = PolarCoordinates.FromCartesian(clickedPoint);

            var polar = PolarCoordinates.RadToDeg(polarPoint.Polar);
            var elevation = PolarCoordinates.RadToDeg(polarPoint.Elevation);

            Debug.Log("in sphere space as per function " + clickedPoint.x + " " + clickedPoint.y + " " + clickedPoint.z + " " + polar + " " + elevation);


            //Vector3 clickedPoint = transform.InverseTransformPoint(_trackedObj.transform.position);

            //            print("ClickedPoint: " + clickedPoint);
            List<float> _subzones = _zoneCentricZones[_currentLevelIndex].SubZones;
            for (int i = 0; i < _subzones.Count / 4; i++)
            {
                //Debug.Log("Checking subzone: " + i);
                if (Interface.CheckPointInZone(clickedPoint, _zoneCentricZones, _currentLevelIndex, i))
                {
                    zone = i;
                }
            }

            //print("Cartesian ClickedPoint: " + clickedPoint + " Polar ClickedPoint: polar " + polar + " elevation " + elevation +" zone: " + zone);

            return zone;
        }

        private bool _isDrawable ()
        {
            bool isDable = _teleportReticleTransform && DrawPointTransform && _teleportReticleTransform.gameObject != null && DrawPointTransform.gameObject != null;
            Debug.Log("isDrawable is " + isDable);
            return isDable;
        }

        void FixedUpdate()
        {
            Vector2 _axis;
            Cooldown -= Time.deltaTime;

            RaycastHit hit;
            

            if (Physics.Raycast(_trackedObj.transform.position, transform.forward, out hit, 100))
            {
                _hitPoint = hit.point;
                _distance = hit.distance;
                ShowLaser(_distance);
                _reticle.SetActive(true);
                _teleportReticleTransform.position = _hitPoint + TeleportReticleOffset;
                if (_isDrawable())
                {
                    DrawPointTransform.position = _teleportReticleTransform.position;
                }
            }
            else
            {
                _laser.SetActive(false);
                _reticle.SetActive(false);
            }

            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad) && _isDrawable())
            {
                _axis = Controller.GetAxis();
                if (_axis.x > .8f) DrawPointTransform.localScale *= 1 + _scaleSpeed;
                if (_axis.x < -.8f) DrawPointTransform.localScale *= 1 - _scaleSpeed;
                _localScale = DrawPointTransform.localScale;
            }
            
            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger) && SingleMode == false && _isDrawable())
            {
                if (Cooldown < 0)
                {
                    DrawGameObject(_nextPrefab, DrawPointTransform.position);
                }
            }

            if (Controller.GetHairTriggerDown() && SingleMode && _isDrawable())
            {
                Debug.Log("Inside single Mode");
                if (Cooldown < 0)
                {
                    DrawGameObject(_nextPrefab, DrawPointTransform.position);
                }
            }

            if (Controller.GetHairTriggerUp())
            {
                Debug.Log(gameObject.name + " Trigger Release");
            }

            if (Controller.GetPressDown(EVRButtonId.k_EButton_Grip))
            {
                //var camera = GameObject.Find("Camera (eye)");
               //var camerapos = camera.transform.position;
              //var refpos = _reference.transform.position;
                //            Debug.Log("camera early: " + camerapos.x + " " + camerapos.y + " " + camerapos.z);
                //          Debug.Log("ref early: " + refpos.x + " " + refpos.y + " " + refpos.z);
                //        refpos = new Vector3(camerapos.x, camerapos.y - 0.32f, camerapos.z);
                //_reference.transform.position = refpos;
                //      Debug.Log("camera later: " + camerapos.x + " " + camerapos.y + " " + camerapos.z);
                //    Debug.Log("ref later: " + refpos.x + " " + refpos.y + " " + refpos.z);
                //refpos = camerapos;
                //_reference.transform.position = refpos;
                int zone = GetZone();
                Debug.Log("zone: " + zone);
                PrefabArray = ZoneDictionary.GetPrefabArrayForZone(zone);
            }
        }

        void DrawGameObject(Transform trans, Vector3 point)
        {
            if (Vector3.Distance(point, _lastPoint) > Step || _lastPoint == Vector3.zero)
            {
                _lastPoint = point;
                DrawPointTransform.parent = transform.parent.parent;
                DrawPointTransform
                    .DOMove(
                        new Vector3(DrawPointTransform.position.x, DrawPointTransform.position.y-1,
                            DrawPointTransform.position.z), 1).From();
                Debug.Log(PrefabArray);
                Debug.Log(PrefabArray[_prefabIndex]);
                _nextPrefab = PrefabArray[_prefabIndex];
                DrawPointTransform = Instantiate(_nextPrefab);
                DrawPointTransform.parent = _trackedObj.transform;
                DrawPointTransform.localScale = _localScale;
                Increment();
            }
        }
    }
}