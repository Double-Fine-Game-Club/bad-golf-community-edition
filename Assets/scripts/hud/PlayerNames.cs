using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerNames : MonoBehaviour {

	networkVariables nvs;
	int numPlayers;
	Font textFont;
	private Dictionary<PlayerInfo, GameObject> playerNames;
	private Camera myCam;

	// Use this for initialization
	void Start () {
		nvs = GetComponent ("networkVariables") as networkVariables;
		playerNames = new Dictionary<PlayerInfo, GameObject> ();
		myCam = nvs.myCam;
		numPlayers = 1;
	}
	
	// Update is called once per frame
	void Update () {
		if (numPlayers < nvs.players.Count) {
			foreach(PlayerInfo p in nvs.players){
				if(!playerNames.ContainsKey(p) && p!=nvs.myInfo){
					GameObject newText = Instantiate(Resources.Load ("name_over_cart")) as GameObject;
					newText.GetComponent<TextMesh>().text = p.name;
					newText.transform.parent = p.cartGameObject.transform;
					newText.transform.position = p.cartGameObject.transform.position + new Vector3(0f,3.5f,0f);
					playerNames.Add(p, newText);
					numPlayers++;
				}
			}

		}

		UpdatePositions ();
	}

	void UpdatePositions()
	{
		for (int i = 0; i < nvs.players.Count; i++) {
			PlayerInfo player = (PlayerInfo)nvs.players[i];
			if (player != null) {
				if (playerNames.ContainsKey(player)) {
					GameObject playerName = playerNames[player];
					playerName.transform.rotation = myCam.transform.rotation; //billboard name towards the camera
				}
			}
		}
	}
	
}
