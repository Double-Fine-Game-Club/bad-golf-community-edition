﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class LocalMultiplayerController : MonoBehaviour 
{
	public GameObject ed_singleView;
	public GameObject ed_dualView;
	public GameObject ed_quadView;

	public GameObject ed_singleUI;
	public GameObject ed_dualUI;
	public GameObject ed_quadUI;

	static public GameObject currentView;
	static public GameObject currentUI;

	private int winningPlayer = -1;
	private networkVariables nvs;

	void OnEnable () 
	{
		nvs = GameObject.FindWithTag("NetObj").GetComponent("networkVariables") as networkVariables;
		winningPlayer = -1;

		//deactivat all views
		ed_singleView.SetActive(false);
		ed_dualView.SetActive(false);
		ed_quadView.SetActive(false);
		
		ed_singleUI.SetActive(false);
		ed_dualUI.SetActive(false);
		ed_quadUI.SetActive(false);

		//count up number of players 
		int players = 0;

		if ( LocalMultiplayerLobbyController.keyboardIndex > -1)
		{
			players++;
		}

		players += LocalMultiplayerLobbyController.controllerDeviceIndexToPlayerIndexMap.Count;

		//convert the device map to something usable for this case
		int[] playerToControllerIndex = Enumerable.Repeat(-1, 4).ToArray();
		if ( LobbyControllerSupport.inputDeviceList != null )
		{
			playerToControllerIndex = Enumerable.Repeat(-1, Mathf.Max (4, LobbyControllerSupport.inputDeviceList.Length)).ToArray();
			foreach( KeyValuePair<int,int> kv in LocalMultiplayerLobbyController.controllerDeviceIndexToPlayerIndexMap)
			{
				playerToControllerIndex[kv.Value] = kv.Key;
			}	
		}
		else
		{
			LocalMultiplayerLobbyController.keyboardIndex = 0;
			players = 1;
		}

		//do setup based on number of players & assign the correct devices to the correct prefab
		if ( players == 1 )
		{
			currentView = ed_singleView;
			currentUI = ed_singleUI;

			//Instantiate player objects and connect them to scene
			createPlayer();


			if ( LocalMultiplayerLobbyController.keyboardIndex != -1)
			{
				//Player is using keyboard
				currentView.GetComponent<ControllerSupport>().playerToControllerIndex[0] = -1;
				currentView.SetActive(true);
				currentUI.SetActive(true);
				currentView.GetComponent<ControllerSupport>().ready = true;
			}
			else
			{
				//Player is using gamepad
				int targetDevice = -1;
				foreach( int val in playerToControllerIndex )
				{
					if ( val != -1)	
					{
						targetDevice = val;
					}		
				}				

				currentView.GetComponent<ControllerSupport>().ready = true;
				currentView.GetComponent<ControllerSupport>().playerToControllerIndex[0] = targetDevice; 
				currentView.SetActive(true);
				currentUI.SetActive(true);
			}	
		}
		else if ( players == 2)
		{
			currentView = ed_dualView;
			currentUI = ed_dualUI;

			foreach( int val in playerToControllerIndex )
			{
				if ( val != -1)	
				{
					for (int i = 0; i < currentView.GetComponent<ControllerSupport>().playerToControllerIndex.Length; i++) 
					{
						if( currentView.GetComponent<ControllerSupport>().playerToControllerIndex[i] == -1)
						{
							currentView.GetComponent<ControllerSupport>().playerToControllerIndex[i] = val;
							break;
						}	
					}
				}		
			}

			currentView.GetComponent<ControllerSupport>().ready = true;
			currentView.SetActive(true);
			currentUI.SetActive(true);
			
		}
		else if ( players > 2)
		{
			currentView = ed_quadView;
			currentUI = ed_quadUI;

			foreach( int val in playerToControllerIndex )
			{
				if ( val != -1)	
				{
					for (int i = 0; i < currentView.GetComponent<ControllerSupport>().playerToControllerIndex.Length; i++) 
					{
						if( currentView.GetComponent<ControllerSupport>().playerToControllerIndex[i] == -1)
						{
							currentView.GetComponent<ControllerSupport>().playerToControllerIndex[i] = val;
							break;
						}	
					}
				}		
			}	

			if ( players == 3) //deactivate one of the screens
			{
				int unused = -1;
				for (int i = 0; i < currentView.GetComponent<ControllerSupport>().playerToControllerIndex.Length; i++) 
				{
					if( currentView.GetComponent<ControllerSupport>().playerToControllerIndex[i] == -1 &&  LocalMultiplayerLobbyController.keyboardIndex != i)
					{
						unused = i;
					}
				}
				
				currentView.GetComponent<ControllerSupport>().playerObjectList[unused].GetComponent<CarAudio>().followCamera.farClipPlane = 2;
				currentView.GetComponent<ControllerSupport>().playerObjectList[unused].SetActive( false);
			}

			currentView.GetComponent<ControllerSupport>().ready = true;
			currentView.SetActive(true);
			currentUI.SetActive(true);
		}

		//done setting gamepads above, now setup keyboard correctly, and tell certain components that care what they are controlled by 
		currentView.GetComponent<ControllerSupport>().checkKeyboard();

		//set colors picked from before, only if we went through the lobby
		if ( LobbyControllerSupport.wasInitialized )
		{
			//characters in order of index matching screenview (camera) NOT player  
			Renderer[] bodyList = currentView.GetComponent<ControllerSupport>().playerBodyList;
			
			//for every golfer, find its matching device, then get the player index for that device, apply that color  
			for (int i = 0; i < bodyList.Length; i++) 
			{
				int controllerIndex = currentView.GetComponent<ControllerSupport>().playerToControllerIndex[i];	 
				
				int playerIndex;
				if ( controllerIndex == -1) //this might be a keyboard
				{
					playerIndex = LocalMultiplayerLobbyController.keyboardIndex;
				}
				else
					playerIndex = LocalMultiplayerLobbyController.controllerDeviceIndexToPlayerIndexMap[controllerIndex]; 
				
				//replace all the materials in the object cause we have many materials for the body at the moment
				Material[] matList = bodyList[i].sharedMaterials;
				for (int c = 0; c < matList.Length; c++) 
				{
					matList[c] = LocalMultiplayerLobbyController.playerMats[playerIndex]; 
				}
				bodyList[i].sharedMaterials = matList;
			}		
		}
		//done coloring
	}

	void declareWinner (GameObject player)
	{
		for (int i = 0; i < currentView.GetComponent<ControllerSupport>().playerToControllerIndex.Length; i++) 
		{
			if (  currentView.GetComponent<ControllerSupport>().playerObjectList[i].transform.parent.gameObject == player )
			{
				winningPlayer = i+1;
				break;
			}
		}
	}

	void OnGUI()
	{
		if( winningPlayer > -1)
		{
			GUIStyle myStyle = new GUIStyle();
			myStyle.fontSize = 34;
			myStyle.normal.textColor = Color.red;


			GUI.Label( new Rect( Screen.width/2, Screen.height/2, 200, 200), "Player " + winningPlayer + " is the Winner !",myStyle);
		}
	}

	void Update () 
	{
	
	}

	void createPlayer(){
		GameObject playerContainer = currentView.transform.FindChild("player").gameObject;
		GameObject playerCamera = playerContainer.transform.FindChild("player_camera").gameObject;
		//Create cart for player
		GameObject cartObject = Instantiate(Resources.Load("buggy_m"), new Vector3(0,10,0), Quaternion.identity) as GameObject;
		cartObject.transform.parent = playerContainer.transform;
		//Create ball for player
		GameObject ballObject = Instantiate(Resources.Load("ball"), new Vector3(0,11,0), Quaternion.identity) as GameObject;
		ballObject.transform.parent = playerContainer.transform;
		//Create character for player
		GameObject characterObject = Instantiate(Resources.Load("PatrickOverPatrick"), new Vector3(0,10,0), Quaternion.identity) as GameObject;
		characterObject.transform.parent = cartObject.transform;
		//Create camera for hit_ball; remove later
		GameObject hitBallCam = new GameObject("hit_ball_camera");
		hitBallCam.SetActive(false);
		hitBallCam.transform.parent = ballObject.transform;
		Camera ballCam = hitBallCam.AddComponent<Camera>() as Camera;
		hitBallCam.AddComponent<AudioListener>();
		
		
		//Add scripts to cart
		(cartObject.AddComponent<PlayerRespawn>() as PlayerRespawn).respawnThreshold = -10;
		(cartObject.transform.GetComponent<CarUserControl>() as CarUserControl).enabled = true;
		Destroy(cartObject.transform.GetComponent<NetworkView>());
		
		TransferToSwing ts = cartObject.AddComponent<TransferToSwing>() as TransferToSwing;
		ts.ball = ballObject;
		
		ScriptToggler st = cartObject.AddComponent<ScriptToggler>() as ScriptToggler;
		st.scripts = new List<MonoBehaviour>();
		st.scripts.Add(cartObject.GetComponent<CarController>());
		st.scripts.Add(cartObject.GetComponent<CarUserControl>());
		st.scripts.Add(cartObject.GetComponent<TransferToSwing>());
		st.cameraObject = playerCamera;
		
		//Add scripts to ball
		InControlSwingMode ics = ballObject.AddComponent<InControlSwingMode>() as InControlSwingMode;
		ics.cameraObject = hitBallCam;
		ics.cart = cartObject;
		ics.enabled = false;
		
		PowerMeter pm = ballObject.AddComponent<PowerMeter>() as PowerMeter;
		pm.m_objectToCircle = ballObject;
		pm.m_markerPrefab = Instantiate(Resources.Load("powerMeterPrefab")) as GameObject;
		pm.m_swingScript = ics;
		pm.enabled = false;
		
		ScriptToggler stb = ballObject.AddComponent<ScriptToggler>() as ScriptToggler;
		stb.scripts = new List<MonoBehaviour>();
		stb.scripts.Add(ics);
		stb.scripts.Add(pm);
		stb.cameraObject = hitBallCam;
		
		PlayerRespawn prb = ballObject.AddComponent<PlayerRespawn>() as PlayerRespawn;
		prb.respawnThreshold = -10;
		
		ControllerSupport cs = currentView.GetComponent<ControllerSupport>() as ControllerSupport;
		cs.playerObjectList = new GameObject[1];
		cs.playerObjectList[0] = cartObject;
		cs.playerBodyList = new Renderer[1];
		cs.playerBodyList[0] = characterObject.transform.FindChild("body").GetComponent<SkinnedMeshRenderer>();
		cs.playerToControllerIndex = new int[1];
		cs.playerToControllerIndex[0] = -1;
		
		(playerCamera.GetComponent<FollowPlayerScript>() as FollowPlayerScript).target = cartObject.transform;
	}


	//LocalMultiplayerLobbyController.controllerDeviceIndexToPlayerIndexMap.Values.CopyTo( playerToControllerIndex, 0) ;	
	//Array.Sort<int>( playerToControllerIndex, new Comparison<int>( (a,b) => a.CompareTo(b) ));
}