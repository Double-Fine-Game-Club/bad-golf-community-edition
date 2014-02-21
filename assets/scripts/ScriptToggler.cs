using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptToggler : MonoBehaviour
{
		// Takes in a list of scripts to turn on or off.
		public List<MonoBehaviour> scripts;
		// Also a camera so all the other scripts don't need to have it.
		public GameObject camera;

		// Use this for initialization
		void Start ()
		{
		}
	
		// Update is called once per frame
		void Update ()
		{

		}

		// Turn on all the scripts and the camera on the object.
		void turnOnScripts ()
		{
				foreach (MonoBehaviour script in scripts) {
						script.enabled = true;
				}
				camera.SetActive (true);
		}

		// Turn off all the scripts and the camera.
		void turnOffScripts ()
		{
				foreach (MonoBehaviour script in scripts) {
						script.enabled = false;
				}
				camera.SetActive (false);
		}
}
