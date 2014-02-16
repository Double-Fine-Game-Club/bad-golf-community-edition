using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkManagerServer : MonoBehaviour {
	Dictionary<float,string> screenMessages = new Dictionary<float,string>();
	networkVariables nvs;
	PlayerInfo myInfo;
	string serverVersion;

	// Use this for initialization
	void Start () {
		// setup reference to networkVariables
		nvs = gameObject.GetComponent("networkVariables") as networkVariables;

		// get server version
		serverVersion = nvs.serverVersion;

		// Use NAT punchthrough if no public IP present
		Network.InitializeServer(32, 11177, !Network.HavePublicAddress());
		MasterServer.RegisterHost(serverVersion, SystemInfo.deviceName, "Test server");
		
		// create server owners buggy
		GameObject myCart = Instantiate(Resources.Load("buggy1"), new Vector3(0,5,0), Quaternion.identity) as GameObject;

		// networkview that shit
		NetworkViewID viewID = Network.AllocateViewID();
		myCart.networkView.viewID = viewID;

		// turn it into a PlayerInfo
		nvs.myInfo.cartGameObject = myCart;
		nvs.myInfo.cartModel = "buggy1";
		nvs.myInfo.cartViewID = viewID;
		nvs.myInfo.ballModel = "ball";
		nvs.myInfo.characterModel = "lil_patrick";

		// get self
		nvs.myInfo.player = Network.player;

		// keep a copy
		myInfo = nvs.myInfo;

		// add myInfo to the player list
		nvs.players = new ArrayList();
		nvs.players.Add(myInfo);

		// ANY SERVER SIDE SCRIPTS GO HERE
		//********************************************
		// receives all players inputs and handles fiziks
		gameObject.AddComponent("controlServer");
		// chat
		gameObject.AddComponent("netChat");
		//********************************************
	}

	// fired when a player joins (if you couldn't tell)
	void OnPlayerConnected(NetworkPlayer player) {
		networkView.RPC("PrintText", player, "Welcome to the test server");
		PrintText("Someone joined");

		// send all current players to new guy
		foreach (PlayerInfo p in nvs.players)
		{
			networkView.RPC("SpawnPrefab", player, p.cartViewID, p.cartGameObject.transform.position, new Vector3(0,0,0), p.cartModel);
			if (p.ballGameObject) {
				networkView.RPC("SpawnPrefab", player, p.ballViewID, p.ballGameObject.transform.position, new Vector3(0,0,0), p.ballModel);
			}
			if (p.characterGameObject) {
				networkView.RPC("SpawnPrefab", player, p.characterViewID, p.characterGameObject.transform.position, new Vector3(0,0,0), p.characterModel);
			}
		}
	}
	void OnPlayerDisconnected(NetworkPlayer player) {
		// tell everyone
		networkView.RPC("PrintText", RPCMode.All, "Someone left");

		// remove all their stuff
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);

		PlayerInfo toDelete = new PlayerInfo();
		foreach (PlayerInfo p in nvs.players)
		{
			if (p.player==player) {
				// remove their buggy
				Destroy(p.cartGameObject);
				// tell everyone else to aswell
				networkView.RPC("RemoveViewID", RPCMode.All, p.cartViewID);
				// remove their ball
				if (p.ballGameObject) {
					Destroy(p.ballGameObject);
					networkView.RPC("RemoveViewID", RPCMode.All, p.ballViewID);
				}
				// remove their character
				if (p.characterGameObject) {
					Destroy(p.characterGameObject);
					networkView.RPC("RemoveViewID", RPCMode.All, p.characterViewID);
				}
				toDelete = p;
			}
		}
		if (nvs.players.Contains(toDelete)) nvs.players.Remove(toDelete);
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
	
	/*/ spawns a golf ball
	[RPC]
	void SpawnBall(NetworkViewID playerViewID, NetworkMessageInfo info) {
        if (randomBalls.ContainsKey(info.sender)) return;
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
		randomBalls.Add(info.sender, viewID);
	}
	//*/

	/*/
    [RPC]
    void SpawnPlayer(NetworkMessageInfo info)
    {
        if (randomBalls.ContainsKey(info.sender) && !spawnedPlayers.ContainsKey(info.sender))
        {
            GameObject playerGameObject = NetworkView.Find(randomBalls[info.sender]).gameObject;
            Vector3 position = playerGameObject.transform.position + new Vector3(2.5f,0);
            
            // instantiate the prefab
            GameObject clone = Instantiate(Resources.Load("lil_patrick"), position, Quaternion.identity) as GameObject;
            NetworkViewID viewID = Network.AllocateViewID();
            clone.networkView.viewID = viewID;
            networkView.RPC("SpawnPrefab", RPCMode.Others, viewID, position, Vector3.zero, "lil_patrick");
            spawnedPlayers.Add(info.sender, viewID);
        }
        else
        {
            if (spawnedPlayers.ContainsKey(info.sender))
            {
                NetworkViewID viewID = spawnedPlayers[info.sender];
                Destroy(NetworkView.Find(viewID).gameObject);
                networkView.RPC("RemoveViewID", RPCMode.Others, viewID);
                spawnedPlayers.Remove(info.sender);
            }
            return;
        }
    }
	//*/

	[RPC]
	void GiveMeACart(string cartModel, string ballModel, string characterModel, NetworkMessageInfo info) {
		// create new buggy for the new guy - his must be done on the server otherwise collisions wont work!
		Vector3 spawnLocation = new Vector3(0,5,0);
		Vector3 velocity = new Vector3(0,0,0);

		// instantiate the prefab
		GameObject clone = Instantiate(Resources.Load(cartModel), spawnLocation, Quaternion.identity) as GameObject;

		// create and set viewID
		NetworkViewID viewID = Network.AllocateViewID();
		clone.networkView.viewID = viewID;

		// tell everyone else about it
		networkView.RPC("SpawnPrefab", RPCMode.Others, viewID, spawnLocation, velocity, cartModel);

		// tell the player it's theirs
		networkView.RPC("ThisOnesYours", info.sender, viewID);

		// create a PlayerInfo for it
		PlayerInfo newGuy = new PlayerInfo();
		newGuy.cartModel = cartModel;
		newGuy.cartGameObject = clone;
		newGuy.cartViewID = viewID;
		newGuy.ballModel = ballModel;
		newGuy.characterModel = characterModel;
		newGuy.currentMode = 0;	// set them in buggy
		newGuy.player = info.sender;

		// add it to the lists
		nvs.players.Add(newGuy);
	}

	
	// blank for client use only
	[RPC]
	void ThisOnesYours(NetworkViewID viewID) {}

	[RPC]
	void SpawnPrefab(NetworkViewID viewID, Vector3 spawnLocation, Vector3 velocity, string prefabName) {}

	[RPC]
	void RemoveViewID(NetworkViewID viewID) {}
}
