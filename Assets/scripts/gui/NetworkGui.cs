using UnityEngine;
using System.Collections;

public class NetworkGUI : MonoBehaviour {
  	
    PlayerInfo myInfo;
    networkVariables nvs;
    bool debugGUIState = false;
    string playerIP;	// player
    string serverIP;	// server
    string cartViewIDTransformValue;	// NetworkViewID of the cart transform
    string cartViewIDRigidbodyValue;	// NetworkViewID of the cart rigidbody
    string cartModel;	// model of the cart
    string ballViewIDValue;	// NetworkViewID of the ball
    string ballModel;	// model of the ball
    string characterViewIDValue;	// NetworkViewID of the character
    string characterModel;	// model of the character
    string currentModeValue;	// current mode of the player (0=in buggy, 1=on foot)
	string playerName;	// name
    
    void Start() {
        // get variables we need
        nvs = GetComponent("networkVariables") as networkVariables;
        myInfo = nvs.myInfo;
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.N))
        { 
			debugGUIState = !debugGUIState;
            
        }
        
        if( debugGUIState)
        {
            playerIP = "Player IP Address : " + myInfo.player.ipAddress;
            serverIP = "Server IP Address : " + myInfo.server.ipAddress;
            cartViewIDTransformValue = "Cart Transform Info : " + myInfo.cartViewIDTransform.ToString();
            cartViewIDRigidbodyValue = "Cart RigidBody Info : " + myInfo.cartViewIDRigidbody.ToString();
            cartModel = "Cart Model Name : " + myInfo.cartModel;
            ballViewIDValue = "Ball Model Info : " + myInfo.ballViewID.ToString();
            ballModel = "Ball Model Name : " + myInfo.ballModel;
            characterViewIDValue = "Caracter Model Info : " + myInfo.characterViewID.ToString();
            currentModeValue = "Current Player Mode : ";
            if ( myInfo.currentMode == 0)
            {
				currentModeValue += "Inside Buggy";
            }
            else
            {
				currentModeValue += "On Foot";
            }
            
			playerName = "Player's Name : " + myInfo.name;
            
        }
    }
    
    void OnGUI()
    {
      if(debugGUIState)
      {
        GUILayout.BeginVertical("Network Debug");
		GUILayout.Label(playerName);
		GUILayout.Label(playerIP);
		GUILayout.Label(serverIP);
		GUILayout.Label(cartViewIDTransformValue);
		GUILayout.Label(cartViewIDRigidbodyValue);
		GUILayout.Label(cartModel);
		GUILayout.Label(ballViewIDValue);
		GUILayout.Label(ballModel);
		GUILayout.Label(characterViewIDValue);
		GUILayout.Label(currentModeValue);
        GUILayout.EndVertical();
      }
    }
}
