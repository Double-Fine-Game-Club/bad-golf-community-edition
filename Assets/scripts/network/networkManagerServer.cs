﻿using UnityEngine;
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
		GameObject cartContainerObject = (Instantiate(Resources.Load("multi_buggy"), new Vector3(0,0,0), Quaternion.identity) as GameObject);
		GameObject ballGameObject = Instantiate(Resources.Load("golf_ball"), new Vector3(-18,6,110), Quaternion.identity) as GameObject;
		GameObject characterGameObject = Instantiate(Resources.Load("lil_patrick"), new Vector3(0,4,0), Quaternion.identity) as GameObject;
		GameObject cartGameObject = cartContainerObject.transform.FindChild ("buggy").gameObject;
		// set buggy as characters parent
		characterGameObject.transform.parent = cartGameObject.transform;

		// networkview that shit
		NetworkViewID cartViewIDTransform = Network.AllocateViewID();
		NetworkView cgt = cartContainerObject.AddComponent("NetworkView") as NetworkView;
		cgt.observed = cartContainerObject.transform;
		cgt.viewID = cartViewIDTransform;
		cgt.stateSynchronization = NetworkStateSynchronization.Unreliable;
		NetworkViewID cartViewIDRigidbody = Network.AllocateViewID();
		NetworkView cgr = cartGameObject.AddComponent("NetworkView") as NetworkView;
		cgr.observed = cartGameObject.rigidbody;
		cgr.viewID = cartViewIDRigidbody;
		cgr.stateSynchronization = NetworkStateSynchronization.Unreliable;
		NetworkViewID ballViewID = Network.AllocateViewID();
		ballGameObject.networkView.viewID = ballViewID;
		NetworkViewID characterViewID = Network.AllocateViewID();
		characterGameObject.networkView.viewID = characterViewID;

		// turn it into a PlayerInfo
		nvs.myInfo.cartContainerObject = cartContainerObject;
		nvs.myInfo.cartGameObject = cartGameObject;
		nvs.myInfo.cartModel = "multi_buggy";
		nvs.myInfo.cartViewIDTransform = cartViewIDTransform;
		nvs.myInfo.cartViewIDRigidbody = cartViewIDRigidbody;
		nvs.myInfo.ballGameObject = ballGameObject;
		nvs.myInfo.ballModel = "golf_ball";
		nvs.myInfo.ballViewID = ballViewID;
		InControlSwingMode ballSwing = nvs.myInfo.ballGameObject.GetComponent (typeof(InControlSwingMode)) as InControlSwingMode;
		ballSwing.cart = nvs.myInfo.cartGameObject;
		TransferToSwing transfSwing = nvs.myInfo.cartGameObject.GetComponent (typeof(TransferToSwing)) as TransferToSwing;
		transfSwing.ball = nvs.myInfo.ballGameObject;
		nvs.myInfo.characterGameObject = characterGameObject;
		nvs.myInfo.characterModel = "lil_patrick";
		nvs.myInfo.characterViewID = characterViewID;
		nvs.myInfo.currentMode = 0;	// set in buggy

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
		//pause
		gameObject.AddComponent ("netPause");
		//********************************************
	}

	// fired when a player joins (if you couldn't tell)
	void OnPlayerConnected(NetworkPlayer player) {
		networkView.RPC("PrintText", player, "Welcome to the test server");
		PrintText("Someone joined");

		// send all current players to new guy
		foreach (PlayerInfo p in nvs.players)
		{
			networkView.RPC("SpawnPrefab", player, p.cartViewIDTransform, p.cartContainerObject.transform.position, new Vector3(0,0,0), p.cartModel);
			networkView.RPC("SpawnPrefab", player, p.ballViewID, p.ballGameObject.transform.position, new Vector3(0,0,0), p.ballModel);
			networkView.RPC("SpawnPrefab", player, p.characterViewID, p.characterGameObject.transform.position, new Vector3(0,0,0), p.characterModel);
			// tell the player this is a player and not some random objects
			networkView.RPC("SpawnPlayer", player, p.cartViewIDTransform, p.cartViewIDRigidbody, p.ballViewID, p.characterViewID, p.currentMode, p.player);
		}


	}
	void OnPlayerDisconnected(NetworkPlayer player) {
		// tell all players to remove them
		networkView.RPC("RemovePlayer", RPCMode.All, player);

		// remove all their stuff
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);

		PlayerInfo toDelete = new PlayerInfo();
		foreach (PlayerInfo p in nvs.players)
		{
			if (p.player==player) {
				// remove their stuff
				Destroy(p.cartContainerObject);
				Destroy(p.ballGameObject);
				Destroy(p.characterGameObject);
				// tell everyone else to aswell
				networkView.RPC("RemoveViewID", RPCMode.All, p.characterViewID);
				networkView.RPC("RemoveViewID", RPCMode.All, p.cartViewIDTransform);
				networkView.RPC("RemoveViewID", RPCMode.All, p.ballViewID);
				// remove from array
				toDelete = p;
			}
		}
		if (nvs.players.Contains(toDelete)) nvs.players.Remove(toDelete);
	}
	
	void OnGUI() {
		GUILayout.BeginHorizontal();
		GUILayout.Label ("Active Players: ");
		GUILayout.Label (nvs.players.Count.ToString());
		GUILayout.EndHorizontal();

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

	[RPC]
	void GiveMeACart(string cartModel, string ballModel, string characterModel, NetworkMessageInfo info) {
		// create new buggy for the new guy - his must be done on the server otherwise collisions wont work!
		Vector3 spawnLocation = new Vector3(0,0,0);
		Vector3 velocity = new Vector3(0,0,0);

		// instantiate the prefabs
		GameObject cartContainerObject = (Instantiate(Resources.Load(cartModel), spawnLocation, Quaternion.identity) as GameObject);
		GameObject ballGameObject = Instantiate(Resources.Load(ballModel), spawnLocation + new Vector3(3,0,0), Quaternion.identity) as GameObject;
		GameObject characterGameObject = Instantiate(Resources.Load(characterModel), spawnLocation + new Vector3(0,-1,0), Quaternion.identity) as GameObject;
		GameObject cartGameObject = cartContainerObject.transform.FindChild ("buggy").gameObject;
		// set buggy as characters parent
		characterGameObject.transform.parent = cartGameObject.transform;

		// create and set viewIDs
		NetworkViewID cartViewIDTransform = Network.AllocateViewID();
		NetworkView cgt = cartContainerObject.AddComponent("NetworkView") as NetworkView;
		cgt.observed = cartContainerObject.transform;
		cgt.viewID = cartViewIDTransform;
		cgt.stateSynchronization = NetworkStateSynchronization.Unreliable;
		NetworkViewID cartViewIDRigidbody = Network.AllocateViewID();
		NetworkView cgr = cartGameObject.AddComponent("NetworkView") as NetworkView;
		cgr.observed = cartGameObject.rigidbody;
		cgr.viewID = cartViewIDRigidbody;
		cgr.stateSynchronization = NetworkStateSynchronization.Unreliable;
		NetworkViewID ballViewID = Network.AllocateViewID();
		ballGameObject.networkView.viewID = ballViewID;
		NetworkViewID characterViewID = Network.AllocateViewID();
		characterGameObject.networkView.viewID = characterViewID;
		// tell everyone else about it
		networkView.RPC("SpawnPrefab", RPCMode.Others, cartViewIDTransform, spawnLocation, velocity, cartModel);
		networkView.RPC("SpawnPrefab", RPCMode.Others, ballViewID, spawnLocation, velocity, ballModel);
		networkView.RPC("SpawnPrefab", RPCMode.Others, characterViewID, spawnLocation, velocity, characterModel);

		// tell all players this is a player and not some random objects
		networkView.RPC("SpawnPlayer", RPCMode.Others, cartViewIDTransform, cartViewIDRigidbody, ballViewID, characterViewID, 0, info.sender);

		// tell the player it's theirs
		networkView.RPC("ThisOnesYours", info.sender, cartViewIDTransform, ballViewID, characterViewID);

		// create a PlayerInfo for it
		PlayerInfo newGuy = new PlayerInfo();
		newGuy.cartModel = cartModel;
		newGuy.cartContainerObject = cartContainerObject;
		newGuy.cartGameObject = cartGameObject;
		newGuy.cartViewIDTransform = cartViewIDTransform;
		newGuy.cartViewIDRigidbody = cartViewIDRigidbody;
		newGuy.ballModel = ballModel;
		newGuy.ballGameObject = ballGameObject;
		newGuy.ballViewID = ballViewID;
		InControlSwingMode ballSwing = newGuy.ballGameObject.GetComponent (typeof(InControlSwingMode)) as InControlSwingMode;
		ballSwing.cart = newGuy.cartGameObject;
		TransferToSwing transfSwing = newGuy.cartGameObject.GetComponent (typeof(TransferToSwing)) as TransferToSwing;
		transfSwing.ball = newGuy.ballGameObject;
		newGuy.characterModel = characterModel;
		newGuy.characterGameObject = characterGameObject;
		newGuy.characterViewID = characterViewID;
		newGuy.currentMode = 0;	// set them in buggy
		newGuy.player = info.sender;

		// add it to the lists
		nvs.players.Add(newGuy);
	}

	void OnDisconnectedFromServer(NetworkDisconnection info){
		MasterServer.UnregisterHost ();

		//Go back to main menu
		string nameOfLevel = "main";
		Application.LoadLevel( nameOfLevel );
	}
	
	// blank for client use only
	[RPC]
	void ThisOnesYours(NetworkViewID viewID, NetworkViewID b, NetworkViewID c) {}
	[RPC]
	void SpawnPrefab(NetworkViewID viewID, Vector3 spawnLocation, Vector3 velocity, string prefabName) {}
	[RPC]
	void SpawnPlayer(NetworkViewID viewID, NetworkViewID b, NetworkViewID c, NetworkViewID d, int mode, NetworkPlayer p) {}
	[RPC]
	void RemoveViewID(NetworkViewID viewID) {}
	[RPC]
	void RemovePlayer(NetworkPlayer p) {}
}
