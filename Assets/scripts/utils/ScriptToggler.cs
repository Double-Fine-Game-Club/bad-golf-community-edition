using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptToggler : MonoBehaviour
{
		// Takes in a list of scripts to turn on or off.
		public List<MonoBehaviour> scripts;
		// Also a camera so all the other scripts don't need to have it.
		public GameObject cameraObject;

		// Turn on all the scripts and the camera on the object.
		void turnOnScripts ()
		{
			foreach (MonoBehaviour script in scripts) 
			{
				if(script!=null)
					script.enabled = true;
			}
			cameraObject.SetActive (true);
		}

		// Turn off all the scripts and the camera.
		void turnOffScripts ()
		{
			foreach (MonoBehaviour script in scripts) 
			{
				if(script!=null)
					script.enabled = false;
			}
			cameraObject.SetActive (false);
		}
}
