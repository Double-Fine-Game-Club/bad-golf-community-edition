using UnityEngine;
using System.Collections;

public class TransferToSwing : MonoBehaviour {

	public GameObject ball;
	private bool inHittingRange = false;
	
	// Update is called once per frame
	void Update ()
	{
		float distance = Vector3.Distance (gameObject.transform.position, ball.transform.position);
		if (distance < 5) {
			inHittingRange = true;
		} else {
			inHittingRange = false;
		}
		if (Input.GetKey (KeyCode.E)) {
			if (inHittingRange) {
				ball.SendMessage ("turnOnScripts");

				this.gameObject.SendMessage("turnOffScripts");
			}
		}
	}
	
	void OnGUI ()
	{
		GUI.Box (new Rect (200, 200, 100, 100), "in range: " + inHittingRange);
	}
}