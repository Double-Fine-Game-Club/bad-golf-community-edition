using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controlServer : MonoBehaviour {
	//float timer = 0;
	networkVariables nvs;
	PlayerInfo myInfo;
	Transform cameraParentTransform;
	GameObject pin;
	GameObject localBallAnalog;	

	void Start() {
		// get variables we need
		nvs = GetComponent("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;

		localBallAnalog = new GameObject ();
	}

	void Update() {
		// only do key presses if it's not paused
		if (!myInfo.playerIsPaused) {
			// HONK (only if in a buggy)
			if (Input.GetKeyDown(KeyCode.Q) && myInfo.currentMode==0) {
				// need to wait for the audio guys to fix this
				// if you think you've fixed this test in online aswell
				networkView.RPC("IHonked", RPCMode.All, myInfo.player);
			}
				
		}

		// find the pin
		if(pin==null){
			pin = GameObject.Find ("winningPole") as GameObject;
			if (pin!=null) (pin.GetComponent ("netWinCollider") as netWinCollider).initialize (); //setup the pin while we have a reference to it.
		}
	}

	// UPDATE ALL THE FIZIKS!
	void FixedUpdate () {
		foreach (PlayerInfo p in nvs.players) {
			// if in buggy
			if (p.currentMode==0) {
				p.carController.Move(p.h,p.v);

			} else if (p.currentMode==1) {	// if in ball mode
				p.carController.Move(0f,0f);
			}
		}
		// if in buggy then update what they be pressing
		if (!myInfo.playerIsPaused && myInfo.currentMode==0) {
			// add own fiziks
			#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
				myInfo.h = Input.acceleration.x;
				myInfo.v = Input.acceleration.y + .5f;
			#else
				myInfo.h = Input.GetAxis("Horizontal");
				myInfo.v = Input.GetAxis("Vertical");
			#endif
			/*/ send packets about keyboard every 0.015s
			timer += Time.fixedDeltaTime;
			if (timer > 0.015) {
				timer = 0;
				networkView.RPC("KartMovement", RPCMode.All, myInfo.h,myInfo.v, myInfo.player);
			}*/

		} else {
			// paused so don't move
			myInfo.h = 0f;
			myInfo.v = 0f;
			/*/ send packets about keyboard every 0.015s
			timer += Time.fixedDeltaTime;
			if (timer > 0.015) {
				networkView.RPC("KartMovement", RPCMode.All, myInfo.h,myInfo.v, myInfo.player);
			}*/
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
			myInfo.ballGameObject.transform.rotation = Quaternion.identity;

			localBallAnalog.transform.position = myInfo.ballGameObject.transform.position;	//hack_answers
			localBallAnalog.transform.rotation = myInfo.ballGameObject.transform.rotation;	//hack_answers
			localBallAnalog.transform.localScale = myInfo.ballGameObject.transform.localScale;	//hack_answers
			//myInfo.characterGameObject.transform.parent = myInfo.ballGameObject.transform;
			myInfo.characterGameObject.transform.parent = localBallAnalog.transform;

			myInfo.ballGameObject.transform.rotation = Quaternion.LookRotation((pin.transform.position - myInfo.ballGameObject.transform.position) - new Vector3(0, pin.transform.position.y - myInfo.ballGameObject.transform.position.y,0));	
			myInfo.characterGameObject.transform.localPosition = new Vector3(1.7f,-.2f,0);
			myInfo.characterGameObject.transform.localRotation = Quaternion.identity * new Quaternion(0f, -Mathf.PI/2, 0f, 1f);

			localBallAnalog.transform.rotation = myInfo.ballGameObject.transform.rotation;
			// lock golf ball
			myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			//*/ move camera - HACKY
			GameObject buggyCam = nvs.myCam.gameObject;
			(buggyCam.GetComponent("FollowPlayerScript") as FollowPlayerScript).enabled = false;
			cameraParentTransform = buggyCam.transform.parent;	// keep a reference for later
			//buggyCam.transform.parent = myInfo.ballGameObject.transform;
			buggyCam.transform.parent = localBallAnalog.transform;	//hack_answers
			buggyCam.transform.rotation = Quaternion.identity;	// is this line needed?
			buggyCam.transform.localPosition = new Vector3(-6,4,0);
			//buggyCam.transform.rotation = Quaternion.LookRotation(myInfo.ballGameObject.transform.position - buggyCam.transform.position);
			buggyCam.transform.localRotation = Quaternion.identity;

			//change animation
			myInfo.characterGameObject.animation.Play("golfIdle",PlayMode.StopAll);
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
		buggyCam.transform.parent = cameraParentTransform;	// put it back
		
		(buggyCam.GetComponent("FollowPlayerScript") as FollowPlayerScript).enabled = true;

		//change animation
		myInfo.characterGameObject.animation.Play("driveIdle",PlayMode.StopAll);

		(GetComponent ("netTransferToSwing") as netTransferToSwing).enabled = true;
	}


	// update what they are currenly doing - this also adds new players automagically
	[RPC]
	public void KartMovement(float h, float v, NetworkPlayer player) {
		if(nvs){	//currently, a player maybe theoretically could load the scene and move before start is finished
			foreach (PlayerInfo p in nvs.players) {
				if (p.player==player) {
					p.v = v;
					p.h = h;
				}
			}
		}
	}
	
	// honks
	[RPC]
	void IHonked(NetworkPlayer player) {
		// find the player
		foreach (PlayerInfo p in nvs.players) {
			if (p.player==player) {
				SoundManager.Get().playSfx3d(p.cartGameObject, "horn1", 5, 500, 1);
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
					//change animation
					p.characterGameObject.animation.Play("driveIdle",PlayMode.StopAll);
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
					//change animation
					p.characterGameObject.animation.Play("golfIdle",PlayMode.StopAll);
				}

				// reset keyboard buffer
				p.h = 0f;
				p.v = 0f;
			}
		}
	}



}
