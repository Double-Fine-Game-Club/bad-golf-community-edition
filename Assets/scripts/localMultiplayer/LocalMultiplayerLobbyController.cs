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

	public GameObject startMessageTarget;

	static public Dictionary<int,int> controllerDeviceIndexToPlayerIndexMap = new Dictionary<int,int>();
	static public int keyboardIndex = -1;

	public GameObject[] controllerHighlights;

	void Start()
	{
		colorPerPlayer = new int[] { 0,1,2,3 };
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
		string[] playerColors =	Config.colorsDictionary[ colorKeys[colorPerPlayer[index] ]];
		
		Material mat = ed_playerRenderer[index].material;
		
		string[] split = playerColors[0].Split( new char[]{','}); 
		//Debug.Log ( split[0]+","+split[1]+","+split[2] );
		mat.SetColor("_Color01", new Color( float.Parse(split[0])/255, float.Parse(split[1])/255, float.Parse(split[2])/255));
		
		split = playerColors[1].Split( new char[]{','}); 
		//Debug.Log ( split[0]+","+split[1]+","+split[2] );
		mat.SetColor("_Color02", new Color( float.Parse(split[0])/255, float.Parse(split[1])/255, float.Parse(split[2])/255));
		
		split = playerColors[2].Split( new char[]{','}); 
		//Debug.Log ( split[0]+","+split[1]+","+split[2] );
		mat.SetColor("_Color03", new Color( float.Parse(split[0])/255, float.Parse(split[1])/255, float.Parse(split[2])/255));
		
		split = playerColors[3].Split( new char[]{','}); 
		//Debug.Log ( split[0]+","+split[1]+","+split[2] );
		mat.SetColor("_Color04", new Color( float.Parse(split[0])/255, float.Parse(split[1])/255, float.Parse(split[2])/255));
		
		ed_playerRenderer[index].material = mat;
		playerMats[index] = mat;
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

	public void onColor ( string val )
	{
		//example val  =  "2_r" 
		string[] split = val.Split( new char[]{'_'});		
					
		int playerIndex =  int.Parse(split[0]);
		//int colorDirection = (split[1] == "r")? 1:-1;

		colorPerPlayer[playerIndex] =  (colorPerPlayer[playerIndex] + 1) % colorKeys.Length; 
		setColorOn( playerIndex);
	}

	public void onStartClicked( string nameOfLevel )
	{
		int players = 0;
		foreach ( SwitchableTexture switchable in ed_controlSelectors)
		{
			if( switchable.index > 0 )
				players += 1; 
		}
			
		if (players > 0 )
		{
			startMessageTarget.SendMessage ( "onStartClicked", nameOfLevel);
		}
	}

	public void onJoin( string val)
	{
		int index = int.Parse(val);
		ed_controlSelectors[index].setIndex(2);
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
			}	
			else
			{
				Debug.Log ( "Controller doesnt fit. All positions are full!");
			}	
		}
	}
}
