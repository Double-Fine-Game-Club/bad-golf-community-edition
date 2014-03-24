using UnityEngine;
using System.Collections;

public class HoverableTextMesh : MonoBehaviour 
{
	public TextMesh targetTextMesh;
	public Color colorHover = Color.red;
	public Color colorNormal = Color.white;
	public Color colorDisabled = Color.gray;

	public AudioClip hoverAudio;

	public bool disabled = false;

	private bool isForcedHover = false;

	void Start()
	{
		if ( isForcedHover)
		{
			targetTextMesh.color = colorHover;
		}
		else if ( disabled )
		{
			targetTextMesh.color = colorDisabled;
		}
		else
		{
			targetTextMesh.color = colorNormal;
		}
	}

	void OnEnable()
	{
		if ( isForcedHover)
		{
			targetTextMesh.color = colorHover;
		}
		else if ( disabled )
		{
			targetTextMesh.color = colorDisabled;
		}
		else
		{
			targetTextMesh.color = colorNormal;
		}
	}
	
	void OnMouseOver()
	{
		if ( !disabled)
			targetTextMesh.color = colorHover;
	}

	void OnMouseEnter()
	{
		if ( !isForcedHover && !disabled && hoverAudio != null )
		{
			SoundManager.Get().playSfx( hoverAudio);
		}
	}

	void OnMouseExit()
	{
		if ( !isForcedHover && !disabled)
			targetTextMesh.color = colorNormal;
	}

	public void disableButton()
	{
		disabled = true;
		targetTextMesh.color = colorDisabled;
	}
	
	public void enableButton()
	{
		disabled = false;
		if ( !isForcedHover)
		{
			targetTextMesh.color = colorNormal;
		}
		else
		{
			targetTextMesh.color = colorHover;
		}
	}

	void removeForcedHover()
	{
		isForcedHover = false;
		targetTextMesh.color = colorNormal;
	}
	
	void forceHover()
	{
		targetTextMesh.color = colorHover;
		isForcedHover = true;
	}
}
