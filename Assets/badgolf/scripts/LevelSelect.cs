using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour {
	
	public string levelSelected;
	public string[] levels;
	public int id = 0;
	private TextMesh lvlSel;
	
	void Start(){
		UnityEditor.PrefabUtility.ResetToPrefabState (gameObject);
		lvlSel = gameObject.GetComponent ("TextMesh") as TextMesh;
		GameObject gObj = gameObject;
		Config cfg = (Config)gObj.GetComponent(typeof(Config));
		cfg.cfgLoadLevels();
	}
	
	// Update is called once per frame
	void Update () {
		if (this.gameObject.name == "levelID") {
			
			if (Input.GetKeyDown (KeyCode.LeftArrow) && id > 0) {
				id--;
			} else if (Input.GetKeyDown (KeyCode.RightArrow) && id < levels.Length - 1) {
				id++;
			}
			levelSelected = levels [id];
			lvlSel.text = levelSelected;
		}
	}
}