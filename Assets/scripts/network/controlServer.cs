using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controlServer : MonoBehaviour {
	int maxPlayers = 4;
	bool limitReached=false;

	networkVariables nvs;
	PlayerInfo myInfo;
	netPause pause;
	void Start() {
		// get variables we need
		nvs = GetComponent("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		pause = GetComponent ("netPause") as netPause;
		GameObject.Find ("lobby_view").transform.FindChild ("camera").gameObject.SetActive (false);
		myInfo.cartContainerObject.transform.FindChild ("multi_buggy_cam").gameObject.SetActive (true);
	}

	void Update() {
		if (!myInfo.playerIsPaused && Input.GetKeyDown(KeyCode.Q)) {
			networkView.RPC("IHonked", RPCMode.All, myInfo.player);
		}
		if (!myInfo.playerIsPaused && Input.GetKeyDown(KeyCode.G)) {
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
			networkView.RPC("PlayerSwap", RPCMode.Server, myInfo.currentMode, myInfo.player);
		}
		if(!myInfo.playerIsBusy && !myInfo.playerIsPaused && Input.GetKey(KeyCode.Escape)){
			pause.SendMessage("onPause");
		}else if(myInfo.playerIsPaused && Input.GetKey(KeyCode.Return)){
			pause.SendMessage("onResume");
		}
	}

	// UPDATE ALL THE FIZIKS!
	void FixedUpdate () {
		foreach (PlayerInfo p in nvs.players) {
			// if in buggy
			if (p.currentMode==0) {
				CarController car = p.cartGameObject.transform.GetComponent("CarController") as CarController;
				car.Move(p.h,p.v);
			}
		}
		// if in buggy
		if (!myInfo.playerIsPaused && myInfo.currentMode==0) {
			// add own fiziks
			myInfo.h = Input.GetAxis("Horizontal");
			myInfo.v = Input.GetAxis("Vertical");
		}
	}

	// update what they are currenly doing - this also adds new players automatically
	[RPC]
	public void KartMovement(float h, float v, NetworkMessageInfo info) {
		foreach (PlayerInfo p in nvs.players) {
			if (p.player==info.sender) {
				p.v = v;
				p.h = h;
			}
		}
	}
	
	// honks
	[RPC]
	void IHonked(NetworkPlayer player) {
		// find the player
		foreach (PlayerInfo p in nvs.players) {
			if (p.player==player) {
				//TODO: add horn to buggy
				//p.cartGameObject.audio.Play();
			}
		}
	}

	// change player mode
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
				p.h = 0f;
				p.v = 0f;
			}
		}
	}


}
