using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour {
	
	public int levelSelected;
	public string[] levels;
	private TextMesh lvlSel;

	public GameObject messageTarget;

	
	void Start()
	{
		lvlSel = gameObject.GetComponent ("TextMesh") as TextMesh;
		GameObject gObj = gameObject;
		Config cfg = (Config)gObj.GetComponent(typeof(Config));
		cfg.cfgLoadLevels();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (this.gameObject.name == "levelID") 
		{
			if (Input.GetKeyDown (KeyCode.LeftArrow) && levelSelected > 0) {
				levelSelected--;
			} else if (Input.GetKeyDown (KeyCode.RightArrow) && levelSelected < levels.Length - 1) {
				levelSelected++;
			}
			lvlSel.text = levels [levelSelected];
		}
	}

	public void onNextLevel()
	{
		levelSelected = (levelSelected + 1)% levels.Length;
	}

	public void onStartClicked( )
	{
		messageTarget.SendMessage( "onStartClicked", levels[levelSelected]);
	}
}