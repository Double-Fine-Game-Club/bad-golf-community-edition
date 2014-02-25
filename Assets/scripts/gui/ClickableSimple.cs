using UnityEngine;
using System.Collections;

public class ClickableSimple : MonoBehaviour
{
	public GameObject messageTarget;
	public string messageName;
	public AudioClip clickAudio;
	public string messageValue;
	
	void OnMouseUp()
	{
		if ( messageTarget != null)
		{
			if (messageValue != "")
				messageTarget.SendMessage(messageName,messageValue);
			else
				messageTarget.SendMessage(messageName);
		}
		else
		{
			if(messageValue != "")
				SendMessageUpwards( messageName, messageValue);
			else
				SendMessageUpwards( messageName);
			
		}

		if( clickAudio != null )
		{
			SoundManager.Get().playSfx( clickAudio);
		}
	}
}