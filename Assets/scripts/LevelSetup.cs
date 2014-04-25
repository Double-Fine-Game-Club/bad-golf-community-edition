using UnityEngine;
using System.Collections;

public class LevelSetup : MonoBehaviour {

	void Awake(){
		networkVariables nvs = GameObject.FindWithTag("NetObj").GetComponent("networkVariables") as networkVariables;
		switch(nvs.gameMode){
		case GameMode.Local:
			GameObject.Find (nvs.levelName).AddComponent<LocalMultiplayerController>();
			break;
		case GameMode.Online:
			//Add Network level startup script here
			break;
		default:
			break;
		}
		Destroy (this);
	}
}
