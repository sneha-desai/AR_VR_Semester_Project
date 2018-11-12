using System;
using UnityEngine;

namespace _Andre._Scripts
{
    public class ZoneVR : MonoBehaviour
    {
        public Color OriginalColor = new Color(1, 1, 1);
        public Color HighlightColor = new Color(1, 0.3f, 0.3f);
        public bool IsHighlighted = false;
        private bool _selected = false;
        public Action<Zone> OnSelect;

        private ZonesController _zonesController;

        // Use this for initialization
        void Start()
        {
            IsHighlighted = false;
            _zonesController = FindObjectOfType(typeof(ZonesController)) as ZonesController;

            GetComponent<Renderer>().material.color = new Color(1, 1, 1);
//		Debug.Log("Zone Start()");
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void OnLaserEnter()
        {
            Debug.Log("OnLaserEnter");
            GetComponent<Renderer>().material.color = HighlightColor;
            IsHighlighted = true;
            // Here some logic to highlight okay?
        }

        public void OnLaserExit()
        {
            if (!_selected)
                GetComponent<Renderer>().material.color = new Color(1, 1, 1);
            IsHighlighted = false;
            Debug.Log("OnLaserExit");
        }

        public Color OriginalColor1
        {
            get { return OriginalColor; }
            set { OriginalColor = value; }
        }

        public Color HighlightColor1
        {
            get { return HighlightColor; }
            set { HighlightColor = value; }
        }

        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; }
        }

        public void OnLaserDown()
        {
            Debug.Log("OnLaserDown");
            if (!_selected)
            {
                if (_zonesController.SelectedCount < 10)
                {
                    _zonesController.SelectedCount++;
                    _selected = !_selected;
                } 
            }
            else
            {
                _zonesController.SelectedCount--;
                _selected = !_selected;
            }
            Debug.Log("Selected Count: " + _zonesController.SelectedCount);
        }
        
        public void OnTriggerEnter(Collider other)
        {
            Debug.Log("OnTriggerEnter");
        }
    }
}