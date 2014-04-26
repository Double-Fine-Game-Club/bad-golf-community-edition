using UnityEngine;
using System.Collections;

public class SpectatorView : MonoBehaviour {
	// temp fix for tuesday - change later
	void FixedUpdate()
	{
		if (Input.GetKey (KeyCode.W)) {
			transform.position += 50 * transform.forward * Time.fixedDeltaTime;
		}
		if (Input.GetKey (KeyCode.S)) {
			transform.position -= 50 * transform.forward * Time.fixedDeltaTime;
		}
		if (Input.GetKey (KeyCode.A)) {
			transform.position -= 50 * transform.right * Time.fixedDeltaTime;
		}
		if (Input.GetKey (KeyCode.D)) {
			transform.position += 50 * transform.right * Time.fixedDeltaTime;
		}
		if (Input.GetMouseButton(0)) {
			transform.Rotate(0,Input.GetAxis("Mouse X") * 500 * Time.fixedDeltaTime,0, Space.World);
			transform.Rotate(-Input.GetAxis("Mouse Y") * 500 * Time.fixedDeltaTime,0,0, Space.Self);
			// if we rotate too far
			if (Mathf.Abs((transform.rotation.eulerAngles.x+180)%360-180)>60) {
				// rotate back
				transform.Rotate(Input.GetAxis("Mouse Y") * 500 * Time.fixedDeltaTime,0,0, Space.Self);
			}
		}
	}
}
