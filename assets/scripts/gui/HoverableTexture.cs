using UnityEngine;
using System.Collections;

public class HoverableTexture : MonoBehaviour 
{
	public GameObject normal;
	public GameObject hover;

	public AudioClip hoverAudio;
	
	void OnEnable()
	{
		normal.SetActive(true);
		hover.SetActive(false);
	}

	void OnMouseOver()
	{
		normal.SetActive(false);
		hover.SetActive(true);
	}

	void OnMouseEnter()
	{
		if ( hoverAudio != null )
		{
			SoundManager.Get().playSfx( hoverAudio);
		}
	}
	
	void OnMouseExit()
	{
		normal.SetActive(true);
		hover.SetActive(false);
	}
}
