using UnityEngine;
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
	
	/*
	void Update () 
	{
		if ( Input.GetMouseButtonUp(0) )
		{
			Ray ray = theCam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100))
			{
				if ( hit.collider.gameObject == this.gameObject )
					target.SendMessage(messageName);
			}
		}
	}*/
}