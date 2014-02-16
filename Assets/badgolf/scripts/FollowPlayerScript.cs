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
		public float minDistance = 3.0f;

		public bool zoom = true;
	
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
				Vector3 lookAtPt = new Vector3(target.transform.position.x, target.transform.position.y + cameraTilt, target.transform.position.z);
				transform.LookAt(lookAtPt);

		        if( zoom ) {
					Ray[] frustumRays = new Ray[4];

					frustumRays[0] = transform.camera.ScreenPointToRay(new Vector3(0,0,0));
					frustumRays[1] = transform.camera.ScreenPointToRay(new Vector3(transform.camera.pixelWidth,0,0));
					frustumRays[2] = transform.camera.ScreenPointToRay(new Vector3(0,transform.camera.pixelHeight,0));
					frustumRays[3] = transform.camera.ScreenPointToRay(new Vector3(transform.camera.pixelWidth,transform.camera.pixelHeight,0));

					transform.position = HandleCollisionZoom(transform.position, lookAtPt, minDistance, ref frustumRays);
				}
		}
	
		// returns a new camera position
		Vector3 HandleCollisionZoom(Vector3 camPos, Vector3 targetPos, 
		                            float minOffsetDist, ref Ray[] frustumRays) {
			float offsetDist = Vector3.Magnitude(targetPos - camPos); 
			float raycastLength = offsetDist - minOffsetDist;
			RaycastHit hit;
			if (raycastLength < 0.0f) {
				// camera is already too near the lookat target
				return camPos;
			}
			
			Vector3 camOut = Vector3.Normalize(targetPos - camPos);
			Vector3 nearestCamPos = targetPos - camOut * minOffsetDist;
			float minHitFraction = 1.0f;
			
			for (int i = 0; i < 4; ++i) {
				Vector3 corner = frustumRays[i].origin;
				Vector3 offsetToCorner = corner - camPos;
				Vector3 rayStart = nearestCamPos + offsetToCorner;
				Vector3 rayEnd = corner;
	
				Debug.DrawRay(rayStart, rayEnd-rayStart);
			
				// a result between 0 and 1 indicates a hit along the ray segment
				Physics.Raycast(new Ray(rayStart,rayEnd), out hit);
				float hitFraction = hit.distance;
				minHitFraction = Mathf.Min(hitFraction, minHitFraction);
			}        
			
			if (minHitFraction < 1.0f) {
				return nearestCamPos - camOut * (raycastLength * minHitFraction);
			}
			else {
				return camPos;
			}
		}
}
	