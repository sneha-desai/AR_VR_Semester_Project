using UnityEngine;
using UnityEngine.Serialization;

namespace _Andre._Scripts
{
    public class SkyDrawVr : MonoBehaviour
    {
        public Transform DrawPointTransform;
        public Transform[] PrefabArray;
        public float Step = 0.0f;
        public bool SingleMode = false;

        private float _cooldown = 1.0f;

        private SteamVR_TrackedObject _trackedObj;


        private Vector2 _axis;
        private Vector3 _newObjectScale = Vector3.one;

        private Transform _nextPrefab;

        private float _scaleSpeed = .1f;
        private float _distanceSpeed = .1f;
        private Vector3 _lastPoint = Vector3.zero;

        private int _prefabIndex = 0;
        private Vector3 _localScale;
        private float _localDistance = 10.0f;

        private SteamVR_Controller.Device Controller
        {
            get { return SteamVR_Controller.Input((int) _trackedObj.index); }
        }

        void Start()
        {
            _trackedObj = GetComponent<SteamVR_TrackedObject>();
            _nextPrefab = PrefabArray[_prefabIndex];
            Increment();
            DrawPointTransform = Instantiate(_nextPrefab);
            DrawPointTransform.parent = _trackedObj.transform;
            DrawPointTransform.localPosition = new Vector3(0, 0, 10);

            _localScale = DrawPointTransform.localScale;

        }

        void Increment()
        {
            _prefabIndex++;
            if (_prefabIndex >= PrefabArray.Length)
            {
                _prefabIndex = 0;
            }
        }

        void Update()
        {
            _cooldown -= Time.deltaTime;

            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger) && SingleMode == false)
            {
                if (_cooldown < 0)
                {
                    DrawGameObject(_nextPrefab, DrawPointTransform.position);
                }
            }

            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                _axis = Controller.GetAxis();
                DrawPointTransform.transform.localPosition =
                    new Vector3(0, 0, (DrawPointTransform.localPosition.z + _axis.y * _scaleSpeed));
                if (_axis.x > .8f) DrawPointTransform.localScale *= 1 +_distanceSpeed;
                if (_axis.x < -.8f) DrawPointTransform.localScale *= 1 - _distanceSpeed;
                _localScale = DrawPointTransform.localScale;
                _localDistance = DrawPointTransform.transform.localPosition.z;
                _newObjectScale = DrawPointTransform.localScale;
            }
            if (Controller.GetHairTriggerDown() && SingleMode)
            {
                if (_cooldown < 0)
                {
                    DrawGameObject(_nextPrefab, DrawPointTransform.position);
                }
            }

            if (Controller.GetHairTriggerUp())
            {
                Debug.Log(gameObject.name + " Trigger Release");
            }
        }

        void DrawGameObject(Transform trans, Vector3 point)
        {
            
            if (Vector3.Distance(point, _lastPoint) > Step || _lastPoint == Vector3.zero)
            {
                _lastPoint = point;
                DrawPointTransform.parent = transform.parent.parent;
                _nextPrefab = PrefabArray[_prefabIndex];
                DrawPointTransform = Instantiate(_nextPrefab);
                DrawPointTransform.parent = _trackedObj.transform;
                DrawPointTransform.localScale = _localScale;
                DrawPointTransform.localPosition = new Vector3(0,0,_localDistance);
                Increment();
            }
            
        }
    }
}