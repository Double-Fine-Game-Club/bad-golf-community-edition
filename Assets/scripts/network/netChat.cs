using UnityEngine;
using System.Collections;

public class netChat : MonoBehaviour {
	string myName;
    PlayerInfo myInfo;
	string chatBuffer = "";
	bool chatVisible = false;
	string currentList = "Chat log:";
	Vector2 scrollPosition = Vector2.zero;
	bool updateScroll = false;
	
	void Start () {
		// get variables we need
		networkVariables nvs = GetComponent("networkVariables") as networkVariables;
		myName = nvs.myInfo.name;
        myInfo = nvs.myInfo;
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
				networkView.RPC("SendChatMessage", RPCMode.All, myName + ": " + chatBuffer, myInfo.ballViewID);
				chatBuffer = "";
			}
		}
		// show chat box if needed
		if (chatVisible) {
			GUI.SetNextControlName("ChatBox");
			chatBuffer = GUI.TextField(new Rect(10,Screen.height/2,Screen.width/2,20), chatBuffer, 96);
			GUI.FocusControl("ChatBox");
		}
		// look for T event
		if ((Event.current.type == EventType.KeyUp) && (Event.current.keyCode == KeyCode.T)) {
			chatVisible = true;
		}
		// do the chat stuff
		GUI.BeginGroup(new Rect(10,Screen.height/2 + 20,Screen.width/2,100));
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxWidth(Screen.width/2), GUILayout.MaxHeight(100));
		// show chat
		GUILayout.Label(currentList, GUILayout.ExpandHeight(true));
		GUILayout.EndScrollView();
		GUI.EndGroup();
		// scroll it if needed
		if (updateScroll) {
			// scroll to a ridiculous height
			scrollPosition = new Vector2(0, Mathf.Max(10000,scrollPosition.y*2));
		}
	}
	
	// recieved a message
	[RPC]
	void SendChatMessage(string text, NetworkViewID myID) {
		Debug.Log(text);
		currentList = currentList + "\n" + text;
		updateScroll = true;

        if (ChatBubble.Instance != null) {
            ChatBubble.DisplayChat(myID);
        }
	}
}
