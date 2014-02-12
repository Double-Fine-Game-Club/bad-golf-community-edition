using UnityEngine;
using System.Collections;

public class movement : MonoBehaviour {
	float forceMultiplyer = 10000;
	GameObject cameraGameObject;
	Vector3 cameraPos = new Vector3(0,2,-4);

	// Use this for initialization
	void Start () {
		cameraGameObject = GameObject.FindGameObjectWithTag("MainCamera");
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 forceFromFront = new Vector3();	// force from front tires
		Vector3 forceFromBack = new Vector3();	// force from back tires
		if (Input.GetKey(KeyCode.W)) {
			// make sure it's facing the direction of the vehicle
			forceFromFront += transform.localRotation * Vector3.forward;
			forceFromBack += transform.localRotation * Vector3.forward;
		}
		if (Input.GetKey(KeyCode.S)) {
			// make sure it's facing the direction of the vehicle
			forceFromFront += transform.localRotation * Vector3.back;
			forceFromBack += transform.localRotation * Vector3.back;
		}
		if (Input.GetKey(KeyCode.A)) {
			// rotate the front forces if they are turning
			forceFromFront = Quaternion.AngleAxis(-60,Vector3.up) * forceFromFront;
		}
		if (Input.GetKey(KeyCode.D)) {
			// rotate the front forces if they are turning
			forceFromFront = Quaternion.AngleAxis(60,Vector3.up) * forceFromFront;
		}
		if (forceFromFront.sqrMagnitude!=0) {
			rigidbody.AddForceAtPosition(forceMultiplyer*forceFromFront.normalized,transform.position+transform.localRotation*Vector3.forward);
			rigidbody.AddForceAtPosition(forceMultiplyer*forceFromFront.normalized,transform.position+transform.localRotation*Vector3.forward);
			rigidbody.AddForceAtPosition(forceMultiplyer*forceFromBack.normalized,transform.position+transform.localRotation*Vector3.back);
			rigidbody.AddForceAtPosition(forceMultiplyer*forceFromBack.normalized,transform.position+transform.localRotation*Vector3.back);
		}

		// camera movement
		Vector3 newPos = transform.position + transform.localRotation * cameraPos;
		float lerper = Mathf.Min((cameraGameObject.transform.position - newPos).sqrMagnitude / 100, 1);
		cameraGameObject.transform.position = (1-lerper)*cameraGameObject.transform.position + lerper*newPos;
		cameraGameObject.transform.rotation = Quaternion.Lerp(cameraGameObject.transform.rotation, Quaternion.LookRotation(transform.position-cameraGameObject.transform.position), lerper);
	}
}
