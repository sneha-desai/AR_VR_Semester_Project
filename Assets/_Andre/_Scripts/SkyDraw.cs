using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyDraw : MonoBehaviour
{
    public Transform DrawPointTransform;
    public Transform Camera;

    public float radius = 50.0f;

    private float cooldown = 1.0f;
    public float intensity = 1.0f;
    private bool _mousePressed = false;

    public Transform[] PrefabArray;
    private SteamVR_TrackedObject _trackedObj;
    public SteamVR_TrackedObject _trackedObj2;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int) _trackedObj.index); }
    }
    private SteamVR_Controller.Device Controller2
    {
        get { return SteamVR_Controller.Input((int) _trackedObj2.index); }
    }

    void Awake()
    {
        _trackedObj = GetComponent<SteamVR_TrackedObject>();
        _trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        _mousePressed = Input.GetMouseButton(0);
        cooldown -= Time.deltaTime;

        Vector3 movement = transform.position * 1.02f;
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (cooldown < 0)
            {
                float f = Random.Range(0, PrefabArray.Length);
                int i = Mathf.RoundToInt(f);
                DrawGameObject(PrefabArray[i], DrawPointTransform.position);
            }        
        }
//        if (Controller2.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
//        {
//            Vector2 axis = Controller2.GetAxis();
//            Vector3 forward = Vector3.Scale(new Vector3(0.1f+axis.x,0.1f*axis.y,0.1f), Camera.rotation.eulerAngles);
//            Debug.Log("Forward: " + forward);
//            Vector3 t = transform.InverseTransformDirection(Camera.transform.rotation.eulerAngles);
////            transform.parent.Translate(forward);
//            // Moving facing direction
//            transform.parent.transform.position += Camera.transform.forward * 10.0f * Time.deltaTime * axis.y;
//            transform.parent.transform.position += Camera.transform.right * 10.0f * Time.deltaTime * axis.x;
//        }
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        {    
//            Debug.Log("Trackpad1");
            Vector2 axis2 = Controller.GetAxis();
            DrawPointTransform.transform.localPosition = new Vector3(0, 0, (DrawPointTransform.position.z+axis2.y*.3f));
            if (axis2.x > .8f) DrawPointTransform.localScale *= 1.1f;
            if (axis2.x < -.8f) DrawPointTransform.localScale *= 0.9f;
//            DrawPointTransform.localScale *= axis2.x > 0 ? 1.1f : 0.9f;
        }
    }

    void DrawGameObject(Transform trans, Vector3 point)
    {
        Debug.Log("DrawGameObject");
        Instantiate(trans, point, Quaternion.identity);
    }
}