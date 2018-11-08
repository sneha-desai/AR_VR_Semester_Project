using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyDraw : MonoBehaviour
{
    public Transform DrawPointTransform;

    public float radius = 50.0f;

    private float cooldown = 1.0f;
    public float intensity = 1.0f;
    private bool _mousePressed = false;

    public Transform[] PrefabArray;


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
        if (_mousePressed)
        {
            if (cooldown < 0)
            {
                float f = Random.Range(0, PrefabArray.Length);
                int i = Mathf.RoundToInt(f);
                DrawGameObject(PrefabArray[i], DrawPointTransform.position);
            }
        }
    }

    void DrawGameObject(Transform transform, Vector3 point)
    {
        Debug.Log("DrawGameObject");
        Instantiate(transform, point, Quaternion.identity);
    }
}