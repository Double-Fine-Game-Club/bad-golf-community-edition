using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkVariables : MonoBehaviour {
	// ADD VARIABLES HERE
	public PlayerInfo myInfo = new PlayerInfo();	// stores info on the current player
	public string serverVersion = "HL5";			// server version

	// client only variables

	// server only variables
	public ArrayList players;
}


public class PlayerInfo {
	public NetworkPlayer player;			// player
	public NetworkViewID cartViewID;		// NetworkViewID of the cart
	public GameObject cartGameObject;		// GameObject of the cart
	public string cartModel;				// model of the cart
	public NetworkViewID ballViewID;		// NetworkViewID of the ball
	public GameObject ballGameObject;		// GameObject of the ball
	public string ballModel;				// model of the ball
	public NetworkViewID characterViewID;	// NetworkViewID of the character
	public GameObject characterGameObject;	// GameObject of the character
	public string characterModel;			// model of the character
	public int currentMode;					// current mode of the player (0=in buggy, 1=on foot)

	public int KBState;						// KB state for server only
}