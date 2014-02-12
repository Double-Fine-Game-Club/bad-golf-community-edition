using UnityEngine;
using System.Collections;

public class Clickable : MonoBehaviour 
{
	public Camera theCam;
	public GameObject target;
	public string messageName;
	
	// Update is called once per frame
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
	}
}