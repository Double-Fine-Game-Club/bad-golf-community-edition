using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controlServer : MonoBehaviour {
	networkVariables nvs;
	PlayerInfo myInfo;
	netPause pause;
	Transform cameraParentTransform;
	GameObject pin;

	void Start() {
		// get variables we need
		nvs = GetComponent("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		pause = GetComponent ("netPause") as netPause;
		pin = GameObject.Find ("Pin") as GameObject;
		// change camera
		//GameObject.Find ("lobby_view").transform.FindChild ("camera").gameObject.SetActive (false);
		//myInfo.cartContainerObject.transform.FindChild ("multi_buggy_cam").gameObject.SetActive (true);
	}

	void Update() {
		// only do key presses if it's not paused
		if (!myInfo.playerIsPaused) {
			// HONK (only if in a buggy)
			if (Input.GetKeyDown(KeyCode.Q) && myInfo.currentMode==0) {
				// need to wait for the audio guys to fix this
				// if you think you've fixed this test in online aswell
				//networkView.RPC("IHonked", RPCMode.All, myInfo.player);
			}
			// (G)et out of buggy (or get in)
			//if (Input.GetKeyDown(KeyCode.G)) {	//handled by netTransferToSwing
				
		}

		// pause menu toggler
		if(Input.GetKeyDown(KeyCode.Escape)) {
			if(myInfo.playerIsPaused){				// if paused resume
				pause.SendMessage("onResume");
			}else if(!myInfo.playerIsBusy){			// if not busy then pause
				pause.SendMessage("onPause");
			}
		}
	}

	// UPDATE ALL THE FIZIKS!
	void FixedUpdate () {
		foreach (PlayerInfo p in nvs.players) {
			// if in buggy
			if (p.currentMode==0) {
				// maybe not the best idea to call GetComponent every time - add it to PlayerInfo at some point so it can do a direct reference
				CarController car = p.cartGameObject.transform.GetComponent("CarController") as CarController;
				car.Move(p.h,p.v);

			} else if (p.currentMode==1) {	// if in ball mode
				// maybe not the best idea to call GetComponent every time - add it to PlayerInfo at some point so it can do a direct reference
				CarController car = p.cartGameObject.transform.GetComponent("CarController") as CarController;
				car.Move(0f,0f);
			}
		}
		// if in buggy then update what they be pressing
		if (!myInfo.playerIsPaused && myInfo.currentMode==0) {
			// add own fiziks
			myInfo.h = Input.GetAxis("Horizontal");
			myInfo.v = Input.GetAxis("Vertical");

		} else {
			// paused so don't move
			myInfo.h = 0f;
			myInfo.v = 0f;
		}
	}

	
	void switchToBall(){
		// if in buggy
		if (myInfo.currentMode==0) {
			networkView.RPC ("PlayerSwap", RPCMode.Others, 1, myInfo.player);	//to ball
			myInfo.currentMode = 1;
			//stop cart
			myInfo.cartGameObject.rigidbody.velocity = Vector3.zero;
			myInfo.cartGameObject.rigidbody.angularVelocity = Vector3.zero;
			// set them at golf ball
			myInfo.characterGameObject.transform.parent = myInfo.ballGameObject.transform;
			myInfo.ballGameObject.transform.rotation = Quaternion.LookRotation((pin.transform.position - myInfo.ballGameObject.transform.position) - new Vector3(0, pin.transform.position.y - myInfo.ballGameObject.transform.position.y,0));	
			myInfo.characterGameObject.transform.localPosition = new Vector3(1.7f,-.2f,0);
			myInfo.characterGameObject.transform.localRotation = Quaternion.identity * new Quaternion(0f, -Mathf.PI/2, 0f, 1f);
			// lock golf ball
			myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			//*/ move camera - HACKY
			GameObject buggyCam = nvs.myCam.gameObject;
			(buggyCam.GetComponent("FollowPlayerScript") as FollowPlayerScript).enabled = false;
			buggyCam.transform.parent = myInfo.ballGameObject.transform;
			buggyCam.transform.rotation = Quaternion.identity;	// is this line needed?
			buggyCam.transform.localPosition = new Vector3(-6,4,0);
			//buggyCam.transform.rotation = Quaternion.LookRotation(myInfo.ballGameObject.transform.position - buggyCam.transform.position);
			buggyCam.transform.localRotation = Quaternion.identity;

			
			//*/// change animation - try and keep the prefabs similar so this doesn't become a massive else if list
			if (myInfo.characterModel=="lil_patrick") {
				myInfo.characterGameObject.transform.FindChild(myInfo.characterModel).animation.Play("golfIdle",PlayMode.StopAll);
			} else {
				myInfo.characterGameObject.animation.Play("golfIdle",PlayMode.StopAll);
			}
			
		}
	}

	void switchToCart(){
		myInfo.currentMode = 0;
		networkView.RPC ("PlayerSwap", RPCMode.Others, 0, myInfo.player);	//to cart
		// set them in buggy
		myInfo.characterGameObject.transform.parent = myInfo.cartGameObject.transform;
		myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,0);
		myInfo.characterGameObject.transform.rotation = myInfo.cartGameObject.transform.rotation;
		// unlock golf ball
		myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.None;
		//*/ move camera - HACKY
		GameObject buggyCam = nvs.myCam.gameObject;
		buggyCam.transform.parent = myInfo.cartGameObject.transform;
		
		(buggyCam.GetComponent("FollowPlayerScript") as FollowPlayerScript).enabled = true;
		//*/// change animation - try and keep the prefabs similar so this doesn't become a massive else if list
		if (myInfo.characterModel=="lil_patrick") {
			myInfo.characterGameObject.transform.FindChild(myInfo.characterModel).animation.Play("driveIdle",PlayMode.StopAll);
		} else {
			myInfo.characterGameObject.animation.Play("driveIdle",PlayMode.StopAll);
		}
		(GetComponent ("netTransferToSwing") as netTransferToSwing).enabled = true;
	}


	// update what they are currenly doing - this also adds new players automagically
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
					p.characterGameObject.transform.parent = p.cartGameObject.transform;
					p.characterGameObject.transform.localPosition = new Vector3(0,0,0);
					p.characterGameObject.transform.rotation = p.cartGameObject.transform.rotation;
					// unlock golf ball
					p.ballGameObject.rigidbody.constraints = RigidbodyConstraints.None;
					// change animation - try and keep the prefabs similar so this doesn't become a massive else if list
					if (p.characterModel=="lil_patrick") {
						p.characterGameObject.transform.FindChild(p.characterModel).animation.Play("driveIdle",PlayMode.StopAll);
					} else {
						p.characterGameObject.animation.Play("driveIdle",PlayMode.StopAll);
					}
					
				} else if (p.currentMode==1) {	// if they're now at golf ball
					//stop cart
					p.cartGameObject.rigidbody.velocity = Vector3.zero;
					p.cartGameObject.rigidbody.angularVelocity = Vector3.zero;
					// set them at golf ball
					p.characterGameObject.transform.parent = p.ballGameObject.transform;
					p.ballGameObject.transform.rotation = Quaternion.LookRotation((pin.transform.position - p.ballGameObject.transform.position) - new Vector3(0, pin.transform.position.y - p.ballGameObject.transform.position.y,0));	
					p.characterGameObject.transform.localPosition = new Vector3(1.7f,-.2f,0);
					p.characterGameObject.transform.localRotation = Quaternion.identity * new Quaternion(0f, -Mathf.PI/2, 0f, 1f);
					// lock golf ball
					p.ballGameObject.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
					// change animation - try and keep the prefabs similar so this doesn't become a massive else if list
					if (p.characterModel=="lil_patrick") {
						p.characterGameObject.transform.FindChild(p.characterModel).animation.Play("golfIdle",PlayMode.StopAll);
					} else {
						p.characterGameObject.animation.Play("golfIdle",PlayMode.StopAll);
					}
				}

				// reset keyboard buffer
				p.h = 0f;
				p.v = 0f;
			}
		}
	}



}
