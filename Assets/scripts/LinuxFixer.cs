using UnityEngine;
using System.Collections;

public class LinuxFixer : MonoBehaviour {
	public Font LinuxFont;
	
	void OnGUI() {
		GUI.skin.font = LinuxFont;
	}
}
