using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour {

	public GameObject ed_pauseScreen;

	private string nameOfLevel;
	private bool isPaused=false;
	// Use this for initialization
	void Start () {
		onResume ();
	}
	
	// Update is called once per frame
	void Update () {
		if(!isPaused && Input.GetKey(KeyCode.Escape)){
			onPauseScreen();
		}
	}

	void hideAllScreens(){
		
		if ( LocalMultiplayerController.currentView != null )
		{
			LocalMultiplayerController.currentView.SetActive (false);
		}
		ed_pauseScreen.SetActive (false);
	}

	void onPauseScreen(){
		hideAllScreens ();
		ed_pauseScreen.SetActive (true);
		Time.timeScale = 0;
		isPaused = true;
	}

	void onResume(){
		hideAllScreens ();
		if ( LocalMultiplayerController.currentView != null )
		{
			LocalMultiplayerController.currentView.SetActive (true);
		}
		Time.timeScale = 1;
		isPaused = false;

	}	

	void onExit(){
		//Go back to main menu
		nameOfLevel = "main";
		Application.LoadLevel( nameOfLevel );
		Time.timeScale = 1;
		hideAllScreens();
	}
}
