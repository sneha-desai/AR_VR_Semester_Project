using UnityEngine;
using UnityEngine.Serialization;

namespace _Andre._Scripts
{
    public class LaserPointer : MonoBehaviour
    {
        [FormerlySerializedAs("teleportReticlePrefab")] public GameObject TeleportReticlePrefab;
        private GameObject _reticle;
        private Transform _teleportReticleTransform;
        [FormerlySerializedAs("teleportReticleOffset")] public Vector3 TeleportReticleOffset;
        public GameObject LaserPrefab;
        private SteamVR_TrackedObject _trackedObj;
        private GameObject _laser;
        private Transform _laserTransform;
        private Vector3 _hitPoint;
        private float _distance;
        private ZoneVR _zoneHovered;
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
        }
    }
}