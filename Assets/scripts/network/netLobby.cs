using UnityEngine;
using System.Collections;

public class netLobby : MonoBehaviour {
	networkVariables nvs;
	int IcartModel=0, IballModel=0, IcharacterModel=0,
		Icolor=0;
	float timer = 0;
	bool changeNeeded = false;
	string currentList = "";
	Vector2 scrollPosition = Vector2.zero;
	int numPlayers=1;	

	public string[] colorKeys;

	// Use this for initialization
	void Start () {
		nvs = GetComponent("networkVariables") as networkVariables;
		nvs.gameMode = GameMode.Online;

		// set models
		nvs.myInfo.cartModel = nvs.buggyModels[IcartModel];
		nvs.myInfo.ballModel = nvs.ballModels[IballModel];
		nvs.myInfo.characterModel = nvs.characterModels[IballModel];
		
		// add chat
		gameObject.AddComponent("netChat");
		
		//pause
		gameObject.AddComponent ("netPause");

		// add self to the list of people in this game
		currentList = nvs.myInfo.name;

		// copy the script on the previewCamera in main
		Orbit cao = nvs.myCam.gameObject.AddComponent("Orbit") as Orbit;
		cao.Point = new Vector3(28,12,147);
		cao.Axis = new Vector3(0,1,0);
		cao.Speed = 0.1f;
		nvs.myCam.transform.position = new Vector3(26,64,86);
		nvs.myCam.transform.rotation = Quaternion.Euler(32,Time.time*0.1f,0);	// the Time.time is for the rotation already

		//get colors
		colorKeys = new string[Config.colorsDictionary.Count ];
		Config.colorsDictionary.Keys.CopyTo( colorKeys,0);
		nvs.myInfo.color = colorKeys [0];
	}
	
	// Update is called once per frame
	void Update () {
		// don't spam on change
		timer += Time.deltaTime;
		if (timer > 0.1 && changeNeeded) {
			timer = 0;
			changeNeeded = false;
			// the server needs to confirm this before it is sent to the clients
			// currently there's nothing in place for the clients to get this info I know, but client<->client interaction is BAD
			//if(Network.isClient)	//because server color wasn't being updated by clients
				networkView.RPC("ChangeModels", RPCMode.Server, nvs.myInfo.cartModel, nvs.myInfo.ballModel, nvs.myInfo.characterModel, nvs.myInfo.color);
			//else
				//networkView.RPC ("UpdateModels", RPCMode.Others, nvs.myInfo.player, nvs.myInfo.cartModel, nvs.myInfo.ballModel, nvs.myInfo.characterModel, nvs.myInfo.color);
		}
		if (nvs.players.Count != numPlayers && (nvs.players[nvs.players.Count-1] as PlayerInfo).name != "Some guy") {
			numPlayers = nvs.players.Count;	
			rebuildPlayerList();
		}
	}

	// remove the rotation camera
	void OnDestroy() {
		if (nvs.myCam.gameObject && nvs.myCam.gameObject.GetComponent<Orbit>()) {
			Component.Destroy(nvs.myCam.gameObject.GetComponent<Orbit>());
		}
	}

	
	void OnGUI() {
		if (GUI.Button(new Rect(Screen.width/4,40,Screen.width/2,20), nvs.buggyModelNames[IcartModel]))
		{
			// change to next model
			IcartModel += 1;
			IcartModel %= nvs.buggyModels.Length;
			nvs.myInfo.cartModel = nvs.buggyModels[IcartModel];
			changeNeeded = true;
		}
		if (GUI.Button(new Rect(Screen.width/4,70,Screen.width/2,20), nvs.ballModelNames[IballModel]))
		{
			// change to next model
			IballModel += 1;
			IballModel %= nvs.ballModels.Length;
			nvs.myInfo.ballModel = nvs.ballModels[IballModel];
			changeNeeded = true;
		}
		if (GUI.Button(new Rect(Screen.width/4,100,Screen.width/2,20), nvs.characterModelNames[IcharacterModel]))
		{
			// change to next model
			IcharacterModel += 1;
			IcharacterModel %= nvs.characterModels.Length;
			nvs.myInfo.characterModel = nvs.characterModels[IcharacterModel];
			changeNeeded = true;
		}

		if (GUI.Button(new Rect(Screen.width/4,130,Screen.width/2,20), "level_full"))
		{
			// change to next level
			//IcharacterModel += 1;
			//IcharacterModel %= nvs.characterModels.Length;
			//nvs.myInfo.characterModel = nvs.characterModels[IcharacterModel];
			//changeNeeded = true;
		}

		if (GUI.Button(new Rect(Screen.width/4,160,Screen.width/2,20), colorKeys[Icolor]))
		{
			// change to next model
			Icolor += 1;
			Icolor %= colorKeys.Length;
			nvs.myInfo.color = colorKeys[Icolor];
			changeNeeded = true;

		}
		if (Network.isServer) {
			if (GUI.Button(new Rect(Screen.width/4,190,Screen.width/2,20), "Start!"))
			{
				GetComponent("networkManagerServer").SendMessage("StartGame");
				this.enabled = false;
			}
		}
		
		GUI.BeginGroup(new Rect(Screen.width/4,220,Screen.width/2,250));
		GUILayout.Label ("Players in Lobby:");
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxWidth(Screen.width/2), GUILayout.MaxHeight(200));
		// show chat
		GUILayout.Label(currentList, GUILayout.ExpandHeight(true));
		GUILayout.EndScrollView();
		GUI.EndGroup();

	}

	void rebuildPlayerList(){
		currentList = "";
		foreach (PlayerInfo playerInfo in nvs.players) {	
			currentList += playerInfo.name + '\n';		
		}
	}

	// blanks
	[RPC]
	void ChangeModels(string a, string b, string c, string d) {}
}
