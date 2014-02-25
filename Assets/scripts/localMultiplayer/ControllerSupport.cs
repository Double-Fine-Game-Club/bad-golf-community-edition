using UnityEngine;
using InControl;
using System.Collections.Generic;

public class ControllerSupport : MonoBehaviour 
{
	public GameObject[] playerObjectList;

	private InputDevice[] inputDeviceList;

	void Start () 
	{
		inputDeviceList = InputManager.Devices.ToArray(); 
	}
	
	void FixedUpdate () 
	{
		for (int i = 0; i < playerObjectList.Length; i++) 
		{
			playerObjectList[i].SendMessage( "directionUpdate", inputDeviceList[i].Direction );		
		}
	}
}