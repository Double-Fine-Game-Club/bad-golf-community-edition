using UnityEngine;
using InControl;
using System;
using System.Collections;
using System.Collections.Generic;

public class ControllerSupport : MonoBehaviour 
{
	public GameObject[] playerObjectList;
	public int[] playerToControllerIndex;

	public Renderer[] playerBodyList;

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

				#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
				{
					playerObjectList[i].GetComponent<CarUserControl>().isKeyboardControlled = false;	
				}
				#endif
			}
			else
			{
				playerObjectList[i].GetComponent<CarUserControl>().isKeyboardControlled = false;
				playerObjectList[i].GetComponent<CarUserControl>().inputDevice = LobbyControllerSupport.inputDeviceList[playerToControllerIndex[i]];
				playerObjectList[i].GetComponent<CarUserControl>().isLocalMulti = true;
			}
		}
	}
#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
	Vector2 accelerationToSend = Vector2.zero;
#endif
	
	void Update () 
	{
		if( ready )
		{
			for (int i = 0; i < playerObjectList.Length; i++) 
			{
				if ( playerToControllerIndex[i] == -1) //keyboard controlled
				{
					//Touch Devices are sorta treated like mouse, check here
					#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
					{	
						//acceleromter for direction
						accelerationToSend.x = Input.acceleration.x;
						accelerationToSend.y = Input.acceleration.y + .5f;
						playerObjectList[i].SendMessage( "directionUpdate", 
						                                accelerationToSend); 
						//any touch is button
						if (Input.touchCount > 0)
						{
							for (int c = 0; c < Input.touchCount; c++) 
							{
								if (Input.GetTouch(c).phase == TouchPhase.Ended) 
								{
									playerObjectList[i].SendMessage( "onUserGamePadButton", Vector2.zero );
								}
							}
						}
					}
					#endif
 
					continue;
				}

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

// to debug accelerometer uncomment this	
//#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
//	void OnGUI()
//	{
//		GUIStyle myStyle = new GUIStyle();
//		myStyle.fontSize = 30;
//		myStyle.normal.textColor = Color.red;
//		
//		
//		GUI.Label( new Rect( 100, 100, 200, 200), accelerationToSend.x.ToString("##.##")  + "," + accelerationToSend.y.ToString("##.##"), myStyle);
//	}
//#endif	
}