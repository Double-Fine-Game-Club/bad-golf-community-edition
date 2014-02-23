using UnityEngine;
using System.Collections;

public class Clickable : MonoBehaviour
{
	public GameObject messageTarget;
	public string messageName;
	public AudioClip clickAudio;
	
	void OnMouseDown()
	{
		if ( messageTarget != null)
			messageTarget.SendMessage(messageName);
		else
			SendMessageUpwards( messageName);

		if( clickAudio != null )
		{
			SoundManager.Get().playSfx( clickAudio);
		}
	}
}