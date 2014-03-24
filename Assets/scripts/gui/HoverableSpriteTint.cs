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
	public bool isDragging = false;

	private bool isForcedHover = false;

	void Start()
	{
		if ( isForcedHover)
		{
			targetSprite.color = colorHover;
		}
		else if ( disabled )
		{
			targetSprite.color = colorDisabled;
		}
		else
		{
			targetSprite.color = colorNormal;
		}
	}

	void OnEnable()
	{
		if ( isForcedHover)
		{
			targetSprite.color = colorHover;
		}
		else if ( disabled )
		{
			targetSprite.color = colorDisabled;
		}
		else
		{
			targetSprite.color = colorNormal;
		}
	}
	
	void OnMouseOver()
	{
		if ( !disabled)
			targetSprite.color = colorHover;
	}

	void OnMouseEnter()
	{
		if ( !isForcedHover && !disabled && hoverAudio != null && isDragging == false )
		{
			SoundManager.Get().playSfx( hoverAudio);
		}
	}

	void OnMouseExit()
	{
		if ( !isForcedHover && !disabled)
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
		if ( isForcedHover)
		{
			targetSprite.color = colorHover;
		}
		else
		{
			targetSprite.color = colorNormal;
		}
	}

	void OnMouseDown()
	{
		isDragging = true;
	}
	
	void OnMouseUp()
	{
		isDragging = false;		
	}	

	void removeForcedHover()
	{
		isForcedHover = false;
		targetSprite.color = colorNormal;
	}
	
	void forceHover()
	{
		targetSprite.color = colorHover;
		isForcedHover = true;
	}
}
