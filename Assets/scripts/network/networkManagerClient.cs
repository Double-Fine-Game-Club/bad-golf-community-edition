using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkManagerClient : MonoBehaviour {
	Dictionary<float,string> screenMessages = new Dictionary<float,string>();
	PlayerInfo myInfo;
	bool connected = false;
	
	/****************************************************
	 * 
	 * DONT EDIT THIS SCRIPT UNLESS ITS TO ADD ANYTHING
	 * IN THE "ANY CLIENT SIDE SCRIPTS GO HERE" SECTION
	 * 
	 ****************************************************/

	// Use this for initialization
	void Start () {
		// spawn in a cart
		networkView.RPC("GiveMeACart", RPCMode.Server, "buggy_m", "golf_ball", "lil_patrick");
	}
	
	// CLIENT SIDE SCRIPTS GO HERE
	// all scripts are called after we have a reference to the buggy and a NetworkViewID
	void AddScripts() {
		// updates network-sunk fiziks
		gameObject.AddComponent("controlClient");

		// chat
		gameObject.AddComponent("netChat");

		//pause
		gameObject.AddComponent ("netPause");

		// Load Player's Cart and Golf Vars into PlayerLoad Script
		networkVariables nvs = GetComponent("networkVariables") as networkVariables;
		networkPlayerLoad netPlayer = gameObject.AddComponent ("networkPlayerLoad") as networkPlayerLoad;
		netPlayer.nvs = nvs;

		// set the camera in the audio script on the buggy - PUT THIS IN A SCRIPT SOMEONE
		//CarAudio mca = myInfo.cartGameObject.GetComponent("CarAudio") as CarAudio;
		//mca.followCamera = nvs.myCam;	// replace tmpCam with our one - this messes up sound atm
		//(nvs.myCam.gameObject.AddComponent("FollowPlayerScript") as FollowPlayerScript).target = myInfo.cartGameObject.transform;	// add smooth follow script
		
		// add the swing script
		gameObject.AddComponent("netSwing");

		// get self
		myInfo.player = Network.player;
		// get server
		myInfo.server = myInfo.cartViewIDTransform.owner;
		// show that we connected
		connected = true;
	}

	// debug shit
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

	void OnDisconnectedFromServer(NetworkDisconnection info){
		//Disconnected from the server for some reason
		if (info == NetworkDisconnection.LostConnection) {
			Debug.Log ("Lost connection to the server");
		} else {
			Debug.Log ("successfully disconnected from the server");
		}

		//Go back to main menu
		string nextLevel = "main";
		Application.LoadLevel( nextLevel );
	}


	// things that can be run over the network
	// debug text
	[RPC]
	void PrintText(string text) {
		Debug.Log(text);
		screenMessages.Add(Time.time+5,"[DEBUG] "+text);
	}
	
	// spawns a prefab
	[RPC]
	void SpawnPrefab(NetworkViewID viewID, Vector3 spawnLocation, Vector3 velocity, string prefabName) {
		Object prefab = Resources.Load(prefabName);
		// instantiate the prefab
		GameObject clone = Instantiate(prefab, spawnLocation, Quaternion.identity) as GameObject;
		// set viewID
		clone.networkView.viewID = viewID;

		// set velocity if we can - why was this commented?
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
		NetworkView cgr = newGuy.cartGameObject.AddComponent("NetworkView") as NetworkView;		// add rigidbody tracking
		cgr.observed = newGuy.cartGameObject.rigidbody;
		cgr.viewID = cartViewIDRigidbody;
		cgr.stateSynchronization = NetworkStateSynchronization.Unreliable;
		newGuy.characterViewID = characterViewID;
		newGuy.characterGameObject = NetworkView.Find(characterViewID).gameObject;
		newGuy.ballViewID = ballViewID;
		newGuy.ballGameObject = NetworkView.Find(ballViewID).gameObject;
		newGuy.currentMode = mode;
		newGuy.player = p;


		// ADD MORE STUFF HERE
		if (newGuy.currentMode==0){
			// set them inside the buggy
			newGuy.characterGameObject.transform.parent = newGuy.cartGameObject.transform;
			newGuy.characterGameObject.transform.localPosition = new Vector3(0,0,0);
			newGuy.characterGameObject.transform.localRotation = Quaternion.identity;
			myInfo.cartGameObject.rigidbody.velocity = Vector3.zero;
			myInfo.cartGameObject.rigidbody.velocity = Vector3.zero;
		} else if (newGuy.currentMode==1) {
			// set them outside the buggy
			newGuy.characterGameObject.transform.parent = newGuy.ballGameObject.transform;
			newGuy.characterGameObject.transform.localPosition = new Vector3(0,0,-2);
			newGuy.characterGameObject.transform.localRotation = Quaternion.identity;
			myInfo.cartGameObject.rigidbody.velocity = Vector3.zero;
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
