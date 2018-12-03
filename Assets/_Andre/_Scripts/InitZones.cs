using UnityEngine;
using ZoneCentric;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace _Andre._Scripts
{
    public class Zones
    {
        public int NumSubZones;
        public List<float> SubZones;
        public List<float> CenterOfSubZones;

        public Zones()
        {
            SubZones = new List<float>();
        }

        public void AddSubZone(float minPolar, float maxPolar, float minElevation, float maxElevation)
        {
            SubZones.Add(minPolar);
            SubZones.Add(maxPolar);
            SubZones.Add(minElevation);
            SubZones.Add(maxElevation);
        }
    }

    public class Interface
    {
        private static List<Zones> _zones;
        private static Interface _instance;

        private Interface() { }

        public static Interface Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Interface();
                }
                return _instance;
            }
        }

        private static void InitZones()
        {
            _zones = new List<Zones> { new Zones() };
            _zones[0].NumSubZones = 2;
            _zones[0].AddSubZone(0f, 180f, -90f, 90f);
            _zones[0].AddSubZone(180f, 360f, -90f, 90f);

            _zones.Add(new Zones());
            _zones[1].NumSubZones = 4;
            _zones[1].AddSubZone(0f, 180f, -90f, 0f);
            _zones[1].AddSubZone(0f, 180f, 0f, 90f);
            _zones[1].AddSubZone(180f, 360f, -90f, 0f);
            _zones[1].AddSubZone(180f, 360f, 0f, 90f);
        }

        public static List<Zones> GetZones()
        {
            InitZones();
            return _zones;
        }


        public static bool CheckPointInZone(Vector3 cartesianPoint, List<Zones> zoneCentricZones, int currentLevelIndex, int currentSubZone)
        {
            List<float> subzones = zoneCentricZones[currentLevelIndex].SubZones;
            float minPolar = subzones[4 * currentSubZone + 0];
            float maxPolar = subzones[4 * currentSubZone + 1];
            float minElevation = subzones[4 * currentSubZone + 2];
            float maxElevation = subzones[4 * currentSubZone + 3];


            var polarPoint = PolarCoordinates.FromCartesian(cartesianPoint);

            var polar = PolarCoordinates.RadToDeg(polarPoint.Polar);
            var elevation = PolarCoordinates.RadToDeg(polarPoint.Elevation);


            // Condition when subzone traverses from fourth quadrant to first quadrant;
            if (maxPolar < minPolar)
            {
                minPolar -= 360f;
                if (polar >= 0f && polar <= maxPolar)
                    return (elevation >= minElevation) && (elevation <= maxElevation);

                polar -= 360f;
                if (polar >= minPolar && polar <= 0f)
                {
                    return (elevation >= minElevation) && (elevation <= maxElevation);
                }
                return false;
            }

            if ((polar >= minPolar) && (polar <= maxPolar) && (elevation >= minElevation) && (elevation <= maxElevation))
            {
                // Debug.Log("polar " + polar + " elevation " + elevation + " minPolar " + minPolar + " maxpolar " + maxPolar + " minElevation " + minElevation + " maxelevation " + maxElevation);
            }


            return (polar >= minPolar) && (polar <= maxPolar) && (elevation >= minElevation) && (elevation <= maxElevation);
        }
    }
}