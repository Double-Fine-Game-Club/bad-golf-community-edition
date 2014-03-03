using UnityEngine;
using InControl;
using System;
using System.Collections;
using System.Collections.Generic;

public class ControllerSupport : MonoBehaviour 
{
	public GameObject[] playerObjectList;
	public int[] playerToControllerIndex;

	public bool ready = false;

	void Start () 
	{	

	}

	public void checkKeyboard()
	{
		for (int i = 0; i < playerToControllerIndex.Length; i++) 
		{
			if ( playerToControllerIndex[i] == -1 )
			{
				playerObjectList[i].GetComponent<CarUserControl>().isKeyboardControlled = true;
				playerObjectList[i].GetComponent<CarUserControl>().isLocalMulti = true;
			}
			else
			{
				playerObjectList[i].GetComponent<CarUserControl>().isKeyboardControlled = false;
				playerObjectList[i].GetComponent<CarUserControl>().inputDevice = LobbyControllerSupport.inputDeviceList[playerToControllerIndex[i]];
				playerObjectList[i].GetComponent<CarUserControl>().isLocalMulti = true;
			}
		}
	}
	
	void Update () 
	{
		if( ready )
		{
			for (int i = 0; i < playerObjectList.Length; i++) 
			{
				if ( playerToControllerIndex[i] == -1) //keyboard controlled
					continue;
	
				//direction
				playerObjectList[i].SendMessage( "directionUpdate", 
				                                LobbyControllerSupport.inputDeviceList[playerToControllerIndex[i]].Direction );		
			
				//any button press
				if ( LobbyControllerSupport.inputDeviceList[playerToControllerIndex[i]].Action1.WasReleased ||  
				     LobbyControllerSupport.inputDeviceList[playerToControllerIndex[i]].Action2.WasReleased || 
				     LobbyControllerSupport.inputDeviceList[playerToControllerIndex[i]].Action3.WasReleased || 
				     LobbyControllerSupport.inputDeviceList[playerToControllerIndex[i]].Action4.WasReleased)
				{
					playerObjectList[i].SendMessage( "onUserGamePadButton", 
					                                LobbyControllerSupport.inputDeviceList[playerToControllerIndex[i]].Direction);
				}
			}
		}
	}
}