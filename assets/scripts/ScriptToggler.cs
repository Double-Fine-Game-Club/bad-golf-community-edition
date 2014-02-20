using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptToggler : MonoBehaviour
{

		public List<MonoBehaviour> scripts;
		public GameObject camera;

		// Use this for initialization
		void Start ()
		{
		}
	
		// Update is called once per frame
		void Update ()
		{

		}

		void turnOnScripts ()
		{
				foreach (MonoBehaviour script in scripts) {
						script.enabled = true;
				}
				camera.SetActive (true);
		}

		void turnOffScripts ()
		{
				foreach (MonoBehaviour script in scripts) {
						script.enabled = false;
				}
				camera.SetActive (false);
		}
}
