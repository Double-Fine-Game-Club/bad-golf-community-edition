using UnityEngine;
using System.Collections;

public class netLobbyManager : MonoBehaviour {

	networkVariables nvs;
	PlayerInfo myInfo;
	ArrayList scenes = new ArrayList();
	GameObject lobbyView;
	bool isStartActive = false, alreadyLoaded=false;		

	// Use this for initialization
	void Start () {
		nvs = GetComponent ("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//Sent from networkManagerServer when enough players for a game join
	//Server only
	void onLobbyFull(){
		isStartActive = true;
	}

	void onStart(){
		if (!isStartActive || alreadyLoaded) {
				return;
		}
		alreadyLoaded = true;

		string levelName = "multi_level_01";
		ArrayList spawnPoints = new ArrayList ();
		spawnPoints.Add (new Vector3 (-18.07f, 12.30f, 140f));
		spawnPoints.Add (new Vector3 (-18.07f, 12.30f, 115f));
		spawnPoints.Add (new Vector3 (-18.07f, 12.30f, 120f));
		spawnPoints.Add (new Vector3 (-18.07f, 12.30f, 125f));
		
		ArrayList ballPoints = new ArrayList ();
		ballPoints.Add (new Vector3 (-18.07f, 20.30f, 140f));
		ballPoints.Add (new Vector3 (-18.07f, 20.30f, 115f));
		ballPoints.Add (new Vector3 (-18.07f, 20.30f, 120f));
		ballPoints.Add (new Vector3 (-18.07f, 20.30f, 125f));
		
		networkView.RPC ("loadLevel", RPCMode.All, levelName);
		
		for(int i=0; i<nvs.players.Count; i++){
			PlayerInfo player = nvs.players[i] as PlayerInfo;
			//player.cartGameObject.transform.position = (Vector3)spawnPoints[i];
			player.cartContainerObject.transform.position = (Vector3)spawnPoints[i];
			player.ballGameObject.transform.position = (Vector3)ballPoints[i];
		}
	}

	void onGui(){

	}

	[RPC]
	void loadLevel(string levelName){
		Application.LoadLevelAdditive (levelName);
		lobbyView = GameObject.Find ("lobby_view");
		lobbyView.SetActive (false);
		myInfo.cartContainerObject.transform.FindChild ("multi_buggy_cam").gameObject.SetActive (true);
	}
}

