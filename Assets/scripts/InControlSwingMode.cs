using UnityEngine;
using InControl;
using System;
using System.Collections;

public abstract class SwingBehaviour : MonoBehaviour
{
		public GameObject camera, cart;
		public int hitMultiplier = 10;
		public const int k_maxShotPower = 500;
		public const int k_maxArcAngle = 80;
		public const int k_minArcAngle = 35;
		public const int k_shotBoost = 3;
		
		public abstract float GetShowPower ();
}

public class InControlSwingMode : SwingBehaviour
{		
		private Vector3 cameraPos = new Vector3 (0, 2, -4);
		private bool flying = false;
		private float shotPower = 0.0f;
		private float shotAngle = 0.0f;
	
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
				GUI.Box (new Rect (200, 200, 100, 100), "power: " + (int)shotPower + "\nangle: " + (int)shotAngle);
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
				gameObject.transform.Rotate (Vector3.up, 200.0f * Time.deltaTime * inputDevice.Direction.x, Space.Self);
				gameObject.transform.Rotate (Vector3.up, 200.0f * Time.deltaTime * inputDevice.LeftStickX, Space.Self);

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

				// flies slow in a high arc. Needs tuning.
				// elneilios: Tuned this so that harder shots fly straighter (like real golf!)
				var angleModifier = (shotPower / k_maxShotPower);
				var angleRange = k_maxArcAngle - k_minArcAngle;
				shotAngle = k_maxArcAngle - (angleRange * angleModifier);	
		
				// This is where the swing happens.
				if (inputDevice.Action1 || inputDevice.Action2) {
						flying = true;
						if (shotPower < 100)
								shotPower = 100;

						Vector3 arc = Vector3.forward;
						arc.y = 0;
						arc.Normalize ();
						arc.y = Mathf.Sin (shotAngle * Mathf.Deg2Rad);
						;
						rigidbody.AddForce (transform.localRotation * arc * shotPower * k_shotBoost);
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

