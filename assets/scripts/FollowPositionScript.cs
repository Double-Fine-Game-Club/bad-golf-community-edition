// This camera positions the camera with the same position as the object for some of the XYZ plane components.

using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Follow Position")]
public class FollowPositionScript : MonoBehaviour {

	// The target we are following
	public Transform target;

	// Whether we want to follow the object x component
	public bool followX = true;
	// Whether we want to follow the object x component
	public bool followY = false;
	// Whether we want to follow the object z component
	public bool followZ = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void LateUpdate () {
		// Early out if we don't have a target
		if (!target) {
			return;
		}

		Vector3 position = gameObject.transform.position;

		if (followX) {
			position.x = target.position.x;
		}

		if (followY) {
			position.y = target.position.y;
		}

		if (followZ) {
			position.z = target.position.z;
		}

		gameObject.transform.position = position;
	}
}
