using System.Collections.Generic;
using UnityEngine.Experimental.PlayerLoop;

namespace ZoneCentric
{
    public class Zones
    {
        public int NumPartition;
        public List<float> SubZones;

        public void AddSubZone(float minPolar, float maxPolar, float minElevation, float maxElevation)
        {
            SubZones.Add(minPolar);
            SubZones.Add(maxPolar);
            SubZones.Add(minElevation);
            SubZones.Add(maxElevation);
        }

        public Zones(int numPartition)
        {
            NumPartition = numPartition;
        }
    }
    
    public sealed class Interface
    {
        private static List<Zones> _zones; 
   
        private static void InitZones()
        {
            _zones[2] = new Zones(2);
            _zones[2].AddSubZone(0f, 180f, -90f, 90f);
            _zones[2].AddSubZone(180f, 360f, -90f, 90f);
            
//            _zones[3] = new Zones(3);
//            _zones[3].AddSubZone(0f, 180f, -90f, 90f);
//            _zones[3].AddSubZone(180f, 360f, -90f, 90f);

            _zones[4] = new Zones(4);
            _zones[4].AddSubZone(0f, 180f, -90f, 0f);
            _zones[4].AddSubZone(0f, 180f, 0f, 90f);
            _zones[4].AddSubZone(180f, 360f, -90f, 0f);
            _zones[4].AddSubZone(180f, 360f, 0f, 90f);
        }
        
        public static List<Zones> GetZones()
        {
            InitZones();
            return _zones;
        }
    }
}