using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerNames : MonoBehaviour {

	networkVariables nvs;
	int numPlayers;
	Font textFont;
	private List<PlayerInfo> handledPlayers;

	// Use this for initialization
	void Start () {
		nvs = GetComponent ("networkVariables") as networkVariables;
		handledPlayers = new List<PlayerInfo> ();
		numPlayers = 1;
	}
	
	// Update is called once per frame
	void Update () {
		if (numPlayers < nvs.players.Count) {
			foreach(PlayerInfo p in nvs.players){
				if(!handledPlayers.Contains(p) && p!=nvs.myInfo){
					GameObject newText = Instantiate(Resources.Load ("name_over_cart")) as GameObject;
					newText.GetComponent<TextMesh>().text = p.name;
					newText.transform.parent = p.cartGameObject.transform;
					newText.transform.position = p.cartGameObject.transform.position + new Vector3(0f,3.5f,0f);
					handledPlayers.Add(p);
					numPlayers++;
				}
			}

		}
	}
	
}
