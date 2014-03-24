using UnityEngine;
using InControl;
using System.Collections.Generic;
using System.Collections;

public class LobbyControllerSupport : MonoBehaviour 
{
	public GameObject lobbyGameObjectTarget;
	static public InputDevice[] inputDeviceList;

	private float[] timeSinceLastMove;
	private Vector2[] lastDirection;
	private float minTimeForRepeat = .35f;
	private float minChangeInDirection = .4f;

	static public bool wasInitialized = false;

	void OnEnable () 
	{
		wasInitialized = true;
		inputDeviceList = InputManager.Devices.ToArray(); 
		
		timeSinceLastMove =new float[]{Time.time,Time.time,Time.time,Time.time};
		lastDirection = new Vector2[]{Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero};
	}
	
	void Update () 
	{
		if ( lobbyGameObjectTarget != null)
		{
			for (int i = 0; i < inputDeviceList.Length; i++) 
			{
				if ( roundVector(inputDeviceList[i].Direction) != roundVector(lastDirection[i]) ) //general direction has changed
				{
					if ( ( inputDeviceList[i].Direction - lastDirection[i] ).magnitude > minChangeInDirection ) // enough change from last time?
					{
						lastDirection[i] = inputDeviceList[i].Direction;
						timeSinceLastMove[i] = Time.time;
						
	
						lobbyGameObjectTarget.SendMessage( "onControlDirection", i );	
					}
				}
				else if ( Time.time - timeSinceLastMove[i] > minTimeForRepeat ) //hasnt changed, enough time to repeat? 
				{
					lastDirection[i] = inputDeviceList[i].Direction;
					timeSinceLastMove[i] = Time.time;

					lobbyGameObjectTarget.SendMessage( "onControlDirection", i );	
				}
	
				if ( inputDeviceList[i].Action1.WasReleased ||  inputDeviceList[i].Action2.WasReleased || inputDeviceList[i].Action3.WasReleased || inputDeviceList[i].Action4.WasReleased)
				{
					lobbyGameObjectTarget.SendMessage( "onControlAnyButtonPress", i);
				}
			}
		}
	}

	//custom vec2 round  
	private float minThresholdX = .38f;
	private float minThresholdY = .25f;
	Vector2 roundVector ( Vector2 vec)
	{
		if ( vec.x > minThresholdX || vec.x < -minThresholdX) 
			vec.x =	Mathf.Sign(vec.x);
		else
			vec.x = 0;

		if ( vec.y > minThresholdY || vec.y < -minThresholdY) 
			vec.y =	Mathf.Sign(vec.y);
		else
			vec.y = 0;

		return vec;
	}
}