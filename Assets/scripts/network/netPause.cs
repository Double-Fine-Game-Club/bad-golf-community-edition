using UnityEngine;
using System.Collections;

public class netPause : MonoBehaviour {

	public GameObject ed_pauseScreen;
	networkVariables nvs;
	PlayerInfo myInfo;

	// Use this for initialization
	void Start () {
		// get variables we need
		nvs = GameObject.FindWithTag("NetObj").GetComponent("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		ed_pauseScreen = Instantiate (Resources.Load ("pauseScreen")) as GameObject;
		ed_pauseScreen.transform.parent = this.transform;
		//Hide the screen while it is inactive
		hideAllScreens ();
	}
	
	// Update is called once per frame
	void Update () {
		// pause menu toggler
		if(Input.GetKeyDown(KeyCode.Escape)) {
			if(myInfo.playerIsPaused){				// if paused resume
				onResume();
			}else if(!myInfo.playerIsBusy){			// if not busy then pause
				onPause();
			}
		}
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
		myInfo.playerIsPaused = false;	//reset pause status
		hideAllScreens();
		Destroy (ed_pauseScreen);		//undo changes to object hierarchy
		if(nvs.gameMode==GameMode.Online){
			Network.Disconnect ();
		}else if(nvs.gameMode==GameMode.Local){
			Application.LoadLevel("main");
		}

		
	}
}
