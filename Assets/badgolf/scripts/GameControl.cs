using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour 
{
	public GameObject ed_menuScreen;

	private GameObject loadedLevel;

	private bool levelLoaded = false;

	public void onStartClicked()
	{
		//TODO: get value from selection thing in menu
		Application.LoadLevelAdditive( "level_01");
		ed_menuScreen.SetActive(false);
	}

	void Update()
	{
		if ( levelLoaded )
		{
			loadedLevel = transform.root.FindChild( "level_01").gameObject;
			levelLoaded = false;
		}
	}
}
