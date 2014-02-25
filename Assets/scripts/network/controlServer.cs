using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controlServer : MonoBehaviour {
	float forceMultiplyer = 10000;
	int maxPlayers = 4;
	bool limitReached=false;

	networkVariables nvs;
	PlayerInfo myInfo;
	netPause pause;
	netLobbyManager lobbyManager;

	void Start() {
		// get variables we need
		nvs = GetComponent("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		pause = GetComponent ("netPause") as netPause;
		lobbyManager = GetComponent ("netLobbyManager") as netLobbyManager;
	}

	void Update() {
		if (!limitReached && nvs.players.Count == maxPlayers) {
			lobbyManager.SendMessage("onLobbyFull");
			limitReached=true;
		}


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
				myInfo.characterGameObject.transform.parent = myInfo.cartGameObject.transform.FindChild("buggy");
				myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,0);
				myInfo.characterGameObject.transform.rotation = myInfo.cartGameObject.transform.FindChild("buggy").rotation;
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
				GameObject playerGameObject = p.cartGameObject;
				/*Vector3 forceFromFront = new Vector3();	// force from front tires
				Vector3 forceFromBack = new Vector3();	// force from back tires
				if ((KBState & 8)==8) {
					// make sure it's facing the direction of the vehicle
					forceFromFront += playerGameObject.transform.localRotation * Vector3.forward;
					forceFromBack += playerGameObject.transform.localRotation * Vector3.forward;
				}
				if ((KBState & 4)==4) {
					// make sure it's facing the direction of the vehicle
					forceFromFront += playerGameObject.transform.localRotation * Vector3.back;
					forceFromBack += playerGameObject.transform.localRotation * Vector3.back;
				}
				if ((KBState & 2)==2) {
					// rotate the front forces if they are turning
					forceFromFront = Quaternion.AngleAxis(-60,Vector3.up) * forceFromFront;
				}
				if ((KBState & 1)==1) {
					// rotate the front forces if they are turning
					forceFromFront = Quaternion.AngleAxis(60,Vector3.up) * forceFromFront;
				}
				if (forceFromFront.sqrMagnitude!=0) {
					// one at each tyre
					playerGameObject.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromFront,playerGameObject.transform.position+playerGameObject.transform.localRotation*Vector3.forward);
					playerGameObject.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromFront,playerGameObject.transform.position+playerGameObject.transform.localRotation*Vector3.forward);
					playerGameObject.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromBack,playerGameObject.transform.position+playerGameObject.transform.localRotation*Vector3.back);
					playerGameObject.rigidbody.AddForceAtPosition(forceMultiplyer*forceFromBack,playerGameObject.transform.position+playerGameObject.transform.localRotation*Vector3.back);
				}*/
				CarController car = p.cartGameObject.transform.FindChild("buggy").gameObject.GetComponent("CarController") as CarController;
				car.Move(p.h,p.v);
			}
		}
		
		// if in buggy
		if (!myInfo.playerIsPaused && myInfo.currentMode==0) {
			// add own fiziks
			/*int toSend = 0;
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
			myInfo.KBState = toSend;*/
			myInfo.h = Input.GetAxis("Horizontal");
			myInfo.v = Input.GetAxis("Vertical");
		}
	}

	void OnPlayerConnected(NetworkPlayer player) {
		/*if (!limitReached && nvs.players.Count == maxPlayers) {
			lobbyManager.SendMessage("onLobbyFull");
			limitReached=true;
		}*/
	}

	// update what they are currenly doing - this also adds new players automatically
	[RPC]
	public void KartMovement(float h, float v, NetworkMessageInfo info) {
		foreach (PlayerInfo p in nvs.players) {
			if (p.player==info.sender) {
				//p.KBState = currentKBStatus;
				p.v = v;
				p.h =h;
			}
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
					p.characterGameObject.transform.parent = p.cartGameObject.transform.FindChild("buggy");
					p.characterGameObject.transform.localPosition = new Vector3(0,0,0);
					p.characterGameObject.transform.rotation = p.cartGameObject.transform.FindChild("buggy").rotation;
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
				//p.KBState = 0;
			}
		}
	}


}
