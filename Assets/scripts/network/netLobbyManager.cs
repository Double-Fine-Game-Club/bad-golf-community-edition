using UnityEngine;
using System.Collections;

public class netLobbyManager : MonoBehaviour {

	networkVariables nvs;
	PlayerInfo myInfo;
	ArrayList scenes = new ArrayList();
	GameObject lobbyView;

	// Use this for initialization
	void Start () {
		nvs = GetComponent ("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		lobbyView = GameObject.Find ("lobby_view");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//Sent from networkManagerServer when enough players for a game join
	//Server only
	void onLobbyFull(){
		string levelName = "multi_level_01";
		ArrayList spawnPoints = new ArrayList ();
		spawnPoints.Add (new Vector3 (-18.07f, 8.30f, 140f));
		spawnPoints.Add (new Vector3 (-18.07f, 12.30f, 115f));
		spawnPoints.Add (new Vector3 (-18.07f, 12.30f, 120f));
		spawnPoints.Add (new Vector3 (-18.07f, 12.30f, 125f));

		networkView.RPC ("loadLevel", RPCMode.All, levelName);

		for(int i=0; i<nvs.players.Count; i++){
			PlayerInfo player = nvs.players[i] as PlayerInfo;
			player.KBState = 0;	//Not moving
			player.cartGameObject.transform.position = (Vector3)spawnPoints[i];
		}

	}

	void onGui(){

	}

	[RPC]
	void loadLevel(string levelName){
		Application.LoadLevelAdditive (levelName);
		lobbyView.SetActive (false);
		GameObject obj = GameObject.Find (levelName);
		myInfo.cartGameObject.transform.FindChild ("buggy_cam").gameObject.SetActive (true);
	}
}

