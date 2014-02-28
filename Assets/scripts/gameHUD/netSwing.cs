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
		nvs = (GetComponent(typeof(networkPlayerLoad))as networkPlayerLoad).nvs as networkVariables;
		myInfo = nvs.myInfo;
		pause = GetComponent ("netPause") as netPause;

		// add scripts that the ball needs
			
			// InControll Swing
			InControlSwingMode swingBall = myInfo.ballGameObject.AddComponent (typeof(InControlSwingMode)) as InControlSwingMode;
			swingBall.camera = myInfo.ballGameObject.transform.FindChild("hit_ball_camera").gameObject;
			swingBall.hitMultiplier = 15;
			swingBall.enabled = false;		
	}



	void FixedUpdate(){
		//Debug.Log ("Cart Vel: " + gameObject.rigidbody.velocity);
		float distance = Vector3.Distance (myInfo.cartGameObject.transform.position, myInfo.ballGameObject.transform.position);
		
		// Check Distance to ball
		if (distance < 5) {
			inHittingRange = true;
		} else {
			inHittingRange = false;
		}
	
		// Left Mouse Press at ball
		if (Input.GetMouseButtonDown (1)) {
			myInfo.currentMode = 0;
		}
		
		if (Input.GetKeyDown (KeyCode.Space)) {
			myInfo.currentMode = 0;
		}
		
		if (Input.GetKeyDown(KeyCode.E)) {
			// if in buggy
			if (myInfo.currentMode==0 && inHittingRange==true) {
				myInfo.currentMode = 1;
				// set them at golf ball
				//myInfo.ballGameObject.transform.rotation = Quaternion.identity;		// reset rotation to make it nice
				//myInfo.characterGameObject.transform.parent = myInfo.ballGameObject.transform;
				myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,-2);
				//myInfo.characterGameObject.transform.rotation = Quaternion.identity;
				// change animation
				//myInfo.characterGameObject.transform.FindChild("lil_patrick").animation.Play("golfIdle",PlayMode.StopAll);
				networkPlayerLoad netPlay = new networkPlayerLoad();
				myInfo.cartGameObject.rigidbody.velocity = Vector3.zero;
				netPlay.BallScriptToggler (myInfo,true);
				netPlay.CarScriptToggler (myInfo, false);


						
				// tell everyone
				networkView.RPC("PlayerSwap", RPCMode.Others, myInfo.currentMode, myInfo.player);
				
				
				// if at ball
			} else if (myInfo.currentMode==1) {
				myInfo.currentMode = 0;
				// set them in buggy
				//myInfo.characterGameObject.transform.parent = myInfo.cartGameObject.transform;
				myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,0);
				//myInfo.characterGameObject.transform.rotation = myInfo.cartGameObject.transform.rotation;
				// unlock golf ball
				networkPlayerLoad netPlay = new networkPlayerLoad();
				myInfo.cartGameObject.rigidbody.velocity = Vector3.zero;
				netPlay.BallScriptToggler (myInfo,false);
				netPlay.CarScriptToggler (myInfo, true);

				// change animation
				//myInfo.characterGameObject.transform.FindChild("lil_patrick").animation.Play("driveIdle",PlayMode.StopAll);

				// tell everyone
				networkView.RPC("PlayerSwap", RPCMode.Others, myInfo.currentMode, myInfo.player);
				
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