using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkManagerServer : MonoBehaviour {
	Dictionary<float,string> screenMessages = new Dictionary<float,string>();
	networkVariables nvs;
	PlayerInfo myInfo;
	string serverVersion;
	bool gameHasBegun;
	ServerComment serverComment;
	
	// Use this for initialization
	void Start () {
		// setup reference to networkVariables
		nvs = gameObject.GetComponent("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		
		// add us to the player list
		nvs.players.Add(myInfo);
		
		// get server version
		serverVersion = nvs.serverVersion;	// maybe set the server version to the map name?
		// get server name
		string serverName = nvs.serverName + ": Lobby";
		// serverComment also holds extra info about the server
		serverComment = new ServerComment();
		serverComment.NATmode = nvs.NATmode;
		serverComment.comment = "This is a comment about the server";
		serverComment.level = "level_full";	// only use this one since it's the only one set up atm
		
		// Use NAT punchthrough if NATmode says to
		Network.InitializeServer(31, 11177, nvs.NATmode!=0);
		Debug.Log(serverComment.toString());
		//MasterServer.updateRate = 5; - not needed anymore since Unity doesn't understand networking
		MasterServer.RegisterHost(serverVersion, serverName, serverComment.toString());

		// get game state
		gameHasBegun = nvs.gameHasBegun;
		
		// go into the lobby
		gameObject.AddComponent("netLobby");
	}
	
	// ANY SERVER SIDE SCRIPTS GO HERE
	void AddScripts() {
		// Anything you want to have running in the lobby should go in the netLobby script.
		// This function gets called when a game starts.
		// All scripts are called after we have a reference to the buggy and a NetworkViewID.

		// receives all players inputs and handles fiziks
		gameObject.AddComponent("controlServer");

		//ball range finder
		gameObject.AddComponent ("netTransferToSwing");

		//hitball
		gameObject.AddComponent ("netSwing");
		
		//cart reset
		gameObject.AddComponent ("netPlayerRespawn");

		//show names over player's cart
		gameObject.AddComponent ("PlayerNames");

        //show chat bubble over players when they chat
		gameObject.AddComponent("ChatBubble");
		
		//ball marker
		BallMarker bms = gameObject.AddComponent ("BallMarker") as BallMarker;
		bms.m_nvs = nvs;
		bms.m_myCamera = nvs.myCam;	// can be set in the script instead
		
		// set the camera in the audio script on the buggy - PUT THIS IN A SCRIPT SOMEONE
		CarAudio mca = myInfo.cartGameObject.GetComponent("CarAudio") as CarAudio;
		mca.followCamera = nvs.myCam;	// replace tmpCam with our one - this messes up sound atm
		(nvs.myCam.gameObject.AddComponent("FollowPlayerScript") as FollowPlayerScript).target = myInfo.cartGameObject.transform;	// add player follow script

		// finally disable the preview scene
		(GameObject.Find("main").GetComponent(typeof(GameControl)) as GameControl).ed_levelPreviewScreen.SetActive(false);
	}

	// carts for all!
	void BeginGame() {
		Vector3 velocity = new Vector3(0,0,0);
		//float i = 0;
		//float spacer = 360 / nvs.players.Count;
		foreach (PlayerInfo newGuy in nvs.players) {
			// create new buggy for the new guy - his must be done on the server otherwise collisions wont work!
			//Vector3 spawnLocation = transform.position + Quaternion.AngleAxis(spacer * i++, Vector3.up) * new Vector3(10,2,0);
			Vector3 spawnLocation = new Vector3(0,-200,0);	//start under the map to trigger a reset
			
			// instantiate the prefabs
			GameObject cartGameObject = Instantiate(Resources.Load(newGuy.cartModel), spawnLocation, Quaternion.identity) as GameObject;
			GameObject ballGameObject = Instantiate(Resources.Load(newGuy.ballModel), spawnLocation + new Vector3(3,0,0), Quaternion.identity) as GameObject;
			GameObject characterGameObject = Instantiate(Resources.Load(newGuy.characterModel), spawnLocation, Quaternion.identity) as GameObject;
			// set buggy as characters parent
			characterGameObject.transform.parent = cartGameObject.transform;
			
			// create and set viewIDs
			NetworkViewID cartViewIDTransform = Network.AllocateViewID();					// track the transform of the cart
			NetworkView cgt = cartGameObject.GetComponent("NetworkView") as NetworkView;
			cgt.observed = cartGameObject.transform;
			cgt.viewID = cartViewIDTransform;
			cgt.stateSynchronization = NetworkStateSynchronization.Unreliable;
			NetworkViewID cartViewIDRigidbody = Network.AllocateViewID();					// track the rigidbody of the cart
			NetworkView cgr = cartGameObject.AddComponent("NetworkView") as NetworkView;
			cgr.observed = cartGameObject.rigidbody;
			cgr.viewID = cartViewIDRigidbody;
			cgr.stateSynchronization = NetworkStateSynchronization.Unreliable;
			NetworkViewID ballViewID = Network.AllocateViewID();
			ballGameObject.networkView.viewID = ballViewID;
			NetworkViewID characterViewID = Network.AllocateViewID();
			characterGameObject.networkView.viewID = characterViewID;
			
			// edit their PlayerInfo
			newGuy.cartGameObject = cartGameObject;
			newGuy.cartViewIDTransform = cartViewIDTransform;
			newGuy.cartViewIDRigidbody = cartViewIDRigidbody;
			newGuy.ballGameObject = ballGameObject;
			newGuy.ballViewID = ballViewID;
			newGuy.characterGameObject = characterGameObject;
			newGuy.characterViewID = characterViewID;
			newGuy.currentMode = 0;	// set them in buggy
			newGuy.carController = cartGameObject.transform.GetComponent("CarController") as CarController;

			//add details
			//add player colors
			Renderer bodyRenderer = newGuy.characterGameObject.transform.FindChild ("body").gameObject.GetComponent<SkinnedMeshRenderer> ();
			RecolorPlayer.recolorPlayerBody (bodyRenderer, newGuy.color);

			// tell everyone else about it
			networkView.RPC("SpawnPrefab", RPCMode.Others, cartViewIDTransform, spawnLocation, velocity, newGuy.cartModel);
			networkView.RPC("SpawnPrefab", RPCMode.Others, ballViewID, spawnLocation, velocity, newGuy.ballModel);
			networkView.RPC("SpawnPrefab", RPCMode.Others, characterViewID, spawnLocation, velocity, newGuy.characterModel);
			
			// tell all players this is a player and not some random objects
			networkView.RPC("SpawnPlayer", RPCMode.Others, cartViewIDTransform, cartViewIDRigidbody, ballViewID, characterViewID, 0, newGuy.player);

			if (newGuy.player!=myInfo.player) {
				// tell the player it's theirs
				networkView.RPC("ThisOnesYours", newGuy.player, cartViewIDTransform, ballViewID, characterViewID);
			}
		}

	}
	
	// fired when a player joins (if you couldn't tell)
	void OnPlayerConnected(NetworkPlayer player) {
		networkView.RPC("PrintText", player, "Welcome to the test server");
		PrintText("Someone joined");
		
		// add them to the list
		PlayerInfo newGuy = new PlayerInfo();
		newGuy.player = player;
		newGuy.name = "Some guy";
		newGuy.cartModel = nvs.buggyModels[0];
		newGuy.ballModel = nvs.ballModels[0];
		newGuy.characterModel = nvs.characterModels[0];

		string[] tmp = new string[Config.colorsDictionary.Count ];
		Config.colorsDictionary.Keys.CopyTo(tmp, 0);
		newGuy.color = tmp [0];

		// send all current players to new guy
		foreach (PlayerInfo p in nvs.players)
		{
			/* goodbye old code, you will be missed
			networkView.RPC("SpawnPrefab", player, p.cartViewIDTransform, p.cartGameObject.transform.position, new Vector3(0,0,0), p.cartModel);
			networkView.RPC("SpawnPrefab", player, p.ballViewID, p.ballGameObject.transform.position, new Vector3(0,0,0), p.ballModel);
			networkView.RPC("SpawnPrefab", player, p.characterViewID, p.characterGameObject.transform.position, new Vector3(0,0,0), p.characterModel);
			// tell the player this is a player and not some random objects
			networkView.RPC("SpawnPlayer", player, p.cartViewIDTransform, p.cartViewIDRigidbody, p.ballViewID, p.characterViewID, p.currentMode, p.player);
			*/
			// if we've started then give the new guy the prefabs to watch
			if (gameHasBegun) {
				networkView.RPC("SpawnPrefab", player, p.cartViewIDTransform, p.cartGameObject.transform.position, p.cartGameObject.rigidbody.velocity, p.cartModel);
				networkView.RPC("SpawnPrefab", player, p.ballViewID, p.ballGameObject.transform.position, p.ballGameObject.rigidbody.velocity, p.ballModel);
				networkView.RPC("SpawnPrefab", player, p.characterViewID, p.characterGameObject.transform.position, new Vector3(0,0,0), p.characterModel);
			}

			// tell the new player about the iterated player
			networkView.RPC("AddPlayer", player, p.cartModel, p.ballModel, p.characterModel, p.player, p.name, p.color);

			// tell the iterated player about the new player, unless the iterated player is the server or we have started
			if (p.player!=myInfo.player && !gameHasBegun) {
				networkView.RPC("AddPlayer", p.player, newGuy.cartModel, newGuy.ballModel, newGuy.characterModel, newGuy.player, newGuy.name, newGuy.color);
			}

			// also tell them to spawn this one as a player if they're spectating
			if (gameHasBegun) {
				networkView.RPC("SpawnPlayer", player, p.cartViewIDTransform, p.cartViewIDRigidbody, p.ballViewID, p.characterViewID, p.currentMode, p.player);
			}
		}
		
		// add it to the list if the game hasn't started
		if (!gameHasBegun) {
			nvs.players.Add(newGuy);
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
				if (p.currentMode==0 || p.currentMode==1) {
					// remove their stuff
					Destroy(p.cartGameObject);
					Destroy(p.ballGameObject);
					Destroy(p.characterGameObject);
					// tell everyone else to aswell - move this onto the server
					networkView.RPC("RemoveViewID", RPCMode.All, p.characterViewID);
					networkView.RPC("RemoveViewID", RPCMode.All, p.cartViewIDTransform);
					networkView.RPC("RemoveViewID", RPCMode.All, p.ballViewID);

				} else if (p.currentMode==2) {// if they haven't got anything yet
				}
				// remove from array
				toDelete = p;
			}
		}
		if (nvs.players.Contains(toDelete)) nvs.players.Remove(toDelete);
	}
	
	// debug shit
	void OnGUI() {
		//number of players in game
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


		if( nvs.playerHasWon )
		{
			GUIStyle myStyle = new GUIStyle();
			myStyle.fontSize = 34;
			myStyle.normal.textColor = Color.red;
			
			
			GUI.Label( new Rect( Screen.width/4, 0, 200, 200), nvs.winningPlayer + " is the Winner !",myStyle);
		}

	}
	
	void OnDisconnectedFromServer(NetworkDisconnection info){
		MasterServer.UnregisterHost();
		
		//Go back to main menu
		string nameOfLevel = "main";
		Application.LoadLevel( nameOfLevel );
	}

	void StartGame() {
		Component.Destroy(GetComponent("netLobby"));

		// lookup from level list will be added here
		Application.LoadLevelAdditive("level_full");
		/*
		// don't let anyone else join - this doesn't work (and hasn't since 2010 -_-)
		MasterServer.UnregisterHost();
		// instead make up a password and set it to that
		string tmpPwCuzUnitysShit = "";
		for (int i=0; i<10; i++) {
			tmpPwCuzUnitysShit += (char)(Random.Range(65,90));
		}
		Debug.Log(tmpPwCuzUnitysShit);
		Network.incomingPassword = tmpPwCuzUnitysShit;
		*/

		string serverName = nvs.serverName + ": Game started";
		serverComment.comment = "This is the server comment";
		serverComment.level = "level_full";
		serverComment.locked = true;
		MasterServer.RegisterHost(serverVersion, serverName, serverComment.toString());

		// tell everyone what their choices were
		foreach (PlayerInfo p in nvs.players)
		{
			if (p.player!=myInfo.player) {
				networkView.RPC("StartingGame", p.player, p.cartModel, p.ballModel, p.characterModel);
			}
		}

		// start the game
		BeginGame();
		// set the game to have started
		gameHasBegun = true;
		nvs.playerHasWon = false;
		nvs.winningPlayer = "";
		// call the functions that need them
		AddScripts();
	}

	[RPC]
	void DeclareWinner(NetworkPlayer player){
		nvs.playerHasWon = true;
		foreach(PlayerInfo pInfo in nvs.players){
			if(pInfo.player==player){
				nvs.winningPlayer=pInfo.name;
				break;
			}
		}
	}
	
	// things that can be run over the network
	// debug text
	[RPC]
	void PrintText(string text) {
		Debug.Log(text);
		int i;
		for (i=10; screenMessages.ContainsKey(Time.time+((float)i)/2); i++);
		screenMessages.Add(Time.time+((float)i)/2,"[DEBUG] "+text);
	}

	[RPC]
	void ChangeModels(string cartModel, string ballModel, string characterModel, string color, NetworkMessageInfo info) {
		foreach (PlayerInfo p in nvs.players) {
			if (p.player==info.sender) {
				p.cartModel = cartModel;
				p.ballModel = ballModel;
				p.characterModel = characterModel;
				p.color = color;
				// tell the other clients that it changed
				networkView.RPC ("UpdateModels", RPCMode.Others, p.player, cartModel, ballModel, characterModel, color);
			}
		}
	}

	[RPC]
	void MyName(string name, NetworkMessageInfo info){
		foreach(PlayerInfo p in nvs.players) {
			if (p.player==info.sender) {
				p.name = name;
				networkView.RPC ("UpdateName", RPCMode.Others, p.player, name);
			}
		}

		// tell them they're spectating if we've begun
		if (gameHasBegun) {
			networkView.RPC("YoureSpectating", info.sender);
		}
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
	[RPC]
	void StartingGame(string a, string b, string c) {}
	[RPC]
	void AddPlayer(string cartModel, string ballModel, string characterModel, NetworkPlayer player, string name, string color) {}
	[RPC]
	void UpdateName( NetworkPlayer player, string name){}
	[RPC]
	void YoureSpectating(){}
	[RPC]
	void UpdateModels(NetworkPlayer player, string cartModel, string ballModel, string characterModel, string color) {}
}
