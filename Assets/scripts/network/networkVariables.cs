using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class networkVariables : MonoBehaviour {
	// ADD VARIABLES HERE - after adding you will need to reset the object in the inspector and re-assign everything
	public Camera myCam;				// camera
	public float lowestHeight;			// lowest height before respawn
	[HideInInspector]
	public string[] buggyModels = new string[2] {"buggy_m", "hotrod_m"};		// buggy models
	[HideInInspector]
	public string[] buggyModelNames = new string[2] {"Buggy", "Hotrod"};	// buggy models names
	[HideInInspector]
	public string[] ballModels = new string[1] {"ball"};	// ball models
	[HideInInspector]
	public string[] ballModelNames = new string[1] {"Ball"};	// ball models names
	[HideInInspector]
	public string[] characterModels = new string[2] {"lil_patrick", "BradOverPatrick"};	// character models
	[HideInInspector]
	public string[] characteryModelNames = new string[2] {"Patrick", "Brad"};	// character models names
	
	// client only variables
	// maybe put the pause things in here?
	
	// server only variables
	[HideInInspector]
	public bool gameHasBegun = false;

	// techincal bits
	[HideInInspector]
	public PlayerInfo myInfo = new PlayerInfo();	// stores info on the current player
	[HideInInspector]
	public string serverVersion = "sandvich4";		// server version
	[HideInInspector]
	public string serverName = "";					// server name
	[HideInInspector]
	public ArrayList players = new ArrayList();		// list of players
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
	public int currentMode = 2;					// current mode of the player (0=in buggy, 1=on foot, 2=spectator)
	public string name;							// name
	public int score = 0;						// player score
	
	// do these need to be net-sunk?
	public bool playerIsBusy   = false;			// player is engaged in an uninteruptable action
	public bool playerIsPaused = false;			// player is paused
	
	public float v;		//player accelleration/brake input
	public float h;		//player steering input
}