using UnityEngine;
using System.Collections;

public class HoverableTexture : MonoBehaviour 
{
	public GameObject normal;
	public GameObject hover;
	
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
	
	void OnMouseExit()
	{
		normal.SetActive(true);
		hover.SetActive(false);
	}
}
