﻿using UnityEngine;
using System.Collections;

public class netLobby : MonoBehaviour {
	networkVariables nvs;
	int IcartModel=0, IballModel=0, IcharacterModel=0;
	float timer = 0;
	bool changeNeeded = false;
	string currentList = "";
	Vector2 scrollPosition = Vector2.zero;
	int numPlayers=1;	

	// Use this for initialization
	void Start () {
		nvs = GetComponent("networkVariables") as networkVariables;

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
	}
	
	// Update is called once per frame
	void Update () {
		// don't spam on change
		timer += Time.deltaTime;
		if (timer > 0.1 && changeNeeded) {
			timer = 0;
			changeNeeded = false;
			networkView.RPC("ChangeModels", RPCMode.Server, nvs.myInfo.cartModel, nvs.myInfo.ballModel, nvs.myInfo.characterModel);
		}
		if (nvs.players.Count != numPlayers && (nvs.players[nvs.players.Count-1] as PlayerInfo).name != "Some guy") {
			numPlayers = nvs.players.Count;	
			rebuildPlayerList();
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
		if (GUI.Button(new Rect(Screen.width/4,80,Screen.width/2,20), nvs.ballModelNames[IballModel]))
		{
			// change to next model
			IballModel += 1;
			IballModel %= nvs.ballModels.Length;
			nvs.myInfo.ballModel = nvs.ballModels[IballModel];
			changeNeeded = true;
		}
		if (GUI.Button(new Rect(Screen.width/4,120,Screen.width/2,20), nvs.characteryModelNames[IcharacterModel]))
		{
			// change to next model
			IcharacterModel += 1;
			IcharacterModel %= nvs.characterModels.Length;
			nvs.myInfo.characterModel = nvs.characterModels[IcharacterModel];
			changeNeeded = true;
		}
		if (GUI.Button(new Rect(Screen.width/4,160,Screen.width/2,20), "level_full"))
		{
			// change to next level
			//IcharacterModel += 1;
			//IcharacterModel %= nvs.characterModels.Length;
			//nvs.myInfo.characterModel = nvs.characterModels[IcharacterModel];
			//changeNeeded = true;
		}
		if (Network.isServer) {
			if (GUI.Button(new Rect(Screen.width/4,200,Screen.width/2,20), "Start!"))
			{
				GetComponent("networkManagerServer").SendMessage("StartGame");
				this.enabled = false;
			}
		}
		
		GUI.BeginGroup(new Rect(Screen.width/4,240,Screen.width/2,250));
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
	void ChangeModels(string a, string b, string c) {}
}
