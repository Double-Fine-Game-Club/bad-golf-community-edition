using UnityEngine;
using System.Collections;

public class ControllerHighlightManager : MonoBehaviour 
{
	public GameObject[] uiItemsToHover;

	public string[] actionMessage;
	public string[] hoverForceMessage;
	public string[] hoverRemoveMessage;

	int currentIndex = 0;

	public bool startSelected = true;

	void OnEnable()
	{
		if ( startSelected )
			sendHoverForceMessage();
	}

	void doRight ( )
	{
		sendActionMessage();
	}

	void doLeft ( )
	{
		sendActionMessage();
	}

	void doDown ( )
	{
		changeIndex( 1);
	}

	void doUp ( )
	{
		changeIndex(-1);
	}

	void doButtonPress ( )
	{
		sendActionMessage();
	}

	void changeIndex( int direction)
	{
		sendHoverRemoveMessage();

		currentIndex = (currentIndex + direction < uiItemsToHover.Length)? (currentIndex + direction) : 0 ;
		currentIndex = (currentIndex < 0 )? currentIndex = uiItemsToHover.Length-1 : currentIndex;

		sendHoverForceMessage();
	}

	public void sendHoverRemoveMessage()
	{
		uiItemsToHover[currentIndex].SendMessage ( hoverRemoveMessage[currentIndex], SendMessageOptions.DontRequireReceiver);
	}

	public void sendHoverForceMessage()
	{
		uiItemsToHover[currentIndex].SendMessage ( hoverForceMessage[currentIndex], SendMessageOptions.DontRequireReceiver);
	}

	public void sendActionMessage()
	{
		if ( actionMessage[currentIndex] != null ||  actionMessage[currentIndex] != "" )
		{
			uiItemsToHover[currentIndex].SendMessage ( actionMessage[currentIndex], SendMessageOptions.DontRequireReceiver);
		}	
	}
}
