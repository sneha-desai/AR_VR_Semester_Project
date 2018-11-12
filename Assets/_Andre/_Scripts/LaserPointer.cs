using UnityEngine;

namespace _Andre._Scripts
{
    public class LaserPointer : MonoBehaviour
    {
        // 1
        public GameObject LaserPrefab;

        private SteamVR_TrackedObject _trackedObj;

// 2
        private GameObject _laser;

// 3
        private Transform _laserTransform;

// 4
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
            // 1
            _laser = Instantiate(LaserPrefab);
            // 2
            _laserTransform = _laser.transform;
        }

        private void ShowLaser(float distance)
        {
            // 1
            _laser.SetActive(true);
            // 2
            _laserTransform.position = Vector3.Lerp(_trackedObj.transform.position, _hitPoint, .5f);
            // 3
            _laserTransform.LookAt(_hitPoint);
            // 4
            _laserTransform.localScale = new Vector3(_laserTransform.localScale.x, _laserTransform.localScale.y,
                distance);
        }

        // Update is called once per frame
        void Update()
        {
            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                if (_zoneHovered != null)
                {
                    _zoneHovered.OnLaserDown();
                }
            }
            

            RaycastHit hit;

            // 2
            if (Physics.Raycast(_trackedObj.transform.position, transform.forward, out hit, 100))
            {
                    ZoneVR zone = hit.collider.GetComponent<ZoneVR>();
                    if (zone && !zone.IsHighlighted)
                    {
                        if (_zoneHovered)
                        {
                            _zoneHovered.OnLaserExit();
                            _zoneHovered = null;
                        }
                        _zoneHovered = zone;
                        zone.OnLaserEnter();
                    }
//
//                    Debug.Log("Hit Point: " + hit.point);
//                    Debug.Log("Hit Distance: " + hit.distance);
                    _hitPoint = hit.point;
                    _distance = hit.distance;
                    ShowLaser(_distance);
            }
            else
            {
                _hitPoint = _trackedObj.transform.forward * 10;
                _distance = Vector3.Distance(_trackedObj.transform.position, _hitPoint);
                Debug.Log("Hit Point: " + _hitPoint);
                Debug.Log("Hit Distance: " + _distance);
                if (_zoneHovered != null)
                {
                    _zoneHovered.OnLaserExit();
                    _zoneHovered = null;
                }
                
                ShowLaser(_distance);
            }
            
        }
    }
}