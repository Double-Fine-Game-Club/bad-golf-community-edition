using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkManagerClient : MonoBehaviour {
	Dictionary<float,string> screenMessages = new Dictionary<float,string>();
	PlayerInfo myInfo;
	bool connected = false;
	networkVariables nvs;
	
	// Use this for initialization
	void Start () {
		// setup reference to networkVariables
		nvs = gameObject.GetComponent("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		
		// get self
		myInfo.player = Network.player;

		// add us to the player list
		nvs.players.Add(myInfo);

		networkView.RPC("MyName", RPCMode.Server, nvs.myInfo.name);	
		
		// go into the lobby
		gameObject.AddComponent("netLobby");
	}
	
	// CLIENT SIDE SCRIPTS GO HERE
	void AddScripts() {
		// Anything you want to have running in the lobby should go in the netLobby script.
		// This function gets called when a game starts.
		// All scripts are called after we have a reference to the buggy and a NetworkViewID.

		// updates network-sunk fiziks
		gameObject.AddComponent("controlClient");

		//hit ball
		gameObject.AddComponent ("netSwing");

		//ball range finder
		gameObject.AddComponent ("netTransferToSwing");
		
		//ball marker
		BallMarker bms = gameObject.AddComponent ("BallMarker") as BallMarker;
		bms.m_nvs = nvs;
		bms.m_myCamera = nvs.myCam;	// can be set in the script instead
		
		//show names over player's cart
		gameObject.AddComponent ("PlayerNames");

		// set the camera in the audio script on the buggy - PUT THIS IN A SCRIPT SOMEONE
		CarAudio mca = myInfo.cartGameObject.GetComponent("CarAudio") as CarAudio;
		mca.followCamera = nvs.myCam;	// replace tmpCam with our one - this messes up sound atm
		(nvs.myCam.gameObject.AddComponent("FollowPlayerScript") as FollowPlayerScript).target = myInfo.cartGameObject.transform;	// add smooth follow script
		
		// finally disable the preview scene
		(GameObject.Find("main").GetComponent(typeof(GameControl)) as GameControl).ed_levelPreviewScreen.SetActive(false);
		
		 //show chat bubble over players when they chat

		gameObject.AddComponent("ChatBubble");
		
		//cart reset
		gameObject.AddComponent ("netPlayerRespawn");
		
		// show that we connected
		connected = true;
	}
	
	// debug shit
	void OnGUI() {
		if (!connected) return;
		//number of players in game
		GUILayout.BeginHorizontal();
		GUILayout.Label("Active players: ");
		GUILayout.Label (nvs.players.Count.ToString());
		GUILayout.EndHorizontal();
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

		if( nvs.playerHasWon )
		{
			GUIStyle myStyle = new GUIStyle();
			myStyle.fontSize = 34;
			myStyle.normal.textColor = Color.red;
			
			
			GUI.Label( new Rect( Screen.width/4, 0, 200, 200), nvs.winningPlayer + " is the Winner !",myStyle);
		}
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection info){
		//Disconnected from the server for some reason
		if (info == NetworkDisconnection.LostConnection) {
			// maybe show this to the player?
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
	
	[RPC]
	void StartingGame(string cartModel, string ballModel, string characterModel) {
		Component.Destroy(GetComponent("netLobby"));
		
		// lookup from level list will be added here
		Application.LoadLevelAdditive("level_full");

		// set out stuff
		nvs.myInfo.cartModel = cartModel;
		nvs.myInfo.ballModel = ballModel;
		nvs.myInfo.characterModel = characterModel;

		nvs.playerHasWon = false;
		nvs.winningPlayer = "";
	}
	
	// tells the player that this viewID is theirs - can be moved to SpawnPlayer now
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
		// get server
		myInfo.server = myInfo.cartViewIDTransform.owner;
		// call the functions that need them
		AddScripts();
	}
	
	// tells the player that this set of viewIDs are a player
	[RPC]
	void SpawnPlayer(NetworkViewID cartViewIDTransform, NetworkViewID cartViewIDRigidbody, NetworkViewID ballViewID, NetworkViewID characterViewID, int mode, NetworkPlayer player) {
		foreach(PlayerInfo newGuy in nvs.players) {
			if(newGuy.player==player){
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
				newGuy.carController = newGuy.cartGameObject.transform.GetComponent("CarController") as CarController;

				//Move this to a point that is post all object creation but pre level start
				Renderer bodyRenderer = newGuy.characterGameObject.transform.FindChild ("body").gameObject.GetComponent<SkinnedMeshRenderer> ();
				RecolorPlayer.recolorPlayerBody(bodyRenderer, newGuy.color);
				
				// ADD MORE STUFF HERE
				if (newGuy.currentMode==0){
					// set them inside the buggy
					newGuy.characterGameObject.transform.parent = newGuy.cartGameObject.transform;
					newGuy.characterGameObject.transform.localPosition = new Vector3(0,0,0);
					newGuy.characterGameObject.transform.localRotation = Quaternion.identity;
				} else if (newGuy.currentMode==1) {
					// set them outside the buggy
					newGuy.characterGameObject.transform.parent = newGuy.ballGameObject.transform;
					newGuy.characterGameObject.transform.localPosition = new Vector3(0,0,-2);
					newGuy.characterGameObject.transform.localRotation = Quaternion.identity;
				}
			}
		}
		
	}

	// tells the player that someone joined
	[RPC]
	void AddPlayer(string cartModel, string ballModel, string characterModel, NetworkPlayer player, string name, string color) {
		PlayerInfo newGuy = new PlayerInfo();
		newGuy.cartModel = cartModel;
		newGuy.ballModel = ballModel;
		newGuy.characterModel = characterModel;
		newGuy.currentMode = 2;
		newGuy.player = player;
		newGuy.name = name;
		newGuy.color = color;

		// need to add this as sometimes AddPlayer is called BEFORE Start - should have used Awake thinking about it...
		nvs = gameObject.GetComponent("networkVariables") as networkVariables;
		// add it to the list
		nvs.players.Add(newGuy);
		Debug.Log("added someone");
		Debug.Log(player.ToString());
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

	[RPC]
	void UpdateName( NetworkPlayer player, string name){
		foreach(PlayerInfo p in nvs.players) {
			if (p.player==player) {
				p.name = name;
			}
		}
	}

	// tells the player they're spectating - REPLACE ME
	[RPC]
	void YoureSpectating() {
		netLobby ntl = GetComponent("netLobby") as netLobby;
		ntl.enabled = false;
		Component.Destroy(ntl);
		
		// show that we connected
		connected = true;

		// add crappy controls
		nvs.myCam.gameObject.AddComponent("SpectatorView");
		// remove us as we aren't playing
		nvs.players.Remove(myInfo);
	}

	// tell them that someone changed their models
	[RPC]
	void UpdateModels(NetworkPlayer player, string cartModel, string ballModel, string characterModel, string color) {
		foreach (PlayerInfo p in nvs.players) {
			if (p.player==player) {
				p.cartModel = cartModel;
				p.ballModel = ballModel;
				p.characterModel = characterModel;
				p.color = color;
			}
		}
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

	// blank for server use only
	[RPC]
	void GiveMeACart(string a, string b, string c) {}
	[RPC]
	void MyName(string a) {}
}
