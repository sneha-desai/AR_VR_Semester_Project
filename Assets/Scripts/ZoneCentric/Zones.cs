using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace ZoneCentric
{
    public class Zones
    {
        public int NumSubZones;
        public List<float> SubZones;

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

        private Interface(){}
        
        public static Interface Instance
        {
            get{
                if(_instance == null){
                    _instance = new Interface();
                }
                return _instance;
            }
        }
        
        private static void InitZones()
        {
            _zones = new List<Zones> {new Zones()};
            _zones[0].NumSubZones = 2;
            _zones[0].AddSubZone(0f, 180f, -90f, 90f);
            _zones[0].AddSubZone(180f, 360f, -90f, 90f);
            
//            _zones[3] = new Zones(3);
//            _zones[3].AddSubZone(0f, 180f, -90f, 90f);
//            _zones[3].AddSubZone(180f, 360f, -90f, 90f);

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
    }
}