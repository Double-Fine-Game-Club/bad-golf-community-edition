using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkManagerClient : MonoBehaviour {
	Dictionary<float,string> screenMessages = new Dictionary<float,string>();
	PlayerInfo myInfo;
	bool connected = false;

	// Use this for initialization
	void Start () {
		// spawn in a cart
		networkView.RPC("GiveMeACart", RPCMode.Server, "buggy1", "ball", "lil_patrick");
	}
	
	// CLIENT SIDE SCRIPTS GO HERE
	// all scripts are called afterwe have a reference to the buggy and a NetworkViewID
	void AddScripts() {
		// updates network-sunk fiziks
		gameObject.AddComponent("controlClient");
		// chat
		gameObject.AddComponent("netChat");
		// get self
		myInfo.player = Network.player;
		// show that we connected
		connected = true;
	}
	
	void OnGUI() {
		if (!connected) return;
		// ping list
		GUILayout.BeginHorizontal();
		GUILayout.Label("Ping: " + Network.GetAveragePing(myInfo.player) + "ms");
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
		// add the references to networkVariables
		networkVariables nvs = GetComponent("networkVariables") as networkVariables;
		nvs.myInfo.cartViewID = viewID;
		nvs.myInfo.cartGameObject = NetworkView.Find(viewID).gameObject;
		// and let us have them aswell
		myInfo = nvs.myInfo;
		// call the functions that need them
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
	void GiveMeACart(string a, string b, string c) {}
}
