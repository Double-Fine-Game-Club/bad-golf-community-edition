using UnityEngine;
using System.Collections;

public class controlClient : MonoBehaviour {
	float timer = 0;
	PlayerInfo myInfo;
	networkVariables nvs;
	netPause pause;

	void Start() {
		// get variables we need
		nvs = GetComponent("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		pause = GetComponent ("netPause") as netPause;
		
		// should probably remove this...
//		GameObject.Find ("lobby_view").transform.FindChild ("camera").gameObject.SetActive (false);
//		myInfo.cartContainerObject.transform.FindChild ("multi_buggy_cam").gameObject.SetActive (true);
	}

	void Update () {
		// only do key presses if it's not paused
		if (!myInfo.playerIsPaused) {
			// if in buggy
			if (myInfo.currentMode==0) {
				// send packets about keyboard every 0.015s
				timer += Time.deltaTime;
				if (timer > 0.015) {
					timer = 0;
					float h = Input.GetAxis("Horizontal");
					float v = Input.GetAxis("Vertical");
					networkView.RPC("KartMovement", RPCMode.Server, h,v);
				}
				// HONK
				if (Input.GetKeyDown(KeyCode.Q)) {
					networkView.RPC("IHonked", RPCMode.All, myInfo.player);
				}
			}
			// (G)et out of buggy (or get in)
			if (Input.GetKeyDown(KeyCode.G)) {
				// if in buggy
				if (myInfo.currentMode==0) {
					myInfo.currentMode = 1;
					// set them at golf ball
					myInfo.characterGameObject.transform.parent = myInfo.ballGameObject.transform;
					myInfo.ballGameObject.transform.rotation = Quaternion.identity;		// reset rotation to make it nice
					myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,-2);
					myInfo.characterGameObject.transform.rotation = Quaternion.identity;
					// lock golf ball
					myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
					//*/ change animation
					myInfo.characterGameObject.transform.FindChild("lil_patrick").animation.Play("golfIdle",PlayMode.StopAll);
					//*/ move camera - HACKY
					GameObject buggyCam = nvs.myCam.gameObject;
					buggyCam.transform.parent = myInfo.ballGameObject.transform;
					buggyCam.transform.rotation = Quaternion.identity;
					buggyCam.transform.localPosition = new Vector3(-6,4,0);
					buggyCam.transform.localRotation = Quaternion.LookRotation(myInfo.ballGameObject.transform.position - buggyCam.transform.localPosition);
					(buggyCam.GetComponent("SmoothFollow") as SmoothFollow).enabled = false;
					Orbit bco = buggyCam.AddComponent("Orbit") as Orbit;
					bco.Axis = Vector3.up;
					bco.Point = myInfo.ballGameObject.transform.position;
					bco.Speed = 0.8f;
					
					// if at ball
				} else if (myInfo.currentMode==1) {
					myInfo.currentMode = 0;
					// set them in buggy
					myInfo.characterGameObject.transform.parent = myInfo.cartGameObject.transform;
					myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,0);
					myInfo.characterGameObject.transform.rotation = myInfo.cartGameObject.transform.rotation;
					// unlock golf ball
					myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.None;
					//*/ change animation
					myInfo.characterGameObject.transform.FindChild("lil_patrick").animation.Play("driveIdle",PlayMode.StopAll);
					//*/ move camera - HACKY
					GameObject buggyCam = nvs.myCam.gameObject;
					buggyCam.transform.parent = myInfo.cartGameObject.transform;
					(buggyCam.GetComponent("SmoothFollow") as SmoothFollow).enabled = true;
					Orbit bco = buggyCam.GetComponent("Orbit") as Orbit;
					Component.Destroy(bco);
				}
				networkView.RPC("PlayerSwap", RPCMode.Others, myInfo.currentMode, myInfo.player);
			}
		} else {
			// send packets about keyboard every 0.015s
			timer += Time.deltaTime;
			if (timer > 0.015) {
				// send a no-keys-pressed message
				networkView.RPC("KartMovement", RPCMode.Server, 0f, 0f);
			}
		}
		
		if (!myInfo.playerIsPaused && Input.GetKeyDown (KeyCode.Space)) {
			myInfo.currentMode=0;
		}
		if (!myInfo.playerIsPaused && Input.GetKeyDown(KeyCode.E) && false) {	//false until netSwing is working
			// if in buggy
			if (myInfo.currentMode==0) {
				myInfo.currentMode = 1;
				// set them at golf ball
				//myInfo.ballGameObject.transform.rotation = Quaternion.identity;		// reset rotation to make it nice
				//myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,-2);
				//myInfo.characterGameObject.transform.rotation = Quaternion.identity;
				// lock golf ball
				//myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
				CarUserControl carCtrl = myInfo.cartGameObject.GetComponent(typeof(CarUserControl)) as CarUserControl;
				carCtrl.enabled = false;
				myInfo.ballGameObject.SendMessage ("turnOnScripts");
				myInfo.cartGameObject.SendMessage("turnOffScripts");
				// if at ball
			} else if (myInfo.currentMode==1) {
				myInfo.currentMode = 0;
				// set them in buggy
				//myInfo.characterGameObject.transform.parent = myInfo.cartGameObject.transform;
				//myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,0);
				//myInfo.characterGameObject.transform.rotation = myInfo.cartGameObject.transform.rotation;
				// unlock golf ball
				//myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.None;
				myInfo.ballGameObject.SendMessage("turnOffScripts");
				myInfo.cartGameObject.SendMessage("turnOnScripts");
				CarUserControl carCtrl = myInfo.cartGameObject.GetComponent(typeof(CarUserControl)) as CarUserControl;
				carCtrl.enabled = true;
			}
		}
	}

	// local interpolation - add all other interpolation here aswell
	void FixedUpdate() {
		// if in buggy
		if (myInfo.currentMode==0) {
			// maybe not the best idea to call GetComponent every time - add it to PlayerInfo at some point so it can do a direct reference
			CarController car = myInfo.cartGameObject.transform.GetComponent("CarController") as CarController;
			car.Move(myInfo.h,myInfo.v);
		} else if (myInfo.currentMode==1) {		// if in ball mode

		}
	}
	
	// honks
	[RPC]
	void IHonked(NetworkPlayer player) {
		// find the player
		foreach (PlayerInfo p in nvs.players) {
			if (p.player==player) {
				//TODO: add horn
				//p.cartGameObject.audio.Play();
			}
		}
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

	// blank for server use only
	[RPC]
	void KartMovement(float h, float v) {}
	[RPC]
	void SpawnBall(NetworkViewID viewId) {}
}
