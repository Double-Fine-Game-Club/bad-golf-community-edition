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

		if (currentIndex < 3) {
			//on Right
			changeIndex(3);	//move left
		}else if(currentIndex<6){
			//On Left
			changeIndex (-3);	//move right
		}else{
			//Do nothing
		}
	}

	void doLeft ( )
	{
		if (currentIndex < 3) {
			//on Right
			changeIndex(3);	//move left
		}else if(currentIndex<6){
			//On Left
			changeIndex (-3);	//move right
		}else{
			//Do nothing
		}
	}

	void doDown ( )
	{
		if(currentIndex==2){
			//On arrow_low_r
			changeIndex (4);	//move to level
		}else
			changeIndex( 1);
	}

	void doUp ( )
	{
		if(currentIndex==3 || currentIndex==6){
			//On level or arrow_top_l
			changeIndex (-4);	//move to arrow_low_r or player_controls
		}else
			changeIndex( -1);
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
