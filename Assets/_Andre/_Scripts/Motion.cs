using UnityEngine;

namespace _Andre._Scripts
{
	public class Motion : MonoBehaviour {
		public float Speed = 1;

		void Start(){
		}
	
		// Update is called once per frame
		void Update () {
			if (Input.GetKey(KeyCode.W))
			{
				//Move the Rigidbody forwards constantly at speed you define (the blue arrow axis in Scene view)
				transform.Translate(Vector3.forward*Speed);
			}

			if (Input.GetKey(KeyCode.S))
			{
				//Move the Rigidbody backwards constantly at the speed you define (the blue arrow axis in Scene view)
				transform.Translate(Vector3.back*Speed);
			}

			if (Input.GetKey(KeyCode.D))
			{
				//Rotate the sprite about the Y axis in the positive direction
				transform.Translate(Vector3.right*Speed);
			}

			if (Input.GetKey(KeyCode.A))
			{
				//Rotate the sprite about the Y axis in the negative direction
				transform.Translate(Vector3.left*Speed);
			}

			if (Input.GetKey(KeyCode.Space))
			{
				//Rotate the sprite about the Y axis in the negative direction
				transform.Translate(Vector3.up*Speed);
			}

			if (Input.GetKey(KeyCode.LeftControl))
			{
				//Rotate the sprite about the Y axis in the negative direction
				transform.Translate(Vector3.down*Speed);
			}
		}
	}
}
