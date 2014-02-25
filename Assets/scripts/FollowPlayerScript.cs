using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Follow Player")]
public class FollowPlayerScript : MonoBehaviour
{

		//public GameObject car;
		public Transform target;
		// The distance in the x-z plane to the target
		public float distance = 5.0f;
		// the height we want the camera to be above the target
		public float height = 4.0f;

		public float heightDamping = 2.0f;
		public float rotationDamping = 3.0f;
		public float leanDamping = 2.0f;

		// To adjust camera angle, in order to see ahead while driving.
		public float cameraTilt = 1.5f;
		public float minDistance = 3.0f;

	    // zoom camera if near plane corner collide with geometry before reaching target
		public bool camZoom = false;

	    // lean camera toward zenth if sphere collides with geometry
		public bool camLean = true;
		public float camCollideRadius = 2.0f;

	    // current distance on the x-z plane of the camera from the target
		private float currentDistance = 0f;
	
		// Use this for initialization
		void Start () {
			//target = car.transform;	
			currentDistance = distance;
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

		        if( camLean ) {
		        	
			        // calculate the squared length of the vector from the camera to the target
					float wantedCamDistanceSquared = (wantedHeight-cameraTilt);
					wantedCamDistanceSquared *= wantedCamDistanceSquared;
					wantedCamDistanceSquared = wantedCamDistanceSquared + distance*distance;

					if( Physics.CheckSphere( transform.position, camCollideRadius) )
					{
						// move toward zero distance on x-y from target
						currentDistance = Mathf.Lerp(currentDistance, 0, leanDamping * Time.deltaTime);
					}
					else
					{
						// move toward user defined distance on x-y from target
						currentDistance = Mathf.Lerp(currentDistance, distance, leanDamping * Time.deltaTime);
					}

			        // calculate the required height to retain the retain the same camera distance [b = sqrt(h^2 - a^2)]
					wantedHeight = Mathf.Sqrt(wantedCamDistanceSquared - currentDistance*currentDistance) + cameraTilt;     
				}

				// Set the position of the camera on the x-z plane to:
				// distance meters behind the target
				transform.position = target.position;

                if( currentDistance > 0 )
                {
					// Damp the rotation around the y-axis
					currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
			
					// Convert the angle into a rotation
					Quaternion currentRotation = Quaternion.Euler (0f, currentRotationAngle, 0f);

	                // rotate vector from target by angle
					transform.position -= currentRotation * Vector3.forward * currentDistance;
				}

				// Damp the height
				currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);
		
				// Set the height of the camera
				transform.position = new Vector3 (transform.position.x, currentHeight, transform.position.z);				

				// Always look a bit (value of cameraTilt) over the target.
				Vector3 lookAtPt = new Vector3(target.position.x, target.position.y + cameraTilt, target.position.z);
				transform.LookAt(lookAtPt);

				if( camZoom ) {
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
	