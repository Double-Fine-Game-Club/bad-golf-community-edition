using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour 
{
	public GameObject ed_introScreen;
	public GameObject ed_menuScreen;
	public GameObject ed_roomScreen;
	public GameObject ed_creditsScreen;

	private GameObject loadedLevel;
	private bool levelLoaded = false;

	void Start()
	{
		hideAllScreens();
		ed_introScreen.SetActive(true);
		StartCoroutine( waitForIntro() );
	}

	IEnumerator waitForIntro()
	{
		yield return new WaitForSeconds(2);
		onIntroIsDone();
	}

	public void onIntroIsDone()
	{
		onMenu();
	}

	public void onLocalMultiplayer()
	{
		hideAllScreens();
		ed_roomScreen.SetActive(true);
	}

	public void onMenu()
	{
		hideAllScreens();
		ed_menuScreen.SetActive(true);
	}

	public void onCredits()
	{
		hideAllScreens();
		ed_creditsScreen.SetActive(true);
	}
	
	public void onStartClicked()
	{
		//TODO: get value from selection thing in menu
		Application.LoadLevelAdditive( "level_01");
		hideAllScreens();
	}

	public void hideAllScreens()
	{
		ed_introScreen.SetActive(false);
		ed_menuScreen.SetActive(false);
		ed_roomScreen.SetActive(false);	
		ed_creditsScreen.SetActive(false);
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