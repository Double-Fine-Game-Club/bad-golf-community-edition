using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;	// remove me later

public class networkManager : MonoBehaviour {
	const float kConnectionTimeout = 10f;	//quit trying to connect after this much time has passed
	float timeoutTime = 0f;					//time spent trying to connect to server
	bool connectingToServer = false;		//is trying to connect to a server
	bool showFailMessage = false;			//Indicate a connection attempt has failed
	string serverVersion;
	networkVariables nvs;
	// random names
	//string[] randomNames = new string[3] {"Leslie", "Test", "REPLACE ME"};
	
	/****************************************************
	 * 
	 * DO EDIT THIS SCRIPT
	 * 
	 ****************************************************/
	
	// Use this for initialization
	void Start () {
		// change to custom master server
		MasterServer.ipAddress = "37.157.247.37";
		MasterServer.port = 23466;
		// NAT punchthrough (finally)
		Network.natFacilitatorIP = "37.157.247.37";
		Network.natFacilitatorPort = 50005;

		nvs = GetComponent("networkVariables") as networkVariables;
		// get server version
		serverVersion = nvs.serverVersion;

		// get them servers
		MasterServer.ClearHostList();
		MasterServer.RequestHostList(serverVersion);

		// set default player name
		nvs.myInfo.name = SystemInfo.deviceName;
		//nvs.myInfo.name = randomNames[Random.Range(0,randomNames.Length-1)];

		// set default server name
		nvs.serverName = nvs.myInfo.name + "'s Server";
	}

	void Update(){
		//If NAT punchthrough fails, no connected or fail to connect
		//	events occur so we need a timer to prevent game lock
		if(connectingToServer){
			timeoutTime += Time.deltaTime;
			if(timeoutTime>=kConnectionTimeout){
				connectingToServer=false;
				showFailMessage=true;
			}
		}
	}
	
	void OnGUI() {
		// if we are connecting to a server
		if (connectingToServer) {
			GUILayout.Label("Connecting to server...");

		// if we aren't connecting to a server
		} else {
			if (GUILayout.Button ("Host a server"))
			{
				//disable menu level preview - "main" doesn't exist if debugin
				if(GameObject.Find("main"))
				{
					GameControl gCtrl = GameObject.Find("main").GetComponent(typeof(GameControl)) as GameControl;
					gCtrl.ed_levelPreviewScreen.SetActive(false);
				} else {
					InputManager.Setup();
				}
				// add the server script to us
				gameObject.AddComponent("networkManagerServer");
				// disable this script
				this.enabled = false;
			}
			if (GUILayout.Button ("Refresh server list"))
			{
				// get them servers
				MasterServer.ClearHostList();
				MasterServer.RequestHostList(serverVersion);
			}
			
			if(GUILayout.Button ("Back")){
				//Go back to main menu
				string nameOfLevel = "main";
				Application.LoadLevel( nameOfLevel );
			}



			// HACKY - REPLACE ME!
			GUI.Label(new Rect(Screen.width-340,20,100,20), "Player name:");
			nvs.myInfo.name = GUI.TextField(new Rect(Screen.width-200,20,180,20), nvs.myInfo.name, 32);
			GUI.Label(new Rect(Screen.width-340,40,100,20), "Server name:");
			nvs.serverName = GUI.TextField(new Rect(Screen.width-200,40,180,20), nvs.serverName, 32);



			// AND THIS BIT ASWELL
			HostData[] data = MasterServer.PollHostList();
			// Go through all the hosts in the host list
			foreach (HostData element in data)
			{
				GUILayout.BeginHorizontal();
				if (element.passwordProtected) {
					GUILayout.Label("Locked");
				} else {
					GUILayout.Label("");
				}
				GUILayout.Space(5);
				string name = element.gameName;
				GUILayout.Label(name);
				GUILayout.Space(5);
				GUILayout.Label(element.connectedPlayers + " / " + element.playerLimit);
				//string hostInfo = "[";
				//foreach (var host in element.ip) hostInfo = hostInfo + host + ":" + element.port + " ";
				//hostInfo = hostInfo + "]";
				//GUILayout.Label(hostInfo);
				GUILayout.Space(5);
				GUILayout.Label(element.comment);
				GUILayout.Space(5);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Connect"))
				{
					// Connect to HostData struct, internally the correct method is used (GUID when using NAT).
					Network.Connect(element);
					connectingToServer = true;
					timeoutTime = 0;
					showFailMessage = false;
				}
				GUILayout.EndHorizontal();
			}
			if (showFailMessage) 
				GUILayout.Label("Failed to connect");
		}
	}
	
	void OnConnectedToServer() {
		//disable menu level preview - "main" doesn't exist if debugin
		if(GameObject.Find("main"))
		{
			GameControl gCtrl = GameObject.Find("main").GetComponent(typeof(GameControl)) as GameControl;
			gCtrl.ed_levelPreviewScreen.SetActive(false);
		} else {
			InputManager.Setup();
		}
		// add the client script to us
		gameObject.AddComponent("networkManagerClient");
		// disable this script
		this.enabled = false;
	}

	void OnFailedToConnect(NetworkConnectionError error){
		connectingToServer = false;
		showFailMessage = true;
	}

}