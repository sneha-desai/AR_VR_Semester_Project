using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;


namespace _Andre._Scripts
{
    public class TornadoTool : MonoBehaviour
    {
        public float Cooldown = 1.0f;
        public Transform Prefab;
        public GameObject TeleportReticlePrefab;
        public Vector3 TeleportReticleOffset;
        public GameObject LaserPrefab;
        public Transform[] PrefabArray;

        private SteamVR_TrackedObject _trackedObj;
        private GameObject _laser;
        private Transform _laserTransform;
        private Vector3 _hitPoint;
        private float _distance;
        private ZoneVR _zoneHovered;

        private GameObject _reticle;
        private Transform _teleportReticleTransform;

        private List<Vector3> _points = new List<Vector3>();

        // the actual tornado
        private Transform _tornado;


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
            Cooldown -= Time.deltaTime;

            RaycastHit hit;


            if (Physics.Raycast(_trackedObj.transform.position, transform.forward, out hit, 100))
            {
                _hitPoint = hit.point;
                _distance = hit.distance;
                ShowLaser(_distance);
                _reticle.SetActive(true);
                _teleportReticleTransform.position = _hitPoint + TeleportReticleOffset;
            }
            else
            {
                _laser.SetActive(false);
                _reticle.SetActive(false);
            }

            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
            }

            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
            {
            }

            if (Controller.GetHairTriggerDown())
            {
                GameObject start = Instantiate(TeleportReticlePrefab);
                start.transform.localPosition = _teleportReticleTransform.localPosition;
                _points.Add(start.transform.position);
                Debug.Log(_points[0]);
                Debug.Log(_teleportReticleTransform.localPosition);
            }

            if (Controller.GetHairTriggerUp())
            {
            }

            if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
            {
                if (_points.Count > 0)
                {
                    Tornado();
                }
            }

            if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
            {
            }
        }

        void Tornado()
        {
            Transform tornado = Instantiate(Prefab);
            tornado.localScale = Vector3.zero;
            Debug.Log(_points[0]);
            Vector3 vec = new Vector3(_points[0].x, _points[0].y, _points[0].z);
            tornado.transform.localPosition = new Vector3(_points[0].x, _points[0].y, _points[0].z);
            Debug.Log(tornado.position);
            Tween tw = tornado.DOPath(_points.ToArray(), _points.Count * 2);
            tornado.gameObject.GetComponent<Material>().DOFade(100, 1);
            tw.OnComplete(() => OnTornadoComplete(tornado));
            tw.OnStart(() => OnTornadoStart(tornado));
        }
        
        void OnTornadoStart(Transform obj)
        {
            Debug.Log("OnTornadoComplete");
            obj.DOScale(Vector3.one, .5f);
        }

        void OnTornadoComplete(Transform obj)
        {
            Debug.Log("OnTornadoComplete");
            obj.DOScale(Vector3.zero, .5f);
            // shake
//            obj.DOLocalRotate(new Vector3(100,100,100), .5f);
//            obj.DOShakePosition(.5f,new Vector3(1,1,1));
        }
    }
}