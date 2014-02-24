using UnityEngine;
using System.Collections;
using System;

public class LocalMultiplayerLobbyController : MonoBehaviour 
{
	public Camera[] ed_playerViewCameras;
	public SwitchableTexture[] ed_controlSelectors;
	public Renderer[] ed_playerRenderer;
	public GameObject[] ed_joinButtonTexts;
	public GameObject[] ed_detailControls;

	public string[] colorKeys;
	public int[] colorPerPlayer;

	public GameObject startMessageTarget;

	void Start()
	{
		colorPerPlayer = new int[] { 0,1,2,3 };

		colorKeys = new string[Config.colorsDictionary.Count ];
		Config.colorsDictionary.Keys.CopyTo( colorKeys,0);

		//start with no guys
		for (int i = 0; i < ed_playerViewCameras.Length; i++) 
		{
			ed_playerViewCameras[i].farClipPlane = 2;
			ed_playerViewCameras[i].clearFlags = CameraClearFlags.SolidColor;

			ed_controlSelectors[i].setIndex(0);
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
		}
		else if ( type == 1 ) //joystick
		{
			ed_playerViewCameras[playerIndex].farClipPlane = 1000;
			ed_playerViewCameras[playerIndex].clearFlags = CameraClearFlags.Skybox;
			ed_joinButtonTexts[playerIndex].SetActive(false);
			ed_detailControls[playerIndex].SetActive(true);
			//bind to not already used joystick
		}
		else if ( type == 2) //keyboard
		{
			ed_playerViewCameras[playerIndex].farClipPlane = 1000;
			ed_playerViewCameras[playerIndex].clearFlags = CameraClearFlags.Skybox;
			ed_joinButtonTexts[playerIndex].SetActive(false);
			ed_detailControls[playerIndex].SetActive(true);

			foreach( SwitchableTexture switchable in ed_controlSelectors)
			{
				if ( callingSwitch != switchable && switchable.index == 2) //if keyboard
					switchable.setIndex(1);//switch to joystick
			}
		}
	}

	public void onColor ( string val )
	{
		//example val  =  "2_r" 
		string[] split = val.Split( new char[]{'_'});		
					
		int playerIndex =  int.Parse(split[0]);
		int colorDirection = (split[1] == "r")? 1:-1;

		colorPerPlayer[playerIndex] =  (colorPerPlayer[playerIndex] + 1) % 4; 
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
		ed_controlSelectors[index].setIndex(1);
	}
}
