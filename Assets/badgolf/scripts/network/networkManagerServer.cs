using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkManagerServer : MonoBehaviour {
	ArrayList playerGameObjects = new ArrayList();
	Dictionary<float,string> screenMessages = new Dictionary<float,string>();
	public GameObject myCart;
	Dictionary<NetworkPlayer,NetworkViewID> playersCartViewID = new Dictionary<NetworkPlayer,NetworkViewID>();
	List<NetworkViewID> randomBalls = new List<NetworkViewID>();

	// Use this for initialization
	void Start () {
		// Use NAT punchthrough if no public IP present
		Network.InitializeServer(32, 11177, !Network.HavePublicAddress());
		MasterServer.RegisterHost("HL4", SystemInfo.deviceName, "Test server");
		
		// create server owners buggy
		myCart = Instantiate(Resources.Load("buggy1"), new Vector3(0,5,0), Quaternion.identity) as GameObject;
		// networkview that shit
		NetworkViewID viewID = Network.AllocateViewID();
		myCart.networkView.viewID = viewID;
		
		// add it to the list
		playerGameObjects.Add(myCart);

		// ANY SERVER SIDE SCRIPTS GO HERE
		//********************************************
		// receives all players inputs and handles fiziks
		controlServer ms = gameObject.AddComponent("controlServer") as controlServer;
		ms.myCart = myCart;
		ms.myViewID = viewID;
		//********************************************
	}

	// fired when a player joins (if you couldn't tell)
	void OnPlayerConnected(NetworkPlayer player) {
		networkView.RPC("PrintText", player, "Welcome to the test server");
		PrintText("Someone joined");
		// send all current players to new guy
		foreach (GameObject playerGameObject in playerGameObjects)
		{
			networkView.RPC("SpawnPrefab", player, playerGameObject.networkView.viewID, playerGameObject.transform.position, new Vector3(0,0,0), "buggy1");
		}
		// send all balls
		foreach (NetworkViewID randomBallViewID in randomBalls)
		{
			GameObject randomBall = NetworkView.Find(randomBallViewID).gameObject;
			networkView.RPC("SpawnPrefab", player, randomBallViewID, randomBall.transform.position, randomBall.rigidbody.velocity, "ball");
		}
	}
	void OnPlayerDisconnected(NetworkPlayer player) {
		// remove all their stuff
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
		// remove their buggy
		playerGameObjects.Remove(NetworkView.Find(playersCartViewID[player]).gameObject);
		Destroy(NetworkView.Find(playersCartViewID[player]).gameObject);
		// tell everyone else to aswell
		networkView.RPC("RemoveViewID", RPCMode.All, playersCartViewID[player]);
	}
	
	void OnGUI() {
		float keyToRemove = 0;
		// show any debug messages
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
	
	// spawns a golf ball
	[RPC]
	void SpawnBall(NetworkViewID playerViewID, NetworkMessageInfo info) {
		// get the players location
		GameObject playerGameObject = NetworkView.Find(playerViewID).gameObject;
		Vector3 position = playerGameObject.transform.position + playerGameObject.transform.rotation * Vector3.forward * 3 + Vector3.up;
		Vector3 velocity = playerGameObject.rigidbody.velocity + playerGameObject.transform.rotation * Vector3.forward * 10;
		// instantiate the prefab
		GameObject clone = Instantiate(Resources.Load("ball"), position, Quaternion.identity) as GameObject;
		// create and set viewID
		NetworkViewID viewID = Network.AllocateViewID();
		clone.networkView.viewID = viewID;
		// give it velocity
		clone.rigidbody.velocity = velocity;
		// tell everyone else about it
		networkView.RPC("SpawnPrefab", RPCMode.Others, viewID, position, velocity, "ball");
		// add it to the list
		randomBalls.Add(viewID);
	}

	[RPC]
	void GiveMeACart(NetworkMessageInfo info) {
		// create new buggy for the new guy - his must be done on the server otherwise collisions wont work!
		Vector3 spawnLocation = new Vector3(0,5,0);
		Vector3 velocity = new Vector3(0,0,0);
		// instantiate the prefab
		GameObject clone = Instantiate(Resources.Load("buggy1"), spawnLocation, Quaternion.identity) as GameObject;
		// create and set viewID
		NetworkViewID viewID = Network.AllocateViewID();
		clone.networkView.viewID = viewID;
		// tell everyone else about it
		networkView.RPC("SpawnPrefab", RPCMode.Others, viewID, spawnLocation, velocity, "buggy1");
		// tell the player it's theirs
		networkView.RPC("ThisOnesYours", info.sender, viewID);
		// add it to the lists
		playersCartViewID.Add(info.sender, viewID);
		playerGameObjects.Add(clone);
	}

	
	// blank for client use only
	[RPC]
	void ThisOnesYours(NetworkViewID viewID) {}

	[RPC]
	void SpawnPrefab(NetworkViewID viewID, Vector3 spawnLocation, Vector3 velocity, string prefabName) {}

	[RPC]
	void RemoveViewID(NetworkViewID viewID) {}
}
