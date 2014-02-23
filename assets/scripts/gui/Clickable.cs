using UnityEngine;
using System.Collections;

public class Clickable : MonoBehaviour
{
	public GameObject messageTarget;
	public string messageName;
	public AudioClip clickAudio;
	
	void OnMouseDown()
	{
		messageTarget.SendMessage(messageName);

		if( clickAudio != null )
		{
			SoundManager.Get().playSfx( clickAudio);
		}
	}
}