using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkVariables : MonoBehaviour {
	// ADD VARIABLES HERE
	public NetworkViewID myViewID;	// the players NetworkViewID
	public GameObject myCart;		// the players cart

	// client only variables

	// server only variables
	public Dictionary<NetworkPlayer,NetworkViewID> playersCartViewID;
	public Dictionary<NetworkPlayer,GameObject> playerGameObjects;
	public Dictionary<NetworkPlayer,NetworkViewID> randomBalls;
	public Dictionary<NetworkPlayer,NetworkViewID> spawnedPlayers;
}
