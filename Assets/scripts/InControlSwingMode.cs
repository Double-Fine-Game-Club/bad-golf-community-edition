using UnityEngine;
using InControl;
using System;
using System.Collections;

public abstract class SwingBehaviour : MonoBehaviour
{
	public GameObject cameraObject, cart;
	public int hitMultiplier = 10;
	public const int k_maxShotPower = 500;
	public const int k_maxArcAngle = 80;
	public const int k_minArcAngle = 35;
	public const int k_shotBoost = 3;
	
	public abstract float GetShotPower ();
}

public class InControlSwingMode : SwingBehaviour
{		
	private Vector3 cameraPos = new Vector3 (0, 2, -4);
	private bool flying = false;
	private float shotPower = 0.0f;
	private float shotAngle = 0.0f;

	private InputDevice ourAttachedDevice;

	// Use this for initialization
	void Start ()
	{
			//XInput only works on windows
			//InputManager.EnableXInput = true;

			if ( !InControlSetup.hasBeenSetup )
			{
				InputManager.Setup ();
				InControlSetup.hasBeenSetup = true;
			}

			// Add a custom device profile.
			InputManager.AttachDevice (new UnityInputDevice (new SwingModeProfile ()));

			if ( cart.GetComponent<CarUserControl>().inputDevice != null)	
				ourAttachedDevice = cart.GetComponent<CarUserControl>().inputDevice;
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
		if (flying) 
		{
				Screen.lockCursor = false;
		
				// Turn on control scripts and camera on the cart.
				cart.SendMessage ("turnOnScripts");
				// Turn off control scripts (including this one) and camera on the ball.
				this.gameObject.SendMessage ("turnOffScripts");
				return;
		}
		InputManager.Update ();
		
		// Use last device which provided input.
		InputDevice inputDevice;
		if ( ourAttachedDevice != null)
			inputDevice = ourAttachedDevice;
		else
		{				
			inputDevice = InputManager.ActiveDevice;
		}
		
		if ( cart.GetComponent<CarUserControl>().isLocalMulti && ourAttachedDevice == null )
		{
			//dont use controller, use mouse / keyboard only
			//code copied from SwingMode / heavily modified

			//rotate camera around ball using horizontal axis
			gameObject.transform.Rotate (0f, Input.GetAxis ("Horizontal"), 0f);
			rigidbody.freezeRotation = true;
			
			// Crappy camera script taken from the original movement.cs. Makes rotation around the ball possible.
			Vector3 newPos = transform.position + transform.localRotation * cameraPos;
			float lerper = Mathf.Min ((cameraObject.transform.position - newPos).sqrMagnitude / 100, 1);
			cameraObject.transform.position = (1 - lerper) * cameraObject.transform.position + lerper * newPos;
			cameraObject.transform.rotation = Quaternion.Lerp (cameraObject.transform.rotation, Quaternion.LookRotation (transform.position - cameraObject.transform.position), lerper);
			
			// if we are in the air, we don't want player to hit again. Somewhat obsolete now that control returns to car when swung.
			if (flying) 
			{
				return;
			}
			
			//add remove power with vertical axis
			shotPower += Input.GetAxis ("Vertical") * hitMultiplier;
			shotPower = Mathf.Clamp( shotPower, 0, k_maxShotPower);
			
			// flies slow in a high arc. Needs tuning.
			// elneilios: Tuned this so that harder shots fly straighter (like real golf!)
			var angleModifier = (shotPower / k_maxShotPower);
			var angleRange = k_maxArcAngle - k_minArcAngle;
			shotAngle = k_maxArcAngle - (angleRange * angleModifier);	
			
			// This is where the swing happens.
			if (Input.GetKeyDown (KeyCode.Space))
			{
				flying = true;
				if (shotPower < 100)
					shotPower = 100;
				
				Vector3 arc = Vector3.forward;
				arc.y = 0;
				arc.Normalize ();
				arc.y = Mathf.Sin (shotAngle * Mathf.Deg2Rad);
				rigidbody.AddForce (transform.localRotation * arc * shotPower * k_shotBoost);
				shotPower = 0;	
			}
		}
		else //allow controller for online
		{			
			// Capture mouse while in this mode
			Screen.lockCursor = true;

			// Rotate camera object with both sticks and d-pad.
			gameObject.transform.Rotate (Vector3.up, 200.0f * Time.deltaTime * inputDevice.Direction.x, Space.Self);
			gameObject.transform.Rotate (Vector3.up, 200.0f * Time.deltaTime * inputDevice.LeftStickX, Space.Self);

			// Crappy camera script taken from the original movement.cs. Makes rotation around the ball possible.
			Vector3 newPos = transform.position + transform.localRotation * cameraPos;
			float lerper = Mathf.Min ((cameraObject.transform.position - newPos).sqrMagnitude / 100, 1);
			cameraObject.transform.position = (1 - lerper) * cameraObject.transform.position + lerper * newPos;
			cameraObject.transform.rotation = Quaternion.Lerp (cameraObject.transform.rotation, Quaternion.LookRotation (transform.position - cameraObject.transform.position), lerper);

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
			if (inputDevice.Action1 || inputDevice.Action2) 
			{
				flying = true;
				if (shotPower < 100)
					shotPower = 100;

				Vector3 arc = Vector3.forward;
				arc.y = 0;
				arc.Normalize ();
				arc.y = Mathf.Sin (shotAngle * Mathf.Deg2Rad);
				rigidbody.AddForce (transform.localRotation * arc * shotPower * k_shotBoost);
				shotPower = 0;	
			}
		}	
	}

	// Figures out when the ball has landed.
	void OnCollisionEnter (Collision col)
	{
		if (col.gameObject.tag == "Ground") 
		{
			flying = false;
		}
	}

	public override float GetShotPower ()
	{
		return shotPower;
	}

	//#if UNITY_ANDROID || UNITY_IPHONE
	public void directionUpdate( Vector2 direction)
	{
		if (enabled)
		{
			// Rotate camera object with both sticks and d-pad.
			gameObject.transform.Rotate (Vector3.up, 200.0f * Time.deltaTime * direction.x, Space.Self);
			
			// Crappy camera script taken from the original movement.cs. Makes rotation around the ball possible.
			Vector3 newPos = transform.position + transform.localRotation * cameraPos;
			float lerper = Mathf.Min ((cameraObject.transform.position - newPos).sqrMagnitude / 100, 1);
			cameraObject.transform.position = (1 - lerper) * cameraObject.transform.position + lerper * newPos;
			cameraObject.transform.rotation = Quaternion.Lerp (cameraObject.transform.rotation, Quaternion.LookRotation (transform.position - cameraObject.transform.position), lerper);
			
			shotPower += direction.y * hitMultiplier;
			
			if (shotPower > k_maxShotPower)
				shotPower = k_maxShotPower;
			else if (shotPower < 0)
				shotPower = 0;
			
			// flies slow in a high arc. Needs tuning.
			// elneilios: Tuned this so that harder shots fly straighter (like real golf!)
			var angleModifier = (shotPower / k_maxShotPower);
			var angleRange = k_maxArcAngle - k_minArcAngle;
			shotAngle = k_maxArcAngle - (angleRange * angleModifier);
		}	
	}


	public void onUserGamePadButton()
	{
		if (enabled)
		{
			flying = true;
			if (shotPower < 100)
				shotPower = 100;
			
			Vector3 arc = Vector3.forward;
			arc.y = 0;
			arc.Normalize ();
			arc.y = Mathf.Sin (shotAngle * Mathf.Deg2Rad);
			rigidbody.AddForce (transform.localRotation * arc * shotPower * k_shotBoost);
			shotPower = 0;	
		}
	}
	//#endif
}

