using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepDraw : MonoBehaviour
{
    public Transform DrawPointTransform;

    public float radius = 50.0f;

    private float cooldown = 1.0f;
    public float intensity = 1.0f;
    public float Step = 1.0f;
    public Transform[] PrefabArray;

    private Vector3 _lastPoint;
    private bool _mousePressed = false;
    private GameObject _drawObj;



    // Use this for initialization
    void Start()
    {
        _drawObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _drawObj.transform.parent = transform;
        _drawObj.transform.localScale = new Vector3(.1f,.1f,.1f);
        _drawObj.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z+10);
        _drawObj.GetComponent<MeshRenderer>().enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        _mousePressed = Input.GetMouseButton(0);
        cooldown -= Time.deltaTime;

        Vector3 movement = transform.position * 1.02f;
        if (_mousePressed)
        {
            if (cooldown < 0)
            {
                float f = Random.Range(0, PrefabArray.Length);
                int i = Mathf.RoundToInt(f);
                DrawGameObject(PrefabArray[i], _drawObj.transform.position);
            }
        }
//        _drawObj.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);

    }

    void DrawGameObject(Transform transform, Vector3 point)
    {
        if (Vector3.Distance(point, _lastPoint) > Step)
        {
            _lastPoint = point;
            Debug.Log("DrawGameObject");
            Instantiate(transform, point, Quaternion.identity);
        }

    }
}