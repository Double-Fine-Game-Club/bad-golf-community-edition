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
				playerObjectList[i].GetComponent<CarUserControl>().isKeyboardControlled = true;
			else
				playerObjectList[i].GetComponent<CarUserControl>().isKeyboardControlled = false;
		}
	}
	
	void FixedUpdate () 
	{
		if( ready )
		{
			for (int i = 0; i < playerObjectList.Length; i++) 
			{
				if ( playerToControllerIndex[i] == -1) //keyboard controlled
					continue;
	
				playerObjectList[i].SendMessage( "directionUpdate", 
				                                LobbyControllerSupport.inputDeviceList[playerToControllerIndex[i]].Direction );		
			}
		}
	}
}