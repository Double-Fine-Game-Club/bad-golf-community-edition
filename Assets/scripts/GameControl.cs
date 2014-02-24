using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour 
{
	public GameObject ed_introScreen;
	public GameObject ed_menuScreen;
	public GameObject ed_roomScreen;
	public GameObject ed_creditsScreen;
	public GameObject ed_onlineLobbyScreen;
	public GameObject ed_optionsScreen;
	public GameObject ed_quitDialogScreen;
	public GameObject ed_levelPreviewScreen;

	private string nameOfLevel;
	private GameObject loadedLevel;
	private bool levelLoaded = false;

	private Dictionary<string,GameObject> loadedLevelMap = new Dictionary<string,GameObject>();

	void Start()
	{
		Input.simulateMouseWithTouches = true;

		hideAllScreens();
		ed_introScreen.SetActive(true);
		ed_levelPreviewScreen.SetActive(false);
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
		ed_levelPreviewScreen.SetActive(true);
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

	public void onOptions()
	{
		hideAllScreens();
		ed_optionsScreen.SetActive(true);
	}

	public void onQuit()
	{
		hideAllScreens();
		ed_quitDialogScreen.SetActive(true);
	}

	public void onMultiClicked()
	{
		hideAllScreens();
		ed_onlineLobbyScreen.SetActive(true);
	}

	public void onStartClicked( string name)
	{
		nameOfLevel = name;
		Application.LoadLevelAdditive(nameOfLevel);
		
		hideAllScreens();
		ed_levelPreviewScreen.SetActive(false);
	}

	public void onMultiSkip()
	{
		nameOfLevel = "multi_lobby";
		Application.LoadLevelAdditive( nameOfLevel );
		
		hideAllScreens();
		ed_levelPreviewScreen.SetActive(false);
	}

	public void onQuitClicked()
	{
		Application.Quit();
	}

	public void hideAllScreens()
	{
		ed_introScreen.SetActive(false);
		ed_menuScreen.SetActive(false);
		ed_roomScreen.SetActive(false);	
		ed_creditsScreen.SetActive(false);
		ed_onlineLobbyScreen.SetActive(false);
		ed_optionsScreen.SetActive(false);
		ed_quitDialogScreen.SetActive(false);
	}

	void Update()
	{
		if ( levelLoaded )
		{
			loadedLevelMap.Add(nameOfLevel, transform.root.FindChild( nameOfLevel ).gameObject);
			levelLoaded = false;
		}
	}
}