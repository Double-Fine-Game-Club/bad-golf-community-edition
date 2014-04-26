using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour 
{
	private int levelSelected;
	private TextMesh lvlSel;

	public GameObject messageTarget;

	void Start()
	{
		lvlSel = gameObject.GetComponent ("TextMesh") as TextMesh;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (this.gameObject.name == "levelID") 
		{
			if (Input.GetKeyDown (KeyCode.LeftArrow) && levelSelected > 0) 
			{
				levelSelected--;
			} 
			else if (Input.GetKeyDown (KeyCode.RightArrow) && levelSelected < Config.levels.Length - 1) 
			{
				levelSelected++;
			}

			lvlSel.text = Config.levels[levelSelected];
		}
	}

	public void onNextLevel()
	{
		levelSelected = (levelSelected + 1)% Config.levels.Length;
	}

	public void onStartClicked( )
	{
		messageTarget.SendMessage( "onStartClicked", Config.levels[levelSelected]);
	}
}