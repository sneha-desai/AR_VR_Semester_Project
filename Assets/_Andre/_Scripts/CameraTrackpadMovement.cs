using UnityEngine;

namespace _Andre._Scripts
{
    public class CameraTrackpadMovement : MonoBehaviour
    {
        private SteamVR_TrackedObject _trackedObj;
        private Vector2 _axis;

        private Transform _camera;

        private SteamVR_Controller.Device Controller
        {
            get { return SteamVR_Controller.Input((int) _trackedObj.index); }
        }

        void Awake()
        {
            _trackedObj = GetComponent<SteamVR_TrackedObject>();
            _camera = transform.parent.GetComponentInChildren<Camera>().transform;
        }

        private void Update()
        {
            _axis = Controller.GetAxis();
            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                // move
                Debug.Log(_camera);
                transform.parent.transform.position += _camera.transform.forward * 10.0f * Time.deltaTime * _axis.y;
                transform.parent.transform.position += _camera.transform.right * 10.0f * Time.deltaTime * _axis.x;
            }

            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
            {
                // rotate
                transform.parent.transform.eulerAngles = new Vector3(
                    transform.parent.transform.eulerAngles.x - _axis.y,
                    transform.parent.transform.eulerAngles.y + _axis.x,
                    transform.parent.transform.eulerAngles.z
                );
            }
        }
    }
}