using UnityEngine;
using System.Collections;

public class FollowPlayerScript : MonoBehaviour
{

		public GameObject car;
		private Transform target;
		// The distance in the x-z plane to the target
		public float distance = 5.0f;
		// the height we want the camera to be above the target
		public float height = 4.0f;

		public float heightDamping = 2.0f;
		public float rotationDamping = 3.0f;

		// To adjust camera angle, in order to see ahead while driving.
		public float cameraTilt = 1.5f;
	
		// Use this for initialization
		void Start () {
			target = car.transform;
		}
	
		// Update is called once per frame
		void Update () {
		}

		void LateUpdate (){
				// Early out if we don't have a target
				if (!target) {
						return;
				}

				// Calculate the current rotation angles
				float wantedRotationAngle = target.eulerAngles.y;
				float wantedHeight = target.position.y + height;
		
				float currentRotationAngle = transform.eulerAngles.y;
				float currentHeight = transform.position.y;
		
				// Damp the rotation around the y-axis
				currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
		
				// Damp the height
				currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);
		
				// Convert the angle into a rotation
				Quaternion currentRotation = Quaternion.Euler (0f, currentRotationAngle, 0f);

				// Set the position of the camera on the x-z plane to:
				// distance meters behind the target
				gameObject.transform.position = target.position;
				gameObject.transform.position -= currentRotation * Vector3.forward * distance;
		
				// Set the height of the camera
				transform.position = new Vector3 (transform.position.x, currentHeight, transform.position.z);

				// Always look a bit (value of cameraTilt) over the target.
				gameObject.transform.LookAt (new Vector3(target.transform.position.x, target.transform.position.y + cameraTilt, target.transform.position.z));
		}
}
