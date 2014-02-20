using UnityEngine;
using System.Collections;

// Mode for being able to hit the ball and send it flying.

public class SwingMode : MonoBehaviour
{
	
		public GameObject camera;
		private Vector3 cameraPos = new Vector3 (0, 2, -4);
		private float shotPower = 0f;
		public int hitMultiplier = 5;
		private bool flying = false;
		public GameObject cart;

	
		// Use this for initialization
		void Start ()
		{
			
		}

	// Draws the ugly GUI Box that tells you how hard you are about to hit.
		void OnGUI ()
		{
				GUI.Box (new Rect (200, 200, 100, 100), "power: " + (int)shotPower);
		}

	
		// Update is called once per frame
		void Update ()
		{
				// Input hardcoded for now
				if (Input.GetKey (KeyCode.D)) {
						gameObject.transform.Rotate (0f, 1f, 0f);
						rigidbody.freezeRotation = true;
				}
				if (Input.GetKey (KeyCode.A)) {
						gameObject.transform.Rotate (0f, -1f, 0f);
						rigidbody.freezeRotation = true;
				}
		
				// Crappy camera script taken from the original movement.cs. Makes rotation around the ball possible.
				Vector3 newPos = transform.position + transform.localRotation * cameraPos;
				float lerper = Mathf.Min ((camera.transform.position - newPos).sqrMagnitude / 100, 1);
				camera.transform.position = (1 - lerper) * camera.transform.position + lerper * newPos;
				camera.transform.rotation = Quaternion.Lerp (camera.transform.rotation, Quaternion.LookRotation (transform.position - camera.transform.position), lerper);

				// if we are in the air, we don't want player to hit again. Somewhat obsolete now that control returns to car when swung.
				if (flying) {
						return;
				}

				// You can only add power. Automatically decrementing it is not implemented.
				if (Input.GetAxis ("Vertical") > 0) {
						shotPower += Input.GetAxis ("Vertical") * hitMultiplier;
						if (shotPower > 500) {
								shotPower = 500;
						}
				}

				// This is where the swing happens.
				if (Input.GetKeyDown (KeyCode.Space)) {
						flying = true;
						
						// flies slow in a high arc. Needs tuning.
						Vector3 arc = Vector3.forward;
						if (shotPower < 100) {
								shotPower = 100;
						}
						arc.y = arc.y + shotPower / 200;
						rigidbody.AddForce (transform.localRotation * arc * shotPower);
						shotPower = 0;

						// Turn on control scripts and camera on the cart.
						cart.SendMessage("turnOnScripts");
						// Turn off control scripts (including this one) and camera on the ball.
						this.gameObject.SendMessage("turnOffScripts");
				}
		}

		// Figures out when the ball has landed.
		void OnCollisionEnter (Collision col)
		{
				if (col.gameObject.tag == "Ground") {
						flying = false;
				}
		}
}
