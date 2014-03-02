using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//maybe change to cover all minimap stuff?
public class mapIndicatorScript : MonoBehaviour {

	networkVariables nvs;
	int numPlayers;
	private List<PlayerInfo> handledPlayers;

	// Use this for initialization
	void Start () {
		nvs = GetComponent ("networkVariables") as networkVariables;
		handledPlayers = new List<PlayerInfo> ();
		numPlayers = 0;


	}
	
	// Update is called once per frame
	void Update () {
		if (numPlayers < nvs.players.Count) {
			foreach(PlayerInfo p in nvs.players){	
				if(p!=null && !handledPlayers.Contains(p)){
					GameObject indicator = GameObject.Instantiate(Resources.Load ("Map_Indicator")) as GameObject;
					indicator.transform.parent = p.cartGameObject.transform;
					indicator.transform.position = p.cartGameObject.transform.position + new Vector3(0f,60f,0f);
					(indicator.GetComponent("FollowPositionScript") as FollowPositionScript).target = p.cartGameObject.transform;
					handledPlayers.Add(p);
					numPlayers++;
				}
			}

		}
	}
	
}
