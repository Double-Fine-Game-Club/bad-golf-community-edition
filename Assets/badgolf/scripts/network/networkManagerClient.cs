using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkManagerClient : MonoBehaviour {
	Dictionary<float,string> screenMessages = new Dictionary<float,string>();
	NetworkViewID myViewID;
	GameObject myCart;

	// Use this for initialization
	void Start () {
		// spawn in a cart
		networkView.RPC("GiveMeACart", RPCMode.Server);
	}
	
	// CLIENT SIDE SCRIPTS GO HERE
	void AddScripts() {
		// updates network-sunk fiziks
		controlClient mc = gameObject.AddComponent("controlClient") as controlClient;
		mc.myViewID = myViewID;
	}
	
	void OnGUI() {
		// ping list
		GUILayout.BeginHorizontal();
		GUILayout.Label("Ping: " + Network.GetAveragePing(myViewID.owner) + "ms");
		GUILayout.EndHorizontal();
		
		// show any debug messages
		float keyToRemove = 0;
		foreach (KeyValuePair<float,string> msgs in screenMessages) {
			if (msgs.Key < Time.time) {
				keyToRemove = msgs.Key;	// don't worry about there being more than 1 - it'll update next frame
			} else {
				GUILayout.BeginHorizontal();
				GUILayout.Label(msgs.Value);
				GUILayout.EndHorizontal();
			}
		}
		if (screenMessages.ContainsKey(keyToRemove)) screenMessages.Remove(keyToRemove);
	}




	// things that can be run over the network
	// debug text
	[RPC]
	void PrintText(string text) {
		Debug.Log(text);
		screenMessages.Add(Time.time+5,text);
	}
	
	// spawns a prefab
	[RPC]
	void SpawnPrefab(NetworkViewID viewID, Vector3 spawnLocation, Vector3 velocity, string prefabName) {
		Object prefab = Resources.Load(prefabName);
		// instantiate the prefab
		GameObject clone = Instantiate(prefab, spawnLocation, Quaternion.identity) as GameObject;
		// set viewID
		clone.networkView.viewID = viewID;
		// set velocity if we can
		if (clone.rigidbody) clone.rigidbody.velocity = velocity;
	}

	// tells the player that this viewID is theirs
	[RPC]
	void ThisOnesYours(NetworkViewID viewID) {
		myViewID = viewID;
		myCart = NetworkView.Find(viewID).gameObject;
		AddScripts();
	}
	
	// remove stuff
	[RPC]
	void RemoveViewID(NetworkViewID viewID) {
		// remove the object
		Destroy(NetworkView.Find(viewID).gameObject);
	}


	// blank for server use only
	[RPC]
	void GiveMeACart() {}
}
