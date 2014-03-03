using UnityEngine;
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

	void OnEnable () 
	{
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

		//convert the device map to something usable
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

			if ( LocalMultiplayerLobbyController.keyboardIndex != -1)
			{
				currentView.GetComponent<ControllerSupport>().playerToControllerIndex[0] = -1;
				currentView.SetActive(true);
				currentUI.SetActive(true);
				currentView.GetComponent<ControllerSupport>().ready = true;
			}
			else
			{
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

		currentView.GetComponent<ControllerSupport>().checkKeyboard();
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
	
	//LocalMultiplayerLobbyController.controllerDeviceIndexToPlayerIndexMap.Values.CopyTo( playerToControllerIndex, 0) ;	
	//Array.Sort<int>( playerToControllerIndex, new Comparison<int>( (a,b) => a.CompareTo(b) ));
}