using UnityEditor.Experimental.UIElements;
using UnityEngine;

namespace ZoneCentric
{
    public class PolarCoordinates
    { 
        public float Radius;
        public float Polar;
        public float Elevation;

        public PolarCoordinates(float radius, float polar, float elevation)
        {
            Radius = radius;
            Polar = polar;
            Elevation = elevation;
        }

        public PolarCoordinates()
        {
			
        }
		
        public static PolarCoordinates FromCartesian(Vector3 cartesianCoordinate)
        {
            PolarCoordinates temp = new PolarCoordinates {Radius = cartesianCoordinate.magnitude};

            if (cartesianCoordinate.x == 0f)
                cartesianCoordinate.x = Mathf.Epsilon;

            temp.Polar = Mathf.Atan(cartesianCoordinate.z / cartesianCoordinate.x);

            if (cartesianCoordinate.x < 0f)
                temp.Polar += Mathf.PI;

            if ((cartesianCoordinate.z < 0f) && (cartesianCoordinate.x > 0f))
                temp.Polar += Mathf.PI * 2f;
            
            temp.Elevation = Mathf.Asin(Mathf.Clamp(cartesianCoordinate.y / temp.Radius, -1f, 1f));

            return temp;
        }

        public static Vector3 ToCartesian(PolarCoordinates polarCoordinates)
        {
            
            float a = polarCoordinates.Radius * Mathf.Cos(polarCoordinates.Elevation);
            return new Vector3(a * Mathf.Cos(polarCoordinates.Polar), polarCoordinates.Radius * Mathf.Sin(polarCoordinates.Elevation), a * Mathf.Sin(polarCoordinates.Polar));
        }
        
        public static float DegToRad(float deg)
        {
            return deg * Mathf.PI / 180f;
        }

        public static float RadToDeg(float rad)
        {
            return rad * 180f / Mathf.PI;
        }

    }
}