using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkVariables : MonoBehaviour {
	// ADD VARIABLES HERE
	public PlayerInfo myInfo = new PlayerInfo();	// stores info on the current player
	public string serverVersion = "HL5";			// server version

	// client only variables

	// server only variables
	public ArrayList players = new ArrayList();	// maybe include this in client for extrapolation of other players buggys - done
}


public class PlayerInfo {
	public NetworkPlayer player;				// player
	public NetworkPlayer server;				// server
	public NetworkViewID cartViewIDTransform;	// NetworkViewID of the cart transform
	public NetworkViewID cartViewIDRigidbody;	// NetworkViewID of the cart rigidbody
	public GameObject cartGameObject;			// GameObject of the cart
	public string cartModel;					// model of the cart
	public NetworkViewID ballViewID;			// NetworkViewID of the ball
	public GameObject ballGameObject;			// GameObject of the ball
	public string ballModel;					// model of the ball
	public NetworkViewID characterViewID;		// NetworkViewID of the character
	public GameObject characterGameObject;		// GameObject of the character
	public string characterModel;				// model of the character
	public int currentMode;						// current mode of the player (0=in buggy, 1=on foot)
	public string name;							// name

	public int KBState;						// KB state for server only
}