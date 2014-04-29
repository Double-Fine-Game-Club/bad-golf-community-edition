using UnityEngine;
using System.Collections;

public class HoverableTexture : MonoBehaviour 
{
	public GameObject normal;
	public GameObject hover;

	public AudioClip hoverAudio;

	private bool isForcedHover = false;
	
	void OnEnable()
	{
		if ( isForcedHover)
		{
			hover.SetActive(true);
			normal.SetActive(false);
		}
		else
		{
			normal.SetActive(true);
			hover.SetActive(false);
		}
	}

	void OnMouseOver()
	{
		if ( !isForcedHover)
		{
			normal.SetActive(false);
			hover.SetActive(true);
		}
	}

	void OnMouseEnter()
	{
		if ( !isForcedHover && hoverAudio != null )
		{
			SoundManager.Get().playSfx( hoverAudio);
		}
	}
	
	void OnMouseExit()
	{
		if ( !isForcedHover )
		{
			normal.SetActive(true);
			hover.SetActive(false);
		}
	}

	void removeForcedHover()
	{
		isForcedHover = false;
		normal.SetActive(true);
		hover.SetActive(false);
	}
		
	void forceHover()
	{
		hover.SetActive(true);
		normal.SetActive(false);
		isForcedHover = true;
	}
}
