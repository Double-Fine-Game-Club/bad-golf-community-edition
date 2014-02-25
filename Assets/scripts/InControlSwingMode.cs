using UnityEngine;
using InControl;
using System.Collections;

public abstract class SwingBehaviour : MonoBehaviour
{
		public GameObject camera, cart;
		public int hitMultiplier = 10;
		public const int k_maxShotPower = 500;

		public abstract float GetShowPower ();
}

public class InControlSwingMode : SwingBehaviour
{		
		private Vector3 cameraPos = new Vector3 (0, 2, -4);
		private bool flying = false;
		private float shotPower = 0.0f;		
	
		// Use this for initialization
		void Start ()
		{
				//InputManager.EnableXInput = true;
				InputManager.Setup ();

				// Add a custom device profile.
				InputManager.AttachDevice (new UnityInputDevice (new SwingModeProfile ()));
				
		}

		// Draws the ugly GUI Box that tells you how hard you are about to hit.
		void OnGUI ()
		{
				GUI.Box (new Rect (200, 200, 100, 100), "power: " + (int)shotPower);
		}

		// Update is called once per frame
		void Update ()
		{
				// if we are in the air, we don't want player to hit again. Somewhat obsolete now that control returns to car when swung.
				if (flying) {
						Screen.lockCursor = false;
			
						// Turn on control scripts and camera on the cart.
						cart.SendMessage ("turnOnScripts");
						// Turn off control scripts (including this one) and camera on the ball.
						this.gameObject.SendMessage ("turnOffScripts");
						return;
				}
				InputManager.Update ();
		
				// Use last device which provided input.
				var inputDevice = InputManager.ActiveDevice;

				// Capture mouse while in this mode
				Screen.lockCursor = true;

				// Rotate camera object with both sticks and d-pad.
				gameObject.transform.Rotate (Vector3.down, 200.0f * Time.deltaTime * inputDevice.Direction.x, Space.World);
				gameObject.transform.Rotate (Vector3.down, 200.0f * Time.deltaTime * inputDevice.LeftStickX, Space.World);

				// Crappy camera script taken from the original movement.cs. Makes rotation around the ball possible.
				Vector3 newPos = transform.position + transform.localRotation * cameraPos;
				float lerper = Mathf.Min ((camera.transform.position - newPos).sqrMagnitude / 100, 1);
				camera.transform.position = (1 - lerper) * camera.transform.position + lerper * newPos;
				camera.transform.rotation = Quaternion.Lerp (camera.transform.rotation, Quaternion.LookRotation (transform.position - camera.transform.position), lerper);

				

				shotPower += inputDevice.Direction.y * hitMultiplier;
				shotPower += inputDevice.RightStickY * hitMultiplier;

				if (shotPower > k_maxShotPower)
						shotPower = k_maxShotPower;
				else if (shotPower < 0)
						shotPower = 0;

				// This is where the swing happens.
				if (inputDevice.Action1 || inputDevice.Action2) {
						flying = true;
			
						// flies slow in a high arc. Needs tuning.
						Vector3 arc = Vector3.forward;
						if (shotPower < 100) {
								shotPower = 100;
						}
						arc.y = arc.y + shotPower / 200;
						rigidbody.AddForce (transform.localRotation * arc * shotPower);
						shotPower = 0;			
						
				}

		}

		// Figures out when the ball has landed.
		void OnCollisionEnter (Collision col)
		{
				if (col.gameObject.tag == "Ground") {
						flying = false;
				}
		}
	
		public override float GetShowPower ()
		{
				return shotPower;
		}
}

