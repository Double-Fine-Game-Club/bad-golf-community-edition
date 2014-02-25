using UnityEngine;
using InControl;
using System.Collections.Generic;
using System.Collections;

public class LobbyControllerSupport : MonoBehaviour 
{
	public GameObject lobbyGameObjectTarget;
	static public InputDevice[] inputDeviceList;

	Vector2 lastDirection = Vector2.zero;
	
	void OnEnable () 
	{
		inputDeviceList = InputManager.Devices.ToArray(); 
	}
	
	void Update () 
	{
		if ( lobbyGameObjectTarget != null)
		{
			for (int i = 0; i < inputDeviceList.Length; i++) 
			{
				if ( inputDeviceList[i].Direction != Vector2.zero ) 
				{
					if ( lastDirection.x != Mathf.Round(inputDeviceList[i].Direction.x) || lastDirection.y != Mathf.Round(inputDeviceList[i].Direction.y) )
					{
						lastDirection.x = Mathf.Round(inputDeviceList[i].Direction.x);
						lastDirection.y = Mathf.Round(inputDeviceList[i].Direction.y);
						
						lobbyGameObjectTarget.SendMessage( "onControlDirection", i );
					}
				
					if ( Mathf.Round(inputDeviceList[i].Direction.x) == 0 ||  Mathf.Round(inputDeviceList[i].Direction.y) == 0 )
					{
						StartCoroutine( waitThenClearLastDirection());	
					}
				}
					
				if ( inputDeviceList[i].Action1.WasReleased ||  inputDeviceList[i].Action2.WasReleased || inputDeviceList[i].Action3.WasReleased || inputDeviceList[i].Action4.WasReleased)
				{
					lobbyGameObjectTarget.SendMessage( "onControlAnyButtonPress", i);
				}
			}
		}
	}

	IEnumerator waitThenClearLastDirection()
	{	
		yield return new WaitForSeconds(.5f);
		lastDirection.x = lastDirection.y = -99; //some garbage number
	}
}