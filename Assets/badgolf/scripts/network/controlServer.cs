using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controlServer : MonoBehaviour {
	float forceMultiplyer = 10000;
	networkVariables nvs;
	PlayerInfo myInfo;

	void Start() {
		// get variables we need
		nvs = GetComponent("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Q)) {
			networkView.RPC("IHonked", RPCMode.All, myInfo.cartViewID);
		}
        if (Input.GetKeyDown(KeyCode.G)) {
			networkView.RPC("PlayerSwap", RPCMode.Server);
		}
	}

	// UPDATE ALL THE FIZIKS!
	void FixedUpdate () {
		foreach (PlayerInfo p in nvs.players) {
			int KBState = p.KBState;
			GameObject playerGameObject = p.cartGameObject;
			Vector3 forceFromFront = new Vector3();	// force from front tires
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
			}
		}

		// add own fiziks
		GameObject myCart = nvs.myInfo.cartGameObject;
		Vector3 myForceFromFront = new Vector3();	// force from front tires
		Vector3 myForceFromBack = new Vector3();	// force from back tires
		if (Input.GetKey(KeyCode.W)) {
			// make sure it's facing the direction of the vehicle
			myForceFromFront += myCart.transform.localRotation * Vector3.forward;
			myForceFromBack += myCart.transform.localRotation * Vector3.forward;
		}
		if (Input.GetKey(KeyCode.S)) {
			// make sure it's facing the direction of the vehicle
			myForceFromFront += myCart.transform.localRotation * Vector3.back;
			myForceFromBack += myCart.transform.localRotation * Vector3.back;
		}
		if (Input.GetKey(KeyCode.A)) {
			// rotate the front forces if they are turning
			myForceFromFront = Quaternion.AngleAxis(-60,Vector3.up) * myForceFromFront;
		}
		if (Input.GetKey(KeyCode.D)) {
			// rotate the front forces if they are turning
			myForceFromFront = Quaternion.AngleAxis(60,Vector3.up) * myForceFromFront;
		}
		if (myForceFromFront.sqrMagnitude!=0) {
			// one at each tyre
			myCart.rigidbody.AddForceAtPosition(forceMultiplyer*myForceFromFront,myCart.transform.position+myCart.transform.localRotation*Vector3.forward);
			myCart.rigidbody.AddForceAtPosition(forceMultiplyer*myForceFromFront,myCart.transform.position+myCart.transform.localRotation*Vector3.forward);
			myCart.rigidbody.AddForceAtPosition(forceMultiplyer*myForceFromBack,myCart.transform.position+myCart.transform.localRotation*Vector3.back);
			myCart.rigidbody.AddForceAtPosition(forceMultiplyer*myForceFromBack,myCart.transform.position+myCart.transform.localRotation*Vector3.back);
		}
	}

	// update what they are currenly doing - this also adds new players automatically
	[RPC]
	public void KartMovement(int currentKBStatus, NetworkMessageInfo info) {
		foreach (PlayerInfo p in nvs.players) {
			if (p.player==info.sender) {
				p.KBState = currentKBStatus;
			}
		}
	}

	// honks
	[RPC]
	void IHonked(NetworkViewID viewId) {
		NetworkView.Find(viewId).gameObject.audio.Play();
	}

	
	// blank for client use only
	[RPC]
	void PlayerSwap() {}
}
