using UnityEngine;

// Credits: http://wiki.unity3d.com/index.php/SphericalCoordinates

namespace ZoneCentric
{
    public class SphericalCoordinates
    {
        public float radius
        {
            get { return _radius; }
            private set { _radius = Mathf.Clamp(value, _minRadius, _maxRadius); }
        }

        public float polar
        {
            get { return _polar; }
            private set
            {
                _polar = loopPolar
                    ? Mathf.Repeat(value, _maxPolar - _minPolar)
                    : Mathf.Clamp(value, _minPolar, _maxPolar);
            }
        }

        public float elevation
        {
            get { return _elevation; }
            private set
            {
                _elevation = loopElevation
                    ? Mathf.Repeat(value, _maxElevation - _minElevation)
                    : Mathf.Clamp(value, _minElevation, _maxElevation);
            }
        }

        // Determine what happen when a limit is reached, repeat or clamp.
        public bool loopPolar = true, loopElevation = false;

        private float _radius, _polar, _elevation;
        private float _minRadius, _maxRadius, _minPolar, _maxPolar, _minElevation, _maxElevation;

        public SphericalCoordinates()
        {
        }

        public SphericalCoordinates(float r, float p, float s,
            float minRadius = 1f, float maxRadius = 20f,
            float minPolar = 0f, float maxPolar = (Mathf.PI * 2f),
            float minElevation = 0f, float maxElevation = (Mathf.PI / 3f))
        {
            _minRadius = minRadius;
            _maxRadius = maxRadius;
            _minPolar = minPolar;
            _maxPolar = maxPolar;
            _minElevation = minElevation;
            _maxElevation = maxElevation;

            SetRadius(r);
            SetRotation(p, s);
        }

        public SphericalCoordinates(Transform T,
            float minRadius = 1f, float maxRadius = 20f,
            float minPolar = 0f, float maxPolar = (Mathf.PI * 2f),
            float minElevation = 0f, float maxElevation = (Mathf.PI / 3f)) :
            this(T.position, minRadius, maxRadius, minPolar, maxPolar, minElevation, maxElevation)
        {
        }

        public SphericalCoordinates(Vector3 cartesianCoordinate,
            float minRadius = 1f, float maxRadius = 20f,
            float minPolar = 0f, float maxPolar = (Mathf.PI * 2f),
            float minElevation = 0f, float maxElevation = (Mathf.PI / 3f))
        {
            _minRadius = minRadius;
            _maxRadius = maxRadius;
            _minPolar = minPolar;
            _maxPolar = maxPolar;
            _minElevation = minElevation;
            _maxElevation = maxElevation;


            FromCartesian(cartesianCoordinate);
        }

        public Vector3 toCartesian
        {
            get
            {
                float a = radius * Mathf.Cos(elevation);
                return new Vector3(a * Mathf.Cos(polar), radius * Mathf.Sin(elevation), a * Mathf.Sin(polar));
            }
        }

        public SphericalCoordinates FromCartesian(Vector3 cartesianCoordinate, int debug = 0)
        {
            radius = cartesianCoordinate.magnitude;
            
            if (cartesianCoordinate.x == 0f)
                cartesianCoordinate.x = Mathf.Epsilon;

            polar = Mathf.Atan(cartesianCoordinate.z / cartesianCoordinate.x);

            if (cartesianCoordinate.x < 0f)
                polar += Mathf.PI;
            
            elevation = Mathf.Asin(Mathf.Clamp(cartesianCoordinate.y / radius, -1f, 1f));

            if (debug == 1)
            {
                Debug.Log("Debug: FromCartesian: (Radius, Polar, Elevation, cartesianCoordinate.y, cartesianCoordinate.y / radius) " + radius + " " + polar *(360/Mathf.PI*2f) + " " + elevation *(360/Mathf.PI*2f) + " " + cartesianCoordinate.y + " " + cartesianCoordinate.y / radius);
            }
            
            return this;
        }

        public SphericalCoordinates RotatePolarAngle(float x)
        {
            return Rotate(x, 0f);
        }

        public SphericalCoordinates RotateElevationAngle(float x)
        {
            return Rotate(0f, x);
        }

        public SphericalCoordinates Rotate(float newPolar, float newElevation)
        {
            return SetRotation(polar + newPolar, elevation + newElevation);
        }

        public SphericalCoordinates SetPolarAngle(float x)
        {
            return SetRotation(x, elevation);
        }

        public SphericalCoordinates SetElevationAngle(float x)
        {
            return SetRotation(x, elevation);
        }

        public SphericalCoordinates SetRotation(float newPolar, float newElevation)
        {
            polar = newPolar;
            elevation = newElevation;

            return this;
        }

        public SphericalCoordinates TranslateRadius(float x)
        {
            return SetRadius(radius + x);
        }

        public SphericalCoordinates SetRadius(float rad)
        {
            radius = rad;
            return this;
        }

        public override string ToString()
        {
            return "[Radius] " + radius + ". [Polar] " + polar*(360/(Mathf.PI * 2f)) + ". [Elevation] " + elevation*(360/(Mathf.PI * 2f)) + ".";
        }
    }
}