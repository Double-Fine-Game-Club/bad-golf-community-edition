using UnityEngine;
using System.Collections;
using System;
using InControl;
using System.Collections.Generic;

public class LocalMultiplayerLobbyController : MonoBehaviour 
{
	public Camera[] ed_playerViewCameras;
	public SwitchableTexture[] ed_controlSelectors;
	public Renderer[] ed_playerRenderer;
	static public Material[] playerMats;
	public GameObject[] ed_joinButtonTexts;
	public GameObject[] ed_detailControls;

	public string[] colorKeys;
	public int[] colorPerPlayer;
	public int[] characterPerPlayer;
	public int[] cartPerPlayer;

	public GameObject startMessageTarget;

	static public Dictionary<int,int> controllerDeviceIndexToPlayerIndexMap = new Dictionary<int,int>();
	static public int keyboardIndex = -1;

	public GameObject[] controllerHighlights;
	public networkVariables nvs;

	void Start()
	{

		colorPerPlayer = new int[] { 0,1,2,3 };
		cartPerPlayer  = new int[] {0,0,0,0};
		characterPerPlayer = new int[] {0,0,0,0};

		playerMats = new Material[ ed_playerRenderer.Length ];
	
		colorKeys = new string[Config.colorsDictionary.Count ];
		Config.colorsDictionary.Keys.CopyTo( colorKeys,0);

		//start with no guys
		for (int i = 0; i < ed_playerViewCameras.Length; i++) 
		{
			ed_playerViewCameras[i].farClipPlane = 2;
			ed_playerViewCameras[i].clearFlags = CameraClearFlags.SolidColor;

			ed_controlSelectors[i].setIndex(0);

			controllerHighlights[i].SetActive(false);
		}
		
		//make sure all players have their colors according to colorPerPlayer
		for (int i = 0; i < ed_playerViewCameras.Length; i++) 
		{
			setColorOn( i );
		}
	}

	private void setColorOn( int index)
	{
		string color = colorKeys [colorPerPlayer [index]];

		PlayerInfo p = nvs.getPlayer (index);
		if(p!=null) p.color = color;

		RecolorPlayer.recolorPlayerBody (ed_playerRenderer [index], color);
	}

	private void setCharacterOn( int index )
	{
		PlayerInfo p = nvs.getPlayer (index);
		p.characterModel = nvs.characterModels[characterPerPlayer [index] ];

		//Switch player character
		GameObject oldCharacter = ed_playerRenderer[index].transform.parent.gameObject;
		GameObject newCharacter = Instantiate (Resources.Load( p.characterModel )) as GameObject;
		Destroy (newCharacter.GetComponent<Animation> ());	//Use a standing pose
		newCharacter.transform.parent = oldCharacter.transform.parent;
		newCharacter.transform.position = oldCharacter.transform.position;
		newCharacter.transform.rotation = oldCharacter.transform.rotation;
		newCharacter.transform.localScale = oldCharacter.transform.localScale;
		newCharacter.name = "player_character";
		ed_playerRenderer [index] = newCharacter.transform.FindChild ("body").GetComponent<Renderer> () as Renderer;
		Destroy(oldCharacter);

		//Apply color
		setColorOn(index);
	}

	private void setCartOn( int index )
	{
		PlayerInfo p = nvs.getPlayer (index);
		p.cartModel = nvs.buggyModels [cartPerPlayer [index]];

		//Switch cart
		GameObject oldCart = ed_playerViewCameras [index].transform.parent.FindChild ("player_cart").gameObject;
		GameObject newCart = Instantiate (Resources.Load (p.cartModel)) as GameObject;

		//Don't want the full cart, only the model
		GameObject cartParent = newCart;
		if(p.cartModel=="hotrod_m")
		{
			newCart = cartParent.transform.FindChild("hotrod").gameObject;

		}else if(p.cartModel=="buggy_m")
		{
			newCart = cartParent.transform.FindChild("cart3_yup").gameObject;
		}

		newCart.transform.parent = oldCart.transform.parent;
		newCart.transform.position = oldCart.transform.position;
		newCart.transform.rotation = oldCart.transform.rotation;

		newCart.name = "player_cart";
		Destroy (cartParent);
		Destroy (oldCart);
	}

	public void onControl( SwitchableTexture callingSwitch )
	{
		int playerIndex = Array.IndexOf( ed_controlSelectors, callingSwitch );
		
		int type = ed_controlSelectors[playerIndex].index ;

		if ( type == 0 ) //off
		{
			//turn off this guy
			ed_playerViewCameras[playerIndex].farClipPlane = 2;
			ed_playerViewCameras[playerIndex].clearFlags = CameraClearFlags.SolidColor;
			ed_joinButtonTexts[playerIndex].SetActive(true);
			ed_detailControls[playerIndex].SetActive(false);
			controllerHighlights[playerIndex].SetActive(false);

			if ( playerIndex == keyboardIndex)
				keyboardIndex = -1;

			//if this is a controller turning off then remove from map
			if ( controllerDeviceIndexToPlayerIndexMap.ContainsValue(playerIndex) )
			{	
				foreach( KeyValuePair<int,int> kv in controllerDeviceIndexToPlayerIndexMap)
				{
					if ( kv.Value == playerIndex)
					{
						controllerDeviceIndexToPlayerIndexMap.Remove( kv.Key);
						break;
					}
				}
			}

			//remove player from active players
			PlayerInfo p = nvs.getPlayer(playerIndex);
			nvs.players.Remove(p);

		}
		else if ( type == 1 ) //joystick
		{
			ed_playerViewCameras[playerIndex].farClipPlane = 1000;
			ed_playerViewCameras[playerIndex].clearFlags = CameraClearFlags.Skybox;
			ed_joinButtonTexts[playerIndex].SetActive(false);
			ed_detailControls[playerIndex].SetActive(true);
		}
		else if ( type == 2) //keyboard
		{
			//if this is a controller skip keyboard
			if ( controllerDeviceIndexToPlayerIndexMap.ContainsValue(playerIndex) )
			{	
				foreach( KeyValuePair<int,int> kv in controllerDeviceIndexToPlayerIndexMap)
				{
					if ( kv.Value == playerIndex)
					{
						ed_controlSelectors[playerIndex].setIndex(0); 
						break;
					}
				}
			}
			else //set keyboard
			{
				ed_playerViewCameras[playerIndex].farClipPlane = 1000;
				ed_playerViewCameras[playerIndex].clearFlags = CameraClearFlags.Skybox;
				ed_joinButtonTexts[playerIndex].SetActive(false);
				ed_detailControls[playerIndex].SetActive(true);
				keyboardIndex = playerIndex;

				controllerHighlights[playerIndex].GetComponent<ControllerHighlightManager>().sendHoverRemoveMessage();
				
				//if this was a controller remove it
				if ( controllerDeviceIndexToPlayerIndexMap.ContainsValue(playerIndex) )
				{	
					foreach( KeyValuePair<int,int> kv in controllerDeviceIndexToPlayerIndexMap)
					{
						if ( kv.Value == playerIndex)
						{
							controllerDeviceIndexToPlayerIndexMap.Remove( kv.Key );
							controllerHighlights[playerIndex].SetActive(false);
							break;
						}
					}
				}

				//check if keyboard was somewhere else, change it to blank
				foreach( SwitchableTexture switchable in ed_controlSelectors)
				{
					if ( callingSwitch != switchable && switchable.index == 2) //if keyboard
					{
						switchable.setIndex(0);
					}
				}
			}
		}
	}

	//Change the color of the given player
	//example val  =  "2_r" 
	//'2' for player 2, 'r' for right arrow
	public void onColor ( string val )
	{
		string[] split = val.Split( new char[]{'_'});		
		int playerIndex =  int.Parse(split[0]);
		int direction = (split[1] == "r")? 1:-1;
		colorPerPlayer[playerIndex] =  (colorPerPlayer[playerIndex] + colorKeys.Length + direction) % colorKeys.Length; 
		setColorOn (playerIndex);
	}

	//Change the character model of the given player
	//example val  =  "2_r" 
	//'2' for player 2, 'r' for right arrow
	public void onCharacter ( string val )
	{
		string[] split = val.Split( new char[]{'_'});			
		int playerIndex =  int.Parse(split[0]);
		int direction = (split[1] == "r")? 1:-1;

		characterPerPlayer [playerIndex] = (characterPerPlayer [playerIndex] + nvs.characterModels.Length + direction) % nvs.characterModels.Length;
		setCharacterOn (playerIndex);
	}

	//Change the cart model of the given player
	//example val  =  "2_r" 
	//'2' for player 2, 'r' for right arrow
	public void onCart ( string val )
	{
		string[] split = val.Split( new char[]{'_'});		
		int playerIndex =  int.Parse(split[0]);
		int direction = (split[1] == "r")? 1:-1;

		cartPerPlayer [playerIndex] = (cartPerPlayer [playerIndex] + nvs.buggyModels.Length + direction) % nvs.buggyModels.Length;
		setCartOn (playerIndex);
	}

	public void onStartClicked( string nameOfLevel )
	{
		nvs.gameMode = GameMode.Local;
		int players = 0;
		foreach ( SwitchableTexture switchable in ed_controlSelectors)
		{
			if( switchable.index > 0 )
				players += 1; 
		}
			
		if (players > 0 )
		{
			//TODO: instantiate characters here
			nvs.levelName = nameOfLevel;
			startMessageTarget.SendMessage ( "onStartClicked", nameOfLevel);
		}
	}

	public void onJoin( string val)
	{
		int index = int.Parse(val);
		ed_controlSelectors[index].setIndex(2);

		PlayerInfo newP = new PlayerInfo ();
		newP.playerId = index;
		newP.color = colorKeys [colorPerPlayer [index]];
		newP.cartModel = nvs.buggyModels[0];
		newP.characterModel = nvs.characterModels [0];
		newP.ballModel = nvs.ballModels [0];
		newP.name = "player" + (index+1).ToString();
		nvs.players.Add (newP);
	}

	public void onControlDirection ( int targetDevice)
	{
		if (  controllerDeviceIndexToPlayerIndexMap.ContainsKey( targetDevice ) )
		{
			int playerIndex = controllerDeviceIndexToPlayerIndexMap[targetDevice];

			//handle controller highlight or action here
			if ( LobbyControllerSupport.inputDeviceList[targetDevice].Direction.x > 0)
			{
				//right
				controllerHighlights[playerIndex].SendMessage("doRight", SendMessageOptions.DontRequireReceiver);
			}
			else if (LobbyControllerSupport.inputDeviceList[targetDevice].Direction.x < 0 )
			{
				//left
				controllerHighlights[playerIndex].SendMessage("doLeft", SendMessageOptions.DontRequireReceiver);
			}
			
			if ( LobbyControllerSupport.inputDeviceList[targetDevice].Direction.y < 0 )
			{
				//down
				controllerHighlights[playerIndex].SendMessage("doDown", SendMessageOptions.DontRequireReceiver);
			}
			else if ( LobbyControllerSupport.inputDeviceList[targetDevice].Direction.y > 0 )
			{
				//up
				controllerHighlights[playerIndex].SendMessage("doUp", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void onControlAnyButtonPress( int targetDevice)
	{
		if (  controllerDeviceIndexToPlayerIndexMap.ContainsKey( targetDevice ) )
		{
			int playerIndex = controllerDeviceIndexToPlayerIndexMap[targetDevice];
			controllerHighlights[playerIndex].SendMessage("doButtonPress");
		}
		else
		{
			//add a new controller player if any spots left 
			int openIndex = -1;
			for (int i = 0; i < ed_controlSelectors.Length; i++) 
			{
				if ( ed_controlSelectors[i].index == 0 )
				{
					openIndex = i;
					break;
				}
			}
				
			if ( openIndex > -1)
			{
				controllerDeviceIndexToPlayerIndexMap[targetDevice] = openIndex;
				ed_controlSelectors[openIndex].setIndex(1);
				controllerHighlights[openIndex].SetActive(true);

				PlayerInfo newP = new PlayerInfo ();
				newP.playerId = openIndex;
				newP.color = colorKeys [colorPerPlayer [openIndex]];
				newP.cartModel = nvs.buggyModels[0];
				newP.characterModel = nvs.characterModels [0];
				newP.ballModel = nvs.ballModels [0];
				newP.name = "player" + (openIndex+1).ToString();
				nvs.players.Add (newP);
			}	
			else
			{
				Debug.Log ( "Controller doesnt fit. All positions are full!");
			}	
		}
	}
}
