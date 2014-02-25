using UnityEngine;
using System.Collections;

public class HoverableSpriteTint : MonoBehaviour 
{
	public SpriteRenderer targetSprite;
	public Color colorHover = Color.red;
	public Color colorNormal = Color.white;
	public Color colorDisabled = Color.gray;

	public AudioClip hoverAudio;

	public bool disabled = false;

	void Start()
	{
		targetSprite.color = colorNormal;
	}

	void OnEnable()
	{
		if ( !disabled)
			targetSprite.color = colorNormal;
	}
	
	void OnMouseOver()
	{
		if ( !disabled)
			targetSprite.color = colorHover;
	}

	void OnMouseEnter()
	{
		if ( !disabled && hoverAudio != null )
		{
			SoundManager.Get().playSfx( hoverAudio);
		}
	}

	void OnMouseExit()
	{
		if ( !disabled)
			targetSprite.color = colorNormal;
	}

	public void disableButton()
	{
		disabled = true;
		targetSprite.color = colorDisabled;
	}
	
	public void enableButton()
	{
		disabled = false;
		targetSprite.color = colorNormal;
	}
}
