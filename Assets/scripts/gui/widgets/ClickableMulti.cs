using UnityEngine;
using System.Collections;

public class ClickableMulti : MonoBehaviour
{
	public GameObject[] messageTarget;
	public string[] messageName;
	public AudioClip clickAudio;
	public string[] messageValue;
	
	void OnMouseUp()
	{
		if ( messageTarget != null)
		{
			for( int i=0; i< messageTarget.Length; i++ )
			{ 
				if (messageValue[i] != "")
					messageTarget[i].SendMessage(messageName[i],messageValue[i]);
				else
					messageTarget[i].SendMessage(messageName[i]);
			}
		}
		else
		{
			if(messageValue[0] != "")
				SendMessageUpwards( messageName[0], messageValue[0]);
			else
				SendMessageUpwards( messageName[0]);
			
		}

		if( clickAudio != null )
		{
			SoundManager.Get().playSfx( clickAudio);
		}
	}
}