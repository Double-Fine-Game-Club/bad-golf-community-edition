using UnityEngine;
using System.Collections;

public class SwingMode : MonoBehaviour
{
	
		public GameObject camera;
		private Vector3 cameraPos = new Vector3 (0, 2, -4);
		private float shotPower = 0f;
		public int hitMultiplier = 5;
		private bool flying = false;

	
		// Use this for initialization
		void Start ()
		{
		
		}

		void OnGUI ()
		{
				GUI.Box (new Rect (200, 200, 100, 100), "power: " + (int)shotPower);
		}

	
		// Update is called once per frame
		void Update ()
		{
				Debug.Log ("Flying: " + flying);
				if (Input.GetKey (KeyCode.LeftArrow)) {
						//rigidbody.freezeRotation = false;
						gameObject.transform.Rotate (0f, 1f, 0f);
						rigidbody.freezeRotation = true;
				}
				if (Input.GetKey (KeyCode.RightArrow)) {
						//rigidbody.freezeRotation = false;
						gameObject.transform.Rotate (0f, -1f, 0f);
						rigidbody.freezeRotation = true;
				}
		
		
				Vector3 newPos = transform.position + transform.localRotation * cameraPos;
				float lerper = Mathf.Min ((camera.transform.position - newPos).sqrMagnitude / 100, 1);
				camera.transform.position = (1 - lerper) * camera.transform.position + lerper * newPos;
				camera.transform.rotation = Quaternion.Lerp (camera.transform.rotation, Quaternion.LookRotation (transform.position - camera.transform.position), lerper);

				// if we are in the air, we don't want player to hit again.
				if (flying) {
						return;
				}

				if (Input.GetAxis ("Vertical") > 0) {
						shotPower += Input.GetAxis ("Vertical") * hitMultiplier;
						if (shotPower > 500) {
								shotPower = 500;
						}
				}
		
				if (Input.GetKeyDown (KeyCode.Space)) {
						Debug.Log ("BOOM: " + shotPower);
						flying = true;
						Vector3 arc = Vector3.forward;
						if (shotPower < 100) {
								shotPower = 100;
						}		
						arc.y = arc.y + shotPower / 200;
						rigidbody.AddForce (transform.localRotation * arc * shotPower);
						shotPower = 0;
				}
		}

		void OnCollisionEnter (Collision col)
		{
				if (col.gameObject.tag == "Ground") {
						flying = false;
				}
		}
}
