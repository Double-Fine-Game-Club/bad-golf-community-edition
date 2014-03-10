using UnityEngine;
using System.Collections;

public class HeadHandler_RigTest : MonoBehaviour {

	private GameObject head;
	
	// for interactive testing...
	void Update () {
		if(Input.GetKeyDown(KeyCode.H) && head == null) {
			GameObject body = GameObject.Find("RigtestBody"); // already in scene
			head = HeadHandler.AddHead(body, "test/RigtestHead", "Neck");
		}
		if(Input.GetKeyDown(KeyCode.J) && head != null) {
			GameObject body = GameObject.Find("RigtestBody"); // already in scene
			HeadHandler.RemoveHead(body, head);
		}
	}
}
