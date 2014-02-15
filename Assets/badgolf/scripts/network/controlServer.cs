using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controlServer : MonoBehaviour {
	float forceMultiplyer = 10000;
	public NetworkViewID myViewID;
	Dictionary<NetworkViewID,int> KBinfos = new Dictionary<NetworkViewID,int>();
	public GameObject myCart;
	bool chatVisible = false;
	string chatBuffer = "";

	void Update() {
		if (Input.GetKeyDown(KeyCode.Q)) {
			networkView.RPC("IHonked", RPCMode.All, myViewID);
		}
		if (Input.GetKeyDown(KeyCode.R)) {
			networkView.RPC("SpawnBall", RPCMode.All, myViewID);
		}
	}

	// UPDATE ALL THE FIZIKS!
	void FixedUpdate () {
		NetworkViewID keyToRemove = NetworkViewID.unassigned;
		foreach(KeyValuePair<NetworkViewID,int> entry in KBinfos)
		{
			// probably not best to call Find every fiz update - will optimize later
			NetworkView playerNetworkView = NetworkView.Find(entry.Key);
			if (!playerNetworkView) {
				keyToRemove = entry.Key;	// don't worry about there being more than 1 - it'll update next fiz-frame
			} else {
				GameObject playerGameObject = playerNetworkView.gameObject;
				Vector3 forceFromFront = new Vector3();	// force from front tires
				Vector3 forceFromBack = new Vector3();	// force from back tires
				if ((entry.Value & 8)==8) {
					// make sure it's facing the direction of the vehicle
					forceFromFront += playerGameObject.transform.localRotation * Vector3.forward;
					forceFromBack += playerGameObject.transform.localRotation * Vector3.forward;
				}
				if ((entry.Value & 4)==4) {
					// make sure it's facing the direction of the vehicle
					forceFromFront += playerGameObject.transform.localRotation * Vector3.back;
					forceFromBack += playerGameObject.transform.localRotation * Vector3.back;
				}
				if ((entry.Value & 2)==2) {
					// rotate the front forces if they are turning
					forceFromFront = Quaternion.AngleAxis(-60,Vector3.up) * forceFromFront;
				}
				if ((entry.Value & 1)==1) {
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
		}
		// remove any if needed
		if (KBinfos.ContainsKey(keyToRemove)) KBinfos.Remove(keyToRemove);

		// add own fiziks
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
	
	// remove them from the list
	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.LogError("Ignore this next error, it's actually fine");
	}



	// update what they are currenly doing - this also adds new updates
	[RPC]
	public void KartMovement(NetworkViewID viewId, int currentKBStatus) {
		KBinfos[viewId] = currentKBStatus;
	}

	// honks
	[RPC]
	void IHonked(NetworkViewID viewId) {
		NetworkView.Find(viewId).gameObject.audio.Play();
	}
}
