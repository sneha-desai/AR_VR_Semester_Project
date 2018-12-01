using UnityEngine;
using UnityEngine.Serialization;

namespace _Andre._Scripts
{
    public class SkyDraw : MonoBehaviour
    {
        public Transform DrawPointTransform;
        public Transform[] PrefabArray;
        private float _cooldown = 1.0f;

        private SteamVR_TrackedObject _trackedObj;
        [FormerlySerializedAs("_trackedObj2")] public SteamVR_TrackedObject TrackedObj2;


        private Vector2 _axis;
        private Vector3 _newObjectScale = Vector3.one;

        private Transform _nextPrefab;

        private float _scaleSpeed = .1f;
        private float _distanceSpeed = .1f;

        private SteamVR_Controller.Device Controller
        {
            get { return SteamVR_Controller.Input((int) _trackedObj.index); }
        }

        void Awake()
        {
            _trackedObj = GetComponent<SteamVR_TrackedObject>();
            _nextPrefab = PrefabArray[0];
        }

        void Update()
        {
            _cooldown -= Time.deltaTime;

            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
            {
                if (_cooldown < 0)
                {
                    int i = Mathf.RoundToInt(Random.Range(0, PrefabArray.Length));
                    DrawGameObject(_nextPrefab, DrawPointTransform.position);
                }
            }

            if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                _axis = Controller.GetAxis();
//                Debug.Log(DrawPointTransform.localPosition.z);
//                Debug.Log(new Vector3(0, 0, (DrawPointTransform.position.z + _axis.y * 0.003f)));
                DrawPointTransform.transform.localPosition =
                    new Vector3(0, 0, (DrawPointTransform.localPosition.z + _axis.y * _scaleSpeed));
                if (_axis.x > .8f) DrawPointTransform.localScale *= 1 +_distanceSpeed;
                if (_axis.x < -.8f) DrawPointTransform.localScale *= 1 - _distanceSpeed;
                _newObjectScale = DrawPointTransform.localScale;
            }
        }

        void DrawGameObject(Transform trans, Vector3 point)
        {
            Transform o = Instantiate(trans, point, Quaternion.identity);
            o.localScale = _newObjectScale;
        }
    }
}