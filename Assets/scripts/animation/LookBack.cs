using UnityEngine;
using System.Collections;

// Testing
public class LookBack : MonoBehaviour {
	
	// blend shape indices
	float goalTurn = 0;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		// Manual turn
		if (Input.GetKeyDown (KeyCode.H)) {
			goalTurn = 180;
		} else if (Input.GetKeyUp (KeyCode.H)) {
			goalTurn = 0;
		}

		float diffTurn = transform.localRotation.eulerAngles.y - goalTurn;
		if(Mathf.Abs(diffTurn) > 1)
		{
			transform.Rotate(new Vector3(0, -Mathf.Sign(diffTurn)*20, 0));
		}
	}
}