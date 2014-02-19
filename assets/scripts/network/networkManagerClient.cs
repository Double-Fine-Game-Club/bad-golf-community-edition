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
		// get server
		myInfo.server = myInfo.cartViewIDTransform.owner;
		// show that we connected
		connected = true;
	}
	
	void OnGUI() {
		if (!connected) return;
		// ping list
		GUILayout.BeginHorizontal();
		GUILayout.Label("Ping: " + Network.GetAveragePing(myInfo.server) + "ms");
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
	void ThisOnesYours(NetworkViewID cartViewID, NetworkViewID ballViewID, NetworkViewID characterViewID) {
		networkVariables nvs = GetComponent("networkVariables") as networkVariables;
		foreach(PlayerInfo p in nvs.players) {
			if (p.cartViewIDTransform==cartViewID) {
				p.name = nvs.myInfo.name;
				nvs.myInfo = p;
				myInfo = nvs.myInfo;
			}
		}
		// call the functions that need them
		AddScripts();
	}
	
	// tells the player that this set of viewIDs are a player
	[RPC]
	void SpawnPlayer(NetworkViewID cartViewIDTransform, NetworkViewID cartViewIDRigidbody, NetworkViewID ballViewID, NetworkViewID characterViewID, int mode, NetworkPlayer p) {
		PlayerInfo newGuy = new PlayerInfo();

		newGuy.cartViewIDTransform = cartViewIDTransform;
		newGuy.cartGameObject = NetworkView.Find(cartViewIDTransform).gameObject;
		newGuy.cartViewIDRigidbody = cartViewIDRigidbody;
		// add another NetworkView for the rigidbody
		NetworkView cgr = newGuy.cartGameObject.AddComponent("NetworkView") as NetworkView;
		cgr.observed = newGuy.cartGameObject.rigidbody;
		cgr.viewID = cartViewIDRigidbody;
		cgr.stateSynchronization = NetworkStateSynchronization.Unreliable;
		newGuy.characterViewID = characterViewID;
		newGuy.characterGameObject = NetworkView.Find(characterViewID).gameObject;
		newGuy.ballViewID = ballViewID;
		newGuy.ballGameObject = NetworkView.Find(ballViewID).gameObject;
		newGuy.currentMode = mode;
		newGuy.player = p;

		// ADD MORE SHIT HERE
		if (newGuy.currentMode==0){
			// set them inside the buggy
			newGuy.characterGameObject.transform.parent = newGuy.cartGameObject.transform;
			newGuy.characterGameObject.transform.localRotation = Quaternion.identity;
		} else if (newGuy.currentMode==1) {
			// set them inside the buggy
			newGuy.characterGameObject.transform.parent = newGuy.ballGameObject.transform;
			newGuy.characterGameObject.transform.localRotation = Quaternion.identity;
		}
		
		// and let us have them aswell
		networkVariables nvs = GetComponent("networkVariables") as networkVariables;
		nvs.players.Add(newGuy);
	}

	// tells the player that someone left
	[RPC]
	void RemovePlayer(NetworkPlayer player) {
		PrintText("Someone left");

		// remove from array
		networkVariables nvs = GetComponent("networkVariables") as networkVariables;
		PlayerInfo toDelete = new PlayerInfo();
		foreach (PlayerInfo p in nvs.players)
		{
			if (p.player==player) {
				// remove from array
				toDelete = p;
			}
		}
		if (nvs.players.Contains(toDelete)) nvs.players.Remove(toDelete);
	}
	
	// remove stuff
	[RPC]
	void RemoveViewID(NetworkViewID viewID) {
		// remove the object
		if (NetworkView.Find(viewID)) Destroy(NetworkView.Find(viewID).gameObject);
	}


	// blank for server use only
	[RPC]
	void GiveMeACart(string a, string b, string c) {}
}
