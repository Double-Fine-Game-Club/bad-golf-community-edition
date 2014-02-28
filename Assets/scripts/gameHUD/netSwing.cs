using UnityEngine;
using System.Collections;

public class netSwing : MonoBehaviour {
	networkVariables nvs;
	PlayerInfo myInfo;
	netPause pause;
	// For updating the GUI Box.
	private bool inHittingRange = false;

	// Use this for initialization
	void Start () {
		// get variables we need
		nvs = GetComponent("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		pause = GetComponent ("netPause") as netPause;

		// THESE NEED FIXING FOR MULTIPLAYER
		// add the script to the ball
		//InControlSwingMode ballSwing = nvs.myInfo.ballGameObject.AddComponent(typeof(InControlSwingMode)) as InControlSwingMode;
		// set things that the script needs
		//ballSwing.camera = nvs.myCam.gameObject;
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//Debug.Log ("Cart Vel: " + gameObject.rigidbody.velocity);
		float distance = Vector3.Distance (myInfo.cartGameObject.transform.position, myInfo.ballGameObject.transform.position);
		if (distance < 5) {
			inHittingRange = true;
		} else {
			inHittingRange = false;
		}
		if (Input.GetKeyDown(KeyCode.E)) {
			//if (inHittingRange) {
				// Stop the cart's forward motion (still might roll away though)
				//gameObject.rigidbody.velocity = Vector3.zero;
				//ball.SendMessage ("turnOnScripts");
				//this.gameObject.SendMessage("turnOffScripts");

			// if in buggy and in hitting range
			if (myInfo.currentMode==0 && inHittingRange) {
				myInfo.currentMode = 1;
				// set them at golf ball
				myInfo.characterGameObject.transform.parent = myInfo.ballGameObject.transform;
				myInfo.ballGameObject.transform.rotation = Quaternion.identity;		// reset rotation to make it nice
				myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,-2);
				myInfo.characterGameObject.transform.rotation = Quaternion.identity;
				// lock golf ball
				myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
				// change animation
				myInfo.characterGameObject.transform.FindChild("lil_patrick").animation.Play("golfIdle",PlayMode.StopAll);
				// tell everyone
				networkView.RPC("PlayerSwap", RPCMode.Others, myInfo.currentMode, myInfo.player);
				// Stop the cart's forward motion and stop it from rolling away
				myInfo.cartGameObject.rigidbody.velocity = Vector3.zero;

				myInfo.cartGameObject.rigidbody.velocity = Vector3.zero;
				myInfo.ballGameObject.SendMessage ("turnOnScripts");
				myInfo.cartGameObject.SendMessage("turnOffScripts");
				CarUserControl carCtrl = myInfo.cartGameObject.GetComponent(typeof(CarUserControl)) as CarUserControl;
				carCtrl.enabled = false;

				/*
				myInfo.cartGameObject.rigidbody.constraints = RigidbodyConstraints.FreezeAll;

				// enable the scripts on the ball
				PowerMeter bpm = myInfo.ballGameObject.GetComponent("PowerMeter") as PowerMeter;
				bpm.enabled = true;*/

				
				// if at ball
			} else if (myInfo.currentMode==1) {
				myInfo.currentMode = 0;
				// set them in buggy
				myInfo.characterGameObject.transform.parent = myInfo.cartGameObject.transform;
				myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,0);
				myInfo.characterGameObject.transform.rotation = myInfo.cartGameObject.transform.rotation;
				// unlock golf ball
				myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.None;
				// change animation
				myInfo.characterGameObject.transform.FindChild("lil_patrick").animation.Play("driveIdle",PlayMode.StopAll);
				// tell everyone
				networkView.RPC("PlayerSwap", RPCMode.Others, myInfo.currentMode, myInfo.player);

				myInfo.ballGameObject.SendMessage("turnOffScripts");
				myInfo.cartGameObject.SendMessage("turnOnScripts");
				CarUserControl carCtrl = myInfo.cartGameObject.GetComponent(typeof(CarUserControl)) as CarUserControl;
				carCtrl.enabled = true;

				/*
				// undo cart lock
				myInfo.cartGameObject.rigidbody.constraints = RigidbodyConstraints.None;
				
				// disable the scripts on the ball
				PowerMeter bpm = myInfo.ballGameObject.GetComponent("PowerMeter") as PowerMeter;
				bpm.enabled = false;*/
			}
		}
	}
	
	// Makes the ugly GUI box that tells when you are close enough to the ball.
	void OnGUI ()
	{
		GUI.Box (new Rect (200, 200, 100, 100), "in range: " + inHittingRange);
	}


	// when a player changes mode - player needed since it would comes from the server!
	[RPC]
	void PlayerSwap(int newMode, NetworkPlayer player) {
		// find the player
		foreach (PlayerInfo p in nvs.players)
		{
			if (p.player==player) {
				p.currentMode = newMode;
				if (p.currentMode==0) {			// if they're now in a buggy
					// set them in buggy
					p.characterGameObject.transform.parent = p.cartGameObject.transform;
					p.characterGameObject.transform.localPosition = new Vector3(0,0,0);
					p.characterGameObject.transform.rotation = p.cartGameObject.transform.rotation;
					// unlock golf ball
					p.ballGameObject.rigidbody.constraints = RigidbodyConstraints.None;
					// change animation
					p.characterGameObject.transform.FindChild("lil_patrick").animation.Play("driveIdle",PlayMode.StopAll);
					
				} else if (p.currentMode==1) {	// if they're now at golf ball
					// set them at golf ball
					p.characterGameObject.transform.parent = p.ballGameObject.transform;
					p.ballGameObject.transform.rotation = Quaternion.identity;		// reset rotation to make it nice
					p.characterGameObject.transform.localPosition = new Vector3(0,0,-2);
					p.characterGameObject.transform.rotation = Quaternion.identity;
					// lock golf ball
					p.ballGameObject.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
					// change animation
					p.characterGameObject.transform.FindChild("lil_patrick").animation.Play("golfIdle",PlayMode.StopAll);
				}
				
				// reset keyboard buffer
				p.h = 0f;
				p.v = 0f;
			}
		}
	}
}