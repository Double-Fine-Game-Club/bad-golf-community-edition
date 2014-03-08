using UnityEngine;
using System.Collections;

// New script separated from Movement. This only makes sure you can move to Swing Mode when you are close enough and press 'E'.

public class TransferToSwing : MonoBehaviour 
{

	public GameObject ball;
	// For updating the GUI Box.
	private bool inHittingRange = false;
	public Rect guiBoxPosition = new Rect (200, 200, 100, 100);
	public bool showGUI = false;

	private Color originalColor = Color.grey;

	private CarUserControl carUserControl;
	
	void Start()
	{
		originalColor = ball.renderer.material.GetColor("_ColorTint");

		carUserControl = GetComponent<CarUserControl>();
	}	

	void Update ()
	{
		//Debug.Log ("Cart Vel: " + gameObject.rigidbody.velocity);
		float distance = Vector3.Distance (gameObject.transform.position, ball.transform.position);
		if (distance < 5) 
		{
			inHittingRange = true;
			Material temp =	ball.renderer.sharedMaterial; 
			temp.SetColor("_ColorTint", Color.red);
			ball.renderer.sharedMaterial = temp;
		} 
		else 
		{
			inHittingRange = false;
			Material temp =	ball.renderer.sharedMaterial; 
			temp.SetColor("_ColorTint", originalColor);
			ball.renderer.sharedMaterial = temp;
		}
		if (Input.GetKey (KeyCode.E) && carUserControl.isKeyboardControlled) 
		{
			if (inHittingRange) 
			{
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
		if( showGUI)
			GUI.Box (guiBoxPosition, "in range: " + inHittingRange);
	}

	void onUserGamePadButton()
	{
		if (inHittingRange) 
		{
			ball.SendMessage( "onUserGamePadButton", Vector2.zero);
			// Stop the cart's forward motion (still might roll away though)
			gameObject.rigidbody.velocity = Vector3.zero;
			ball.SendMessage ("turnOnScripts");
			this.gameObject.SendMessage("turnOffScripts");
		}
	}

	void directionUpdate( Vector2 direction)
	{
		ball.SendMessage( "directionUpdate", direction);
	}
}