using UnityEngine;
using System.Collections;

public class MovementWithSwing : MonoBehaviour
{
	float forceMultiplyer = 10000;
	Vector3 cameraPos = new Vector3 (0, 2, -4);
	public GameObject ball;
	public GameObject camera;
	private bool inHittingRange = false;
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 forceFromFront = new Vector3 ();	// force from front tires
		Vector3 forceFromBack = new Vector3 ();	// force from back tires
		if (Input.GetKey (KeyCode.W)) {
			// make sure it's facing the direction of the vehicle
			forceFromFront += transform.localRotation * Vector3.forward;
			forceFromBack += transform.localRotation * Vector3.forward;
		}
		if (Input.GetKey (KeyCode.S)) {
			// make sure it's facing the direction of the vehicle
			forceFromFront += transform.localRotation * Vector3.back;
			forceFromBack += transform.localRotation * Vector3.back;
		}
		if (Input.GetKey (KeyCode.A)) {
			// rotate the front forces if they are turning
			forceFromFront = Quaternion.AngleAxis (-60, Vector3.up) * forceFromFront;
		}
		if (Input.GetKey (KeyCode.D)) {
			// rotate the front forces if they are turning
			forceFromFront = Quaternion.AngleAxis (60, Vector3.up) * forceFromFront;
		}
		float distance = Vector3.Distance (gameObject.transform.position, ball.transform.position);
		if (distance < 5) {
			inHittingRange = true;
		} else {
			inHittingRange = false;
		}
		if (Input.GetKey (KeyCode.E)) {
			if (inHittingRange) {
				ball.SendMessage ("toggleScript");
				camera.SetActive (false);
				this.enabled = false;
			}
		}
		
		if (forceFromFront.sqrMagnitude != 0) {
			rigidbody.AddForceAtPosition (forceMultiplyer * forceFromFront.normalized, transform.position + transform.localRotation * Vector3.forward);
			rigidbody.AddForceAtPosition (forceMultiplyer * forceFromFront.normalized, transform.position + transform.localRotation * Vector3.forward);
			rigidbody.AddForceAtPosition (forceMultiplyer * forceFromBack.normalized, transform.position + transform.localRotation * Vector3.back);
			rigidbody.AddForceAtPosition (forceMultiplyer * forceFromBack.normalized, transform.position + transform.localRotation * Vector3.back);
		}
	}
	
	void OnGUI ()
	{
		GUI.Box (new Rect (200, 200, 100, 100), "in range: " + inHittingRange);
	}
}
