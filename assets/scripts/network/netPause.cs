using UnityEngine;
using System.Collections;

public class netPause : MonoBehaviour {

	public GameObject ed_pauseScreen;

	private string nameOfLevel;
	private bool isPaused=false;

	// Use this for initialization
	void Start () {
		isPaused = false;
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
		isPaused = true;
		this.SendMessageUpwards ("onPauseScreen");
	}

	void onResume(){
		hideAllScreens ();
		isPaused = false;
		this.SendMessageUpwards ("onResumeScreen");
	}

	void onExit(){
		this.SendMessageUpwards ("onDisconnect");

		hideAllScreens();
	}
}
