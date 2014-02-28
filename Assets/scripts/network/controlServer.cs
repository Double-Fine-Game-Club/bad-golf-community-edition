﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controlServer : MonoBehaviour {
	networkVariables nvs;
	PlayerInfo myInfo;
	netPause pause;

	void Start() {
		// get variables we need
		nvs = GetComponent("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		pause = GetComponent ("netPause") as netPause;
		
		// change camera
		//GameObject.Find ("lobby_view").transform.FindChild ("camera").gameObject.SetActive (false);
		//myInfo.cartContainerObject.transform.FindChild ("multi_buggy_cam").gameObject.SetActive (true);
	}

	void Update() {
		// only do key presses if it's not paused
		if (!myInfo.playerIsPaused) {
			// HONK (only if in a buggy)
			if (Input.GetKeyDown(KeyCode.Q) && myInfo.currentMode==0) {
				networkView.RPC("IHonked", RPCMode.All, myInfo.player);
			}
		if (Input.GetKeyDown (KeyCode.Space)) {
			myInfo.currentMode=0;
		}
			// (G)et out of buggy (or get in)
			if (Input.GetKeyDown(KeyCode.E) && false) {	// IGNORE
				// if in buggy
				if (myInfo.currentMode==0) {
					myInfo.currentMode = 1;
					/* set them at golf ball
					myInfo.characterGameObject.transform.parent = myInfo.ballGameObject.transform;
					myInfo.ballGameObject.transform.rotation = Quaternion.identity;		// reset rotation to make it nice
					myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,-2);
					myInfo.characterGameObject.transform.rotation = Quaternion.identity;
					// lock golf ball
					myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
					*/
					
					myInfo.cartGameObject.rigidbody.velocity = Vector3.zero;
					myInfo.ballGameObject.SendMessage ("turnOnScripts");
					//myInfo.cartGameObject.SendMessage("turnOffScripts");
					CarUserControl carCtrl = myInfo.cartGameObject.GetComponent(typeof(CarUserControl)) as CarUserControl;
					carCtrl.enabled = false;
				
					/* move camera - HACKY
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
					*/
					
					// change animation
					myInfo.characterGameObject.transform.FindChild("lil_patrick").animation.Play("golfIdle",PlayMode.StopAll);

					// if at ball
				} else if (myInfo.currentMode==1) {
					myInfo.currentMode = 0;
					/* set them in buggy
					myInfo.characterGameObject.transform.parent = myInfo.cartGameObject.transform;
					myInfo.characterGameObject.transform.localPosition = new Vector3(0,0,0);
					myInfo.characterGameObject.transform.rotation = myInfo.cartGameObject.transform.rotation;
					// unlock golf ball
					myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.None;
					*/
					
					myInfo.ballGameObject.SendMessage("turnOffScripts");
					myInfo.cartGameObject.SendMessage("turnOnScripts");
					CarUserControl carCtrl = myInfo.cartGameObject.GetComponent(typeof(CarUserControl)) as CarUserControl;
					carCtrl.enabled = true;
					
					/* move camera - HACKY
					GameObject buggyCam = nvs.myCam.gameObject;
					buggyCam.transform.parent = myInfo.cartGameObject.transform;
					(buggyCam.GetComponent("SmoothFollow") as SmoothFollow).enabled = true;
					Orbit bco = buggyCam.GetComponent("Orbit") as Orbit;
					Component.Destroy(bco);
					*/
					
					// change animation
					myInfo.characterGameObject.transform.FindChild("lil_patrick").animation.Play("driveIdle",PlayMode.StopAll);
				}
				// tell server which mode we swapped to
				networkView.RPC("PlayerSwap", RPCMode.Server, myInfo.currentMode, myInfo.player);
			}
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
				//TODO: add horn to buggy - who's Todo?
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
