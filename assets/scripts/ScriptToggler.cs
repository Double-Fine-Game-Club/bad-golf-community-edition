using UnityEngine;
using System.Collections;

public class ScriptToggler : MonoBehaviour {

	public MonoBehaviour script;
	public GameObject camera;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void toggleScript(){
		camera.SetActive (true);
		script.enabled = true;
	}
}
