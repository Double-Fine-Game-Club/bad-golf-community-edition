using UnityEngine;
using System.Collections;

public class ControllerHighlightManager : MonoBehaviour 
{
	public GameObject[] uiItemsToHover;
	public string[] messagesToSendToThem;	

	int currentIndex = 0;

	void OnEnable()
	{
		currentIndex = 0;
		transform.position = new Vector3( transform.position.x, uiItemsToHover[currentIndex].transform.position.y, transform.position.z);
	}

	void doRight ( )
	{
		if ( messagesToSendToThem[currentIndex] != null ||  messagesToSendToThem[currentIndex] != "" )
		{
			uiItemsToHover[currentIndex].SendMessage ( messagesToSendToThem[currentIndex] ,SendMessageOptions.DontRequireReceiver);
		}
	}

	void doLeft ( )
	{
		if ( messagesToSendToThem[currentIndex] != null ||  messagesToSendToThem[currentIndex] != "" )
		{
			uiItemsToHover[currentIndex].SendMessage ( messagesToSendToThem[currentIndex] ,SendMessageOptions.DontRequireReceiver);
		}
	}

	void doDown ( )
	{
		currentIndex = (currentIndex + 1 < uiItemsToHover.Length)? (currentIndex + 1) : 0 ;
		transform.position = new Vector3( transform.position.x, uiItemsToHover[currentIndex].transform.position.y, transform.position.z);
	}

	void doUp ( )
	{
		currentIndex = (currentIndex - 1 > -1)? (currentIndex - 1) : uiItemsToHover.Length-1;
		transform.position = new Vector3( transform.position.x, uiItemsToHover[currentIndex].transform.position.y, transform.position.z);
	}

	void doButtonPress ( )
	{
		if ( messagesToSendToThem[currentIndex] != null ||  messagesToSendToThem[currentIndex] != "" )
		{
			uiItemsToHover[currentIndex].SendMessage ( messagesToSendToThem[currentIndex]  ,SendMessageOptions.DontRequireReceiver);
		}	
	}
}
