using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkManager : MonoBehaviour {
	bool connectingToServer = false;
	NetworkViewID myViewID;
	GameObject myCart;
	
	// Use this for initialization
	void Start () {
		// change to custom master server
		MasterServer.ipAddress = "37.157.247.37";
		MasterServer.port = 23466;
		// get them servers
		MasterServer.ClearHostList();
		MasterServer.RequestHostList("HL4");
	}
	
	void OnGUI() {
		if (connectingToServer) {
			GUILayout.Label("Connecting to server...");
		} else {
			if (GUILayout.Button ("Host a server"))
			{
				// add the server script to us
				gameObject.AddComponent("networkManagerServer");
				// set anything that needs to be set
				// remove this script
				(gameObject.GetComponent("networkManager") as networkManager).enabled = false;
			}
			if (GUILayout.Button ("Refresh server list"))
			{
				// get them servers
				MasterServer.ClearHostList();
				MasterServer.RequestHostList("HL4");
			}
			
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
		// add the client script to us
		gameObject.AddComponent("networkManagerClient");
		// remove this script
		(gameObject.GetComponent("networkManager") as networkManager).enabled = false;
	}
}
