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
        
        
        //ZonceCentric
        private static List<Zones> _zoneCentricZones;
        private static int _currentLevelIndex;


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
            TeleportReticlePrefab = Resources.Load<GameObject>("utils/TeleportReticle");
            LaserPrefab = Resources.Load<GameObject>("utils/Laser");
            _laser = Instantiate(LaserPrefab);
            _laserTransform = _laser.transform;

            _reticle = Instantiate(TeleportReticlePrefab);
            _teleportReticleTransform = _reticle.transform;

            _nextPrefab = PrefabArray[_prefabIndex];
            Increment();
            DrawPointTransform = Instantiate(TeleportReticlePrefab.transform);
            DrawPointTransform.parent = _teleportReticleTransform.parent;
//            DrawPointTransform.localPosition = _reticle.transform.localPosition;
            _localScale = DrawPointTransform.localScale;
            
            
            //ZoneCentric Code
            _zoneCentricZones = Interface.GetZones();
            _currentLevelIndex = 1;

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

        void Update()
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
                DrawPointTransform.position = _teleportReticleTransform.position;
            }
            else
            {
                _laser.SetActive(false);
                _reticle.SetActive(false);
            }

            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                _axis = Controller.GetAxis();
                if (_axis.x > .8f) DrawPointTransform.localScale *= 1 + _scaleSpeed;
                if (_axis.x < -.8f) DrawPointTransform.localScale *= 1 - _scaleSpeed;
                _localScale = DrawPointTransform.localScale;
            }
            
            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger) && SingleMode == false)
            {
                if (Cooldown < 0)
                {
                    DrawGameObject(_nextPrefab, DrawPointTransform.position);
                }
            }

            if (Controller.GetHairTriggerDown() && SingleMode)
            {
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
                _nextPrefab = PrefabArray[_prefabIndex];
                DrawPointTransform = Instantiate(_nextPrefab);
                DrawPointTransform.parent = _trackedObj.transform;
                DrawPointTransform.localScale = _localScale;
                Increment();
            }
        }
    }
}