// This camera positions the camera with the same position as the object for some of the XYZ plane components.

using UnityEngine;
using System.Collections;

public class FollowPositionScript : MonoBehaviour {
	
	// The target we are following
	public Transform target;

	public enum PlaneType {XY, XZ, YZ}

	// Which plane to follow the position
	public PlaneType followPlane = PlaneType.XZ;

	public float rotationDamping = 3.0f;

	Vector3 startPosition;

	// Use this for initialization
	void Start () {
		startPosition = gameObject.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void LateUpdate () {
		// Early out if we don't have a target
		if (target == null) {
			return;
		}

		Vector3 position = gameObject.transform.position;
		Quaternion rotation = gameObject.transform.rotation;
		Vector3 rotationEulerAngles = rotation.eulerAngles;
		
		if (followPlane == PlaneType.XZ) {
			position.x = target.position.x;
			position.z = target.position.z;
			rotationEulerAngles.y = Mathf.LerpAngle(rotationEulerAngles.y, target.eulerAngles.y, rotationDamping * Time.deltaTime);
			position.y = startPosition.y;
		}
		else if (followPlane == PlaneType.XY) {
			position.x = target.position.x;
			position.z = target.position.y;
			rotationEulerAngles.z = Mathf.LerpAngle(rotationEulerAngles.z, target.eulerAngles.z, rotationDamping * Time.deltaTime);
			position.z = startPosition.z;
		}
		else if (followPlane == PlaneType.YZ) {
			position.x = target.position.y;
			position.z = target.position.z;
			rotationEulerAngles.x = Mathf.LerpAngle(rotationEulerAngles.x, target.eulerAngles.x, rotationDamping * Time.deltaTime);
			position.x = startPosition.x;
		}
		
		rotation.eulerAngles = rotationEulerAngles;

		gameObject.transform.position = position;
		gameObject.transform.rotation = rotation;

	}
}
