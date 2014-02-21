using UnityEngine;
using System.Collections;

public class netPause : MonoBehaviour {

	public GameObject ed_pauseScreen;
	PlayerInfo myInfo;

	private string nameOfLevel;

	// Use this for initialization
	void Start () {
		// get variables we need
		myInfo = (GetComponent("networkVariables") as networkVariables).myInfo;
		ed_pauseScreen = GameObject.Find ("pauseScreen");
		
		//Hide the screen while it is inactive
		hideAllScreens ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	void hideAllScreens(){
		ed_pauseScreen.SetActive (false);
	}

	void onPause(){
		hideAllScreens ();
		ed_pauseScreen.SetActive (true);
		myInfo.playerIsPaused = true;
	}

	void onResume(){
		hideAllScreens ();
		myInfo.playerIsPaused = false;
	}

	void onExit(){
		this.SendMessageUpwards ("onDisconnect");
		hideAllScreens();
		myInfo.playerIsPaused = false;	//reset pause status
	}
}
