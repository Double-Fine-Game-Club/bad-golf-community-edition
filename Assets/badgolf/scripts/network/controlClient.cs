using UnityEngine;
using System.Collections;

public class controlClient : MonoBehaviour {
	float timer = 0;
	public NetworkViewID myViewID;
	public controlServer ms;
	bool chatVisible = false;
	string chatBuffer = "";
	float forceMultiplyer = 10000;
	GameObject myCart;

	void Start() {
		myCart = NetworkView.Find(myViewID).gameObject;
	}

	void Update () {
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
			//if (Network.isServer) {
			//	ms.KartMovement(myViewID, toSend);
			//} else {
				networkView.RPC("KartMovement", RPCMode.Server, myViewID, toSend);
			//}
		}
		if (Input.GetKeyDown(KeyCode.Q)) {
			networkView.RPC("IHonked", RPCMode.All, myViewID);
		}
		if (Input.GetKeyDown(KeyCode.R)) {
			networkView.RPC("SpawnBall", RPCMode.All, myViewID);
		}
	}

	// local interpolation - add all other intetpolation here aswell
	void FixedUpdate() {
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
	}

	// chat box
	void OnGUI() {
		if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Escape)) {
			chatVisible = false;
			chatBuffer = "";
		}
		if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Return)) {
			chatVisible = false;
			if (chatBuffer!="") {
				networkView.RPC("PrintText", RPCMode.All, myViewID.ToString().Substring(13) + ": " + chatBuffer);
				chatBuffer = "";
			}
		}
		if (chatVisible) {
			GUI.SetNextControlName("ChatBox");
			chatBuffer = GUI.TextField(new Rect(10,Screen.height/2,200,20), chatBuffer, 25);
			GUI.FocusControl("ChatBox");
		}
		if ((Event.current.type == EventType.KeyUp) && (Event.current.keyCode == KeyCode.T)) {
			chatVisible = true;
		}
	}

	
	// honks
	[RPC]
	void IHonked(NetworkViewID viewId) {
		NetworkView.Find(viewId).gameObject.audio.Play();
	}


	// blank for server use only
	[RPC]
	void KartMovement(NetworkViewID viewId, int currentKBStatus) {}

	// blank for server use only
	[RPC]
	void SpawnBall(NetworkViewID viewId) {}
}
