using UnityEngine;
using Random = UnityEngine.Random;


namespace _Andre._Scripts
{
    public class FloorPaintVr : MonoBehaviour
    {
        
        public GameObject TeleportReticlePrefab;
        private GameObject _reticle;
        private Transform _teleportReticleTransform;
        public Vector3 TeleportReticleOffset;
        public GameObject LaserPrefab;
                
        public float DrawRadius = 50.0f;
        public float Intensity = 1.0f;
        public Transform[] PrefabArray;

        private SteamVR_TrackedObject _trackedObj;
        private GameObject _laser;
        private Transform _laserTransform;
        private Vector3 _hitPoint;
        private float _distance;
        private ZoneVR _zoneHovered;

        private float _cooldown = 1.0f;
        
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
            _cooldown -= Time.deltaTime;

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

        private void DrawGameObject(Transform prefab, Vector3 point)
        {
            Instantiate(prefab, point, Quaternion.identity);
        }
    }
}