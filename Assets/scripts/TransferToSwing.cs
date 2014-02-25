using UnityEngine;
using System.Collections;

// New script separated from Movement. This only makes sure you can move to Swing Mode when you are close enough and press 'E'.

public class TransferToSwing : MonoBehaviour {

	public GameObject ball;
	// For updating the GUI Box.
	private bool inHittingRange = false;
	
	// Update is called once per frame
	void Update ()
	{
		Debug.Log ("Cart Vel: " + gameObject.rigidbody.velocity);
		float distance = Vector3.Distance (gameObject.transform.position, ball.transform.position);
		if (distance < 5) {
			inHittingRange = true;
		} else {
			inHittingRange = false;
		}
		if (Input.GetKey (KeyCode.E)) {
			if (inHittingRange) {
				// Stop the cart's forward motion (still might roll away though)
				gameObject.rigidbody.velocity = Vector3.zero;
				ball.SendMessage ("turnOnScripts");
				this.gameObject.SendMessage("turnOffScripts");
			}
		}
	}

	// Makes the ugly GUI box that tells when you are close enough to the ball.
	void OnGUI ()
	{
		GUI.Box (new Rect (200, 200, 100, 100), "in range: " + inHittingRange);
	}
}