using UnityEngine;
using System.Collections;

public class netChat : MonoBehaviour {
	NetworkViewID myViewID;
	string chatBuffer = "";
	bool chatVisible = false;

	void Start () {
		// get variables we need
		networkVariables nvs = GetComponent("networkVariables") as networkVariables;
		myViewID = nvs.myInfo.cartViewID;
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
				networkView.RPC("PrintText", RPCMode.All, myViewID.ToString().Substring(13) + ": " + chatBuffer);
				chatBuffer = "";
			}
		}
		// show chat box if needed
		if (chatVisible) {
			GUI.SetNextControlName("ChatBox");
			chatBuffer = GUI.TextField(new Rect(10,Screen.height/2,200,20), chatBuffer, 64);
			GUI.FocusControl("ChatBox");
		}
		// look for T event
		if ((Event.current.type == EventType.KeyUp) && (Event.current.keyCode == KeyCode.T)) {
			chatVisible = true;
		}
	}
}
