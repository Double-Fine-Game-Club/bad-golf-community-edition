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

	private string nameOfLevel;
	private GameObject loadedLevel;
	private bool levelLoaded = false;

	private Dictionary<string,GameObject> loadedLevelMap = new Dictionary<string,GameObject>();

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

	public void onOptions()
	{
		hideAllScreens();
		ed_optionsScreen.SetActive(true);
	}

	public void onMultiClicked()
	{
		hideAllScreens();
		ed_onlineLobbyScreen.SetActive(true);
	}

	public void onStartClicked()
	{
		GameObject gObj = GameObject.Find ("levelID");
		LevelSelect levelSel = gObj.GetComponent(typeof(LevelSelect)) as LevelSelect;	
		Application.LoadLevelAdditive(levelSel.levels[levelSel.levelSelected]);
		hideAllScreens();
	}

	public void onMultiSkip()
	{
		nameOfLevel = "multi_lobby";
		Application.LoadLevelAdditive( nameOfLevel );
		hideAllScreens();
	}

	public void hideAllScreens()
	{
		ed_introScreen.SetActive(false);
		ed_menuScreen.SetActive(false);
		ed_roomScreen.SetActive(false);	
		ed_creditsScreen.SetActive(false);
		ed_onlineLobbyScreen.SetActive(false);
		ed_optionsScreen.SetActive(false);
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