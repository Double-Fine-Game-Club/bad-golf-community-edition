using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkManager : MonoBehaviour {
	bool connectingToServer = false;
	string serverVersion;
	string nameBuffer;
	
	// Use this for initialization
	void Start () {
		// change to custom master server
		MasterServer.ipAddress = "37.157.247.37";
		MasterServer.port = 23466;
		// get server version
		serverVersion = (GetComponent("networkVariables") as networkVariables).serverVersion;
		// get them servers
		MasterServer.ClearHostList();
		MasterServer.RequestHostList(serverVersion);
		// set default name
		nameBuffer = SystemInfo.deviceName;
	}
	
	void OnGUI() {
		if (connectingToServer) {
			GUILayout.Label("Connecting to server...");
		} else {
			if (GUILayout.Button ("Host a server"))
			{
				// set name
				(GetComponent("networkVariables") as networkVariables).myInfo.name = nameBuffer;
				// add the server script to us
				gameObject.AddComponent("networkManagerServer");
				// remove this script
				(gameObject.GetComponent("networkManager") as networkManager).enabled = false;
			}
			if (GUILayout.Button ("Refresh server list"))
			{
				// get them servers
				MasterServer.ClearHostList();
				MasterServer.RequestHostList(serverVersion);
			}

			GUI.Label(new Rect(Screen.width-250,20,50,20), "Name:");
			nameBuffer = GUI.TextField(new Rect(Screen.width-200,20,150,20), nameBuffer, 32);
			
			HostData[] data = MasterServer.PollHostList();
			// Go through all the hosts in the host list
			foreach (HostData element in data)
			{
				GUILayout.BeginHorizontal();
				string name = element.gameName + " " + element.connectedPlayers + " / " + element.playerLimit;
				GUILayout.Label(name);
				GUILayout.Space(5);
				string hostInfo = "[";
				foreach (var host in element.ip) hostInfo = hostInfo + host + ":" + element.port + " ";
				hostInfo = hostInfo + "]";
				GUILayout.Label(hostInfo);
				GUILayout.Space(5);
				GUILayout.Label(element.comment);
				GUILayout.Space(5);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Connect"))
				{
					// Connect to HostData struct, internally the correct method is used (GUID when using NAT).
					Network.Connect(element);
					connectingToServer = true;
				}
				GUILayout.EndHorizontal();
			}
		}
	}

	void OnConnectedToServer() {
		// set name
		(GetComponent("networkVariables") as networkVariables).myInfo.name = nameBuffer;
		// add the client script to us
		gameObject.AddComponent("networkManagerClient");
		// remove this script
		(gameObject.GetComponent("networkManager") as networkManager).enabled = false;
	}
}
