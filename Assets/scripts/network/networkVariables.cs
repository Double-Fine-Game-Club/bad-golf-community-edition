using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// general variables
public class networkVariables : MonoBehaviour {
	[HideInInspector]
	public string[] buggyModels;		// buggy models
	[HideInInspector]
	public string[] buggyModelNames;	// buggy models names
	[HideInInspector]
	public string[] ballModels;	// ball models
	[HideInInspector]
	public string[] ballModelNames;	// ball models names
	[HideInInspector]
	public string[] characterModels;	// character models
	[HideInInspector]
	public string[] characterModelNames;	// character models names
	[HideInInspector]
	public string[] levelNames;	// level names

	// ADD VARIABLES HERE - after adding you will need to reset the object in the inspector and re-assign everything
	[HideInInspector]
	public Camera myCam;				// camera
	[HideInInspector]
	public float lowestHeight = 50;			// lowest height before respawn
	[HideInInspector]
	public bool playerHasWon = false;	// indicate if a player has won the current game
	[HideInInspector]
	public string winningPlayer;		// Winner of the last game played
	// client only variables
	// maybe put the pause things in here?
	
	// server only variables
	[HideInInspector]
	public bool gameHasBegun = false;

	// techincal bits
	[HideInInspector]
	public PlayerInfo myInfo = new PlayerInfo();	// stores info on the current player
	[HideInInspector]
	public string serverVersion = "DEBUG";			// server version
	[HideInInspector]
	public string serverName = "";					// server name
	[HideInInspector]
	public ArrayList players = new ArrayList();		// list of players
	[HideInInspector]
	public int NATmode = -1;						// which NAT version we have for server comparison
}

// a class that contains all information on a player
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
	public string color;						// player color
	public int score = 0;						// player score
	
	// do these need to be net-sunk?
	public bool playerIsBusy   = false;			// player is engaged in an uninteruptable action
	public bool playerIsPaused = false;			// player is paused

	public CarController carController;		//The movement script for this player's buggy
	public float v;		//player accelleration/brake input
	public float h;		//player steering input
}
// server comment system
public class ServerComment {
	public int NATmode;
	public string comment;
	public string level;
	
	public string toString() {
		string tmp = "";
		tmp = tmp + NATmode.ToString() + ";";
		tmp = tmp + level + ";";
		tmp = tmp + comment + ";";
		return tmp;
	}
	public ServerComment(string str) {
		string[] tmp = str.Split(new string[]{";"},System.StringSplitOptions.None);
		NATmode = int.Parse(tmp[0]);
		level = tmp[1];
		comment = tmp[2];
	}
	public ServerComment() {
		NATmode = 0;
		comment = "";
		level = "";
	}
}