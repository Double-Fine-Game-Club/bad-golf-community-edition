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

	void Start()
	{
		targetTextMesh.color = colorNormal;
	}

	void OnEnable()
	{
		if ( !disabled)
			targetTextMesh.color = colorNormal;
	}
	
	void OnMouseOver()
	{
		if ( !disabled)
			targetTextMesh.color = colorHover;
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
		targetTextMesh.color = colorNormal;
	}
}
