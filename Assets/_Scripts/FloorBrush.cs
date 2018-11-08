using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;


public class FloorBrush : MonoBehaviour
{

    public float DrawRadius = 50.0f;
    public float Intensity = 1.0f;
    public Transform[] PrefabArray;

    private float _cooldown = 1.0f;
    private bool _mousePressed = false;



    private void Start()
    {
//		Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        _mousePressed = Input.GetMouseButton(0);
        _cooldown -= Time.deltaTime;
        if (_mousePressed)
        {
            if (_cooldown < 0)
            {
                int layerMask = 1 << 9;

                layerMask = ~layerMask;

                _cooldown = 1 / Intensity;
                RaycastHit hit;
                Vector3 mousePosition = Input.mousePosition;
                Vector3 mousePositionWithRadius =
                    new Vector3(Random.Range(mousePosition.x - DrawRadius, mousePosition.x + DrawRadius),
                        Random.Range(mousePosition.y - DrawRadius, mousePosition.y + DrawRadius), 0);

                Debug.Log(mousePositionWithRadius);
                if (Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(mousePositionWithRadius), out hit,
                    Mathf.Infinity, layerMask))
                {
                    Transform objecthit = hit.transform;
                    float f = Random.Range(0, PrefabArray.Length);
                    int i = Mathf.RoundToInt(f);
                    DrawGameObject(PrefabArray[i], hit.point);
                }
            }
        }
    }

    private void DrawGameObject(Transform prefab, Vector3 point)
    {
        Instantiate(prefab, point, Quaternion.identity);
    }
}