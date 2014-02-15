using UnityEngine;
using UnityEditor;
using System.Collections;

public class Clickable : MonoBehaviour 
{
	//private Camera theCam;
	public string messageName;
	internal TextMesh menuTextMesh;

	void Start(){
		Input.simulateMouseWithTouches = true;
		menuTextMesh = (this.gameObject.GetComponent("TextMesh")) as TextMesh;
	}

	void OnMouseDown(){
		this.SendMessageUpwards(messageName);
	}

	void OnMouseOver(){
		menuTextMesh.color = Color.red;
	}

	void OnMouseExit(){
		menuTextMesh.color = Color.white;
	}

	void OnDisable(){
		if (menuTextMesh != null) {
			menuTextMesh.color = Color.white;
		}
	}


	/* To be uncommented if touch doesn't work
	void Update () 
	{
		if ( Input.touches[0].tapCount == 1 )
		{
			Ray ray = camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100))
			{
				if ( hit.collider.gameObject == this.gameObject )
					OnMouseDown();
			}
		}
	}*/
}