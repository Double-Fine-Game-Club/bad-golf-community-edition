﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkManagerServer : MonoBehaviour {
	ArrayList playerGameObjects = new ArrayList();
	Dictionary<float,string> screenMessages = new Dictionary<float,string>();
	GameObject myCart;
	Dictionary<NetworkPlayer,NetworkViewID> playersCartViewID = new Dictionary<NetworkPlayer,NetworkViewID>();
	Dictionary<NetworkPlayer,NetworkViewID> randomBalls = new Dictionary<NetworkPlayer, NetworkViewID>();
    Dictionary<NetworkPlayer, NetworkViewID> spawnedPlayers = new Dictionary<NetworkPlayer, NetworkViewID>();
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

		// add variables to networkVariables
		networkVariables nvs = gameObject.GetComponent("networkVariables") as networkVariables;
		nvs.myCart = myCart;
		nvs.myViewID = viewID;
		nvs.playersCartViewID = playersCartViewID;
		nvs.randomBalls = randomBalls;
		nvs.spawnedPlayers = spawnedPlayers;

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
		foreach (GameObject playerGameObject in playerGameObjects)
		{
			networkView.RPC("SpawnPrefab", player, playerGameObject.networkView.viewID, playerGameObject.transform.position, new Vector3(0,0,0), "buggy1");
		}
		// send all balls
        foreach (KeyValuePair<NetworkPlayer, NetworkViewID> pair in randomBalls)
		{
			GameObject randomBall = NetworkView.Find(pair.Value).gameObject;
			networkView.RPC("SpawnPrefab", player, pair.Value, randomBall.transform.position, randomBall.rigidbody.velocity, "ball");
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
        if (randomBalls.ContainsKey(info.sender))
            return;
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
