using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class LocalMultiplayerController : MonoBehaviour 
{
	static public GameObject currentView;

	private int winningPlayer = -1;
	private networkVariables nvs;

	void OnEnable () 
	{
		nvs = GameObject.FindWithTag("NetObj").GetComponent("networkVariables") as networkVariables;
		winningPlayer = -1;

		currentView = new GameObject ("current_view");
		currentView.transform.parent = GameObject.Find ("level_full").transform;
		currentView.AddComponent<ControllerSupport> ();
		currentView.tag = "LocalMultiplayerView";

		//count up number of players 
		int players = 0;

		if ( LocalMultiplayerLobbyController.keyboardIndex > -1)
		{
			players++;
		}

		players += LocalMultiplayerLobbyController.controllerDeviceIndexToPlayerIndexMap.Count;

		//This should be moved to after character selection is complete
		createPlayers();

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

			if ( LocalMultiplayerLobbyController.keyboardIndex != -1)
			{
				//Player is using keyboard
				currentView.GetComponent<ControllerSupport>().playerToControllerIndex[0] = -1;
				currentView.SetActive(true);
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
			}	
		}
		else if ( players == 2)
		{
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

		}
		else if ( players > 2)
		{

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
		}

		//done setting gamepads above, now setup keyboard correctly, and tell certain components that care what they are controlled by 
		currentView.GetComponent<ControllerSupport>().checkKeyboard();
		
		//done coloring
		GameObject.Find (nvs.levelName).AddComponent<netPlayerRespawn> ();
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

	void createPlayers(){
		int numPlayers = nvs.players.Count;
		ControllerSupport cs = currentView.GetComponent<ControllerSupport>() as ControllerSupport;
		cs.playerObjectList = new GameObject[numPlayers];
		cs.playerBodyList = new Renderer[numPlayers];
		cs.playerToControllerIndex = new int[numPlayers];

		GameObject[] playerCams = CameraManager.createSplitScreenCameras (numPlayers);
		GameObject[] ballCams   = CameraManager.createSplitScreenCameras (numPlayers);
		
		for(int i=0; i<numPlayers; ++i){
			PlayerInfo player = nvs.players[i] as PlayerInfo;

			GameObject playerContainer = new GameObject(player.name);
			playerContainer.transform.parent = currentView.transform;
			playerContainer.AddComponent<LocalBallMarker> ();
			GameObject playerCamera = playerCams[i];
			playerCamera.transform.parent = playerContainer.transform;
			player.cameraObject = playerCamera;

			//Create cart for player
			GameObject cartObject = Instantiate(Resources.Load(player.cartModel), new Vector3(0,-100,0), Quaternion.identity) as GameObject;
			cartObject.name = "buggy";
			cartObject.transform.parent = playerContainer.transform;
			player.cartGameObject = cartObject;
			//Create ball for player
			GameObject ballObject = Instantiate(Resources.Load(player.ballModel), new Vector3(0,-100,0), Quaternion.identity) as GameObject;
			ballObject.name = "hit_mode_ball";
			ballObject.transform.parent = playerContainer.transform;
			player.ballGameObject = ballObject;
			//Create character for player
			GameObject characterObject = Instantiate(Resources.Load(player.characterModel)) as GameObject;
			characterObject.name = "player_character";
			characterObject.transform.parent = cartObject.transform;
			characterObject.transform.localPosition = Vector3.zero + new Vector3(0,0.3f,0);
			characterObject.transform.localRotation = Quaternion.identity;
			player.characterGameObject = characterObject;
			//Apply color
			RecolorPlayer.recolorPlayerBody( characterObject.transform.FindChild("body").GetComponent<Renderer>() as Renderer, player.color );
			if(i<1)	//Only one audiolistener can exist
				characterObject.AddComponent<AudioListener> ();
			//Create camera for hit_ball; remove later
			GameObject hitBallCam = ballCams[i];
			hitBallCam.name = "hit_ball_camera";
			hitBallCam.SetActive(false);
			hitBallCam.transform.parent = ballObject.transform;

			//Add scripts to cart
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
			

			//controller support
			cs.playerObjectList[i] = cartObject;
			cs.playerBodyList[i] = characterObject.transform.FindChild("body").GetComponent<SkinnedMeshRenderer>();
			cs.playerToControllerIndex[i] = -1;	//dummy value

			FollowPlayerScript fps = (playerCamera.AddComponent<FollowPlayerScript> () as FollowPlayerScript);
			fps.target = cartObject.transform;

		}
		(GameObject.Find ("winningPole").gameObject.GetComponent<netWinCollider> () as netWinCollider).initialize();
		nvs.myInfo = nvs.players [0] as PlayerInfo;	//just so some of the network scripts work
		nvs.gameObject.AddComponent<netPause> ();
	}


	//LocalMultiplayerLobbyController.controllerDeviceIndexToPlayerIndexMap.Values.CopyTo( playerToControllerIndex, 0) ;	
	//Array.Sort<int>( playerToControllerIndex, new Comparison<int>( (a,b) => a.CompareTo(b) ));
}