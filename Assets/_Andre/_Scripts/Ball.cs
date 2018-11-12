using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {
    private Vector3 screenPoint;
    private Vector3 offset;
    private float _lockedYPosition;
    public Camera cam;
    private float x;
    private float y;
    private Vector3 rotateValue;
    void Start(){
        Debug.Log("Ball");
	
    }
    
    
    void OnMouseDown()
    {
//        // load a new scene
//        Debug.Log("Test");
//        Debug.Log(Input.mousePosition);
//        if (cam != null)
//            Debug.Log("Setting camera rotation");
//            cam.transform.LookAt(transform);
    }
//    void OnMouseDrag() 
//    { 
//        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
//        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
//        curPosition.x = _lockedYPosition;
//        transform.position = curPosition;
//    }
    // Update is called once per frame
    void Update()
    {
//        y = Input.GetAxis("Mouse X");
//        x = Input.GetAxis("Mouse Y");
//        Debug.Log(x + ":" + y);
//        rotateValue = new Vector3(x, y * -1, 0);
//        this.cam.transform.eulerAngles = transform.eulerAngles - rotateValue;
//        Debug.Log(Input.mousePosition);
    }
    public float speed;
 
    void FixedUpdate () 
    {
        // Generate a plane that intersects the transform's position with an upwards normal.
        Plane playerPlane = new Plane(Vector3.up, transform.position);
 
        // Generate a ray from the cursor position
        Ray ray = cam.ScreenPointToRay (Input.mousePosition);
 
        // Determine the point where the cursor ray intersects the plane.
        // This will be the point that the object must look towards to be looking at the mouse.
        // Raycasting to a Plane object only gives us a distance, so we'll have to take the distance,
        //   then find the point along that ray that meets that distance.  This will be the point
        //   to look at.
        float hitdist = 0.0f;
        // If the ray is parallel to the plane, Raycast will return false.
        if (playerPlane.Raycast (ray, out hitdist)) 
        {
            // Get the point along the ray that hits the calculated distance.
            Vector3 targetPoint = ray.GetPoint(hitdist);
 
            // Determine the target rotation.  This is the rotation if the transform looks at the target point.
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position);
 
            // Smoothly rotate towards the target point.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
        }
    }
}
