using UnityEngine;
using System.Collections.Generic;
using InControl;

public class InControlSetup : MonoBehaviour 
{
	static public bool hasBeenSetup = false;

	// Use this for initialization
	void Start () 
	{
		InputManager.Setup();
		hasBeenSetup = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		InputManager.Update();
	}
}
