using UnityEngine;
using System.Collections;

//always sends itself as value when switching

public class SwitchableTexture : MonoBehaviour 
{
	public GameObject[] objectsToSwitchBetween;
	public int index = 0;

	public AudioClip switchAudio;

	public bool wrapAround = false;

	public GameObject messageTarget;
	public string messageName;
	
	void Start()
	{
		showIndex();
	}
	
	public void onSwitchRight()
	{
		index += 1 ;
		if ( index > objectsToSwitchBetween.Length-1)
		{
			index = (wrapAround)?0: objectsToSwitchBetween.Length-1;
		}
		showIndex();
	}
	
	public void onSwitchLeft()
	{
		index -= 1 ;
		if ( index < 0)
		{
			index = (wrapAround)? objectsToSwitchBetween.Length-1 : 0;
		}
		showIndex();
	}

	public void showIndex()
	{
		for (int i = 0; i < objectsToSwitchBetween.Length; i++) 
		{
			if ( index == i )
				objectsToSwitchBetween[i].SetActive(true);
			else
				objectsToSwitchBetween[i].SetActive(false);
		}

		//do message send when show index 
		if ( messageTarget != null)
		{
			messageTarget.SendMessage(messageName, this);
		}
		
		if( switchAudio != null )
		{
			SoundManager.Get().playSfx( switchAudio);
		}
	}

	public void setIndex( int newIndex)
	{
		index = newIndex;
		showIndex();
	}
}
