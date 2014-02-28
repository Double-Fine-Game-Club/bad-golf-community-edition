using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkVariables : MonoBehaviour {
	// ADD VARIABLES HERE
	public PlayerInfo myInfo = new PlayerInfo();	// stores info on the current player
	[HideInInspector]
	public string serverVersion = "sandvich2";		// server version
	public string serverName = "";					// server name
	public ArrayList players = new ArrayList();		// list of players
	public Camera myCam;							// camera

	// client only variables
	// maybe put the pause things in here?

	// server only variables
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

	// do these need to be net-sunk?
	public bool playerIsBusy   = false;			// player is engaged in an uninteruptable action
	public bool playerIsPaused = false;			// player is paused

	public float v;		//player accelleration/brake input
	public float h;		//player steering input
}