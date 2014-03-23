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
	string testStatus = "Please wait while we test your internet connection...";		//net connection test message
	bool doneTesting = false;															//are we done testing
	bool probingPublicIP = false;														//if we're doing something internally
	float NATtimer = 0;																	//just a timer
	ConnectionTesterStatus connectionTestResult = ConnectionTesterStatus.Undetermined;	//for the complex stuff
	// NATmode says what we can and can't connect to:
	//   | 0 | 1 | 2 |
	// ---------------
	// 0 | x | x | x |
	// 1 | x | x |   |
	// 2 | x |   |   |
	//
	// 0 = everything works
	// 1 = port restricted
	// 2 = sym - very bad
	// -1 = error if doneTesting==true
	int NATmode = -1;
	
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
		
		// test the current setup rather than poll for the result
		connectionTestResult = Network.TestConnection(true);
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
		// check connection type and screen printout
		if (testStatus!="") GUILayout.Label(testStatus);
		if (!doneTesting) {
			TestConnection();
		} else if (NATmode!=-1) {	// only carry on if no error
			// if we are connecting to a server
			if (connectingToServer) {
				GUILayout.Label("Connecting to server...");

			// if we aren't connecting to a server
			} else {
				// why does this cause an error? It seems to not like being inside an if inside an if inside an if! :S
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
					// get the server data from its comment
					ServerComment hostParams = new ServerComment(element.comment);
					// only show the server if it's possible to connect to it
					if (hostParams.NATmode+nvs.NATmode<=2) {
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
						GUILayout.Label(hostParams.comment);
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
				}
				if (showFailMessage) 
					GUILayout.Label("Failed to connect");
			}
		}
		
		if(GUILayout.Button ("Back")){
			//Go back to main menu
			string nameOfLevel = "main";
			Application.LoadLevel( nameOfLevel );
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

	
	void TestConnection() {
		// Start/Poll the connection test, report the results in a label and 
		// react to the results accordingly
		if (probingPublicIP) {
			connectionTestResult = Network.TestConnectionNAT();
		} else {
			connectionTestResult = Network.TestConnection();
		}
		switch (connectionTestResult) {
		case ConnectionTesterStatus.Error:	// check your internet connection
			testStatus = "Error testing your connection.\nMake sure you are connected to the internet and try again.";
			doneTesting = true;
			break;
			
		case ConnectionTesterStatus.Undetermined:	// still connecting
			break;
			
		case ConnectionTesterStatus.PublicIPIsConnectable:	// everythings fine
			testStatus = "";
			doneTesting = true;
			NATmode = 0;
			break;
			
			// This case is a bit special as we now need to check if we can 
			// circumvent the blocking by using NAT punchthrough
		case ConnectionTesterStatus.PublicIPPortBlocked:
		case ConnectionTesterStatus.PublicIPNoServerStarted:	// ignore the fact it needs a server and just go for it!
			testStatus = "Checking NAT punchthough capabilities...";
			// If no NAT punchthrough test has been performed on this public 
			// IP, force a test
			if (!probingPublicIP) {
				connectionTestResult = Network.TestConnectionNAT(true);	// force a pull
				probingPublicIP = true;
				NATtimer = Time.time + 10;
			}
			// NAT punchthrough test was performed but we still get blocked - ie it did it again
			else if (Time.time > NATtimer) {
				testStatus = "";
				probingPublicIP = false; 		// reset - don't see the point in this?
				NATmode = 1;	// could be 2? I have no idea what I'm doing!
				doneTesting = true;
			}
			break;
			
		case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:	// cannot connect to or from sym
			testStatus = "You may have issues hosting a server.\n"+
				"Any servers you can't connect to have also been removed.";
			doneTesting = true;
			NATmode = 1;
			break;
			
		case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:	// sym cannot connect to anything using NAT-punch
			testStatus = "You may have issues hosting a server.\n"+
				"Any servers you can't connect to have also been removed.";
			doneTesting = true;
			NATmode = 2;
			break;
			
		case ConnectionTesterStatus.NATpunchthroughAddressRestrictedCone:	// everything works
		case ConnectionTesterStatus.NATpunchthroughFullCone:				// everything works
			testStatus = "";
			doneTesting = true;
			NATmode = 0;
			break;
			
		default:	// error
			testStatus = "Error in test routine, got " + connectionTestResult + ".\nTell a dev!";
			break;
		}
		if (doneTesting) {
			// if we're done then update nvs
			nvs.NATmode = NATmode;
		}
	}

}