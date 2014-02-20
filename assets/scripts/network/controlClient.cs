using UnityEngine;
using System.Collections;

public class controlClient : MonoBehaviour {
	float timer = 0;
	float forceMultiplyer = 10000;
	PlayerInfo myInfo;
	networkVariables nvs;

	void Start() {
		// get variables we need
		nvs = GetComponent("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
	}

	void Update () {
		// if in buggy
		if (myInfo.currentMode==0) {
			// send packets about keyboard every 0.015s
			timer += Time.deltaTime;
			if (timer > 0.015) {
				timer = 0;
				int toSend = 0;
				if (Input.GetKey(KeyCode.W)) {
					toSend += 1;
				}
				toSend = toSend << 1;
				if (Input.GetKey(KeyCode.S)) {
					toSend += 1;
				}
				toSend = toSend << 1;
				if (Input.GetKey(KeyCode.A)) {
					toSend += 1;
				}
				toSend = toSend << 1;
				if (Input.GetKey(KeyCode.D)) {
					toSend += 1;
				}
				networkView.RPC("KartMovement", RPCMode.Server, toSend);
			}
			if (Input.GetKeyDown(KeyCode.Q)) {
				networkView.RPC("IHonked", RPCMode.All, myInfo.player);
			}
		}
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
				
				// if at ball
			} else if (myInfo.currentMode==1) {
				myInfo.currentMode = 0;
				// set them in buggy
				myInfo.characterGameObject.transform.parent = myInfo.cartGameObject.transform;
				myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,0);
				myInfo.characterGameObject.transform.rotation = myInfo.cartGameObject.transform.rotation;
				// unlock golf ball
				myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.None;
			}
			networkView.RPC("PlayerSwap", RPCMode.Others, myInfo.currentMode, myInfo.player);
		}
	}

	// local interpolation - add all other interpolation here aswell
	void FixedUpdate() {
		// if in buggy
		if (myInfo.currentMode==0) {
			GameObject myCart = myInfo.cartGameObject;
			Vector3 forceFromFront = new Vector3();	// force from front tires
			Vector3 forceFromBack = new Vector3();	// force from back tires
			if (Input.GetKey(KeyCode.W)) {
				// make sure it's facing the direction of the vehicle
				forceFromFront += myCart.transform.localRotation * Vector3.forward;
				forceFromBack += myCart.transform.localRotation * Vector3.forward;
			}
			if (Input.GetKey(KeyCode.S)) {
				// make sure it's facing the direction of the vehicle
				forceFromFront += myCart.transform.localRotation * Vector3.back;
				forceFromBack += myCart.transform.localRotation * Vector3.back;
			}
			if (Input.GetKey(KeyCode.A)) {
				// rotate the front forces if they are turning
				forceFromFront = Quaternion.AngleAxis(-60,Vector3.up) * forceFromFront;
			}
			if (Input.GetKey(KeyCode.D)) {
				// rotate the front forces if they are turning
				forceFromFront = Quaternion.AngleAxis(60,Vector3.up) * forceFromFront;
			}
			if (forceFromFront.sqrMagnitude!=0) {
				// one at each tyre
				myCart.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromFront,myCart.transform.position+myCart.transform.localRotation*Vector3.forward);
				myCart.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromFront,myCart.transform.position+myCart.transform.localRotation*Vector3.forward);
				myCart.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromBack,myCart.transform.position+myCart.transform.localRotation*Vector3.back);
				myCart.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromBack,myCart.transform.position+myCart.transform.localRotation*Vector3.back);
			}
		} else if (myInfo.currentMode==1) {		// if in ball mode
			// do ball stuff
		}
	}

	
	// honks
	[RPC]
	void IHonked(NetworkPlayer player) {
		// find the player
		foreach (PlayerInfo p in nvs.players) {
			if (p.player==player) {
				p.cartGameObject.audio.Play();
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
					
				} else if (p.currentMode==1) {	// if they're now at golf ball
					// set them at golf ball
					p.characterGameObject.transform.parent = p.ballGameObject.transform;
					p.ballGameObject.transform.rotation = Quaternion.identity;		// reset rotation to make it nice
					p.characterGameObject.transform.localPosition = new Vector3(0,0,-2);
					p.characterGameObject.transform.rotation = Quaternion.identity;
					// lock golf ball
					p.ballGameObject.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
				}
				
				// reset keyboard buffer
				p.KBState = 0;
			}
		}
	}


	// blank for server use only
	[RPC]
	void KartMovement(int currentKBStatus) {}
	[RPC]
	void SpawnBall(NetworkViewID viewId) {}
}
