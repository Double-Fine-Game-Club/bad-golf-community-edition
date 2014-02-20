using UnityEngine;
using System.Collections;

public class netPause : MonoBehaviour {

	public GameObject ed_pauseScreen;

	private string nameOfLevel;

	// Use this for initialization
	void Start () {
		ed_pauseScreen = GameObject.Find ("pauseScreen");
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
		this.SendMessageUpwards ("onPauseScreen");
	}

	void onResume(){
		hideAllScreens ();
		this.SendMessageUpwards ("onResumeScreen");
	}

	void onExit(){
		this.SendMessageUpwards ("onDisconnect");

		hideAllScreens();
	}
}
