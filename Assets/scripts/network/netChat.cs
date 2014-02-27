﻿using UnityEngine;
using System.Collections;

public class netChat : MonoBehaviour {
	string myName;
	string chatBuffer = "";
	bool chatVisible = false;
	int messageCount = 1;
	string currentList = "Chat log:";
	Vector2 scrollPosition = Vector2.zero;
	int fontSize = 18;	// massive fonts due to Linux
    NetworkViewID myID;

	void Start () {
		// get variables we need
		networkVariables nvs = GetComponent("networkVariables") as networkVariables;
		myName = nvs.myInfo.name;
        myID = nvs.myInfo.ballViewID;
	}
	
	// chat box
	void OnGUI() {
		// cancel if esc is pressed
		if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Escape)) {
			chatVisible = false;
			chatBuffer = "";
		}
		// send if they hit enter
		if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.Return)) {
			chatVisible = false;
			if (chatBuffer!="") {
				// this line sends the message
				networkView.RPC("SendChatMessage", RPCMode.All, myName + ": " + chatBuffer, myID);
				chatBuffer = "";
			}
		}
		// show chat box if needed
		if (chatVisible) {
			GUI.SetNextControlName("ChatBox");
			chatBuffer = GUI.TextField(new Rect(10,Screen.height/2,Screen.width/2,20), chatBuffer, 64);
			GUI.FocusControl("ChatBox");
		}
		// look for T event
		if ((Event.current.type == EventType.KeyUp) && (Event.current.keyCode == KeyCode.T)) {
			chatVisible = true;
		}
		scrollPosition = GUI.BeginScrollView(new Rect(10,Screen.height/2 + 20,Screen.width/2,100), scrollPosition, new Rect(0,0,Screen.width/2,fontSize*messageCount));
		// show chat
		GUI.Label(new Rect(0,0,Screen.width/2,fontSize*messageCount+4), currentList);
		GUI.EndScrollView();
	}

	// recieved a message
	[RPC]
	void SendChatMessage(string text, NetworkViewID ID) {
		Debug.Log(text);
		messageCount++;
		currentList = currentList + "\n" + text;
		// scroll it
		scrollPosition = new Vector2(0, fontSize*Mathf.Max(0,messageCount-4));
        //display chat bubble over speaking player
        if (ChatBubble.Instance != null) {
            ChatBubble.DisplayChat(ID);
        }
	}
}
