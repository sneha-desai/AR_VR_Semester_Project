using UnityEngine;

namespace _Andre._Scripts
{
	public class ControllerGrabObject2 : MonoBehaviour {

		private SteamVR_TrackedObject _trackedObj;
		
		// 1
		private GameObject _collidingObject; 
// 2
		private GameObject _objectInHand; 

		private SteamVR_Controller.Device Controller
		{
			get { return SteamVR_Controller.Input((int)_trackedObj.index); }
		}

		void Awake()
		{
			_trackedObj = GetComponent<SteamVR_TrackedObject>();
		}
		
		private void SetCollidingObject(Collider col)
		{
			// 1
			if (_collidingObject || !col.GetComponent<Rigidbody>())
			{
				return;
			}
			// 2
			_collidingObject = col.gameObject;
		}
		// 1
		public void OnTriggerEnter(Collider other)
		{
			SetCollidingObject(other);
		}

// 2
		public void OnTriggerStay(Collider other)
		{
			SetCollidingObject(other);
		}

// 3
		public void OnTriggerExit(Collider other)
		{
			if (!_collidingObject)
			{
				return;
			}

			_collidingObject = null;
		}
		
		private void GrabObject()
		{
			// 1
			_objectInHand = _collidingObject;
			_collidingObject = null;
			// 2
			var joint = AddFixedJoint();
			joint.connectedBody = _objectInHand.GetComponent<Rigidbody>();
		}
		private void ReleaseObject()
		{
			// 1
			if (GetComponent<FixedJoint>())
			{
				// 2
				GetComponent<FixedJoint>().connectedBody = null;
				Destroy(GetComponent<FixedJoint>());
				// 3
				_objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
				_objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
			}
			// 4
			_objectInHand = null;
		}
// 3
		private FixedJoint AddFixedJoint()
		{
			FixedJoint fx = gameObject.AddComponent<FixedJoint>();
			fx.breakForce = 20000;
			fx.breakTorque = 20000;
			return fx;
		}
		
	
		// Update is called once per frame
		void Update () {
			if (Controller.GetHairTriggerDown())
			{
				if (_collidingObject)
				{
					GrabObject();
				}
			}

// 2
			if (Controller.GetHairTriggerUp())
			{
				if (_objectInHand)
				{
					ReleaseObject();
				}
			}
		}
	}
}
