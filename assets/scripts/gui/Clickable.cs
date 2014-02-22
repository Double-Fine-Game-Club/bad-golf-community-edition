using UnityEngine;
using System.Collections;

public class Clickable : MonoBehaviour
{
	public GameObject messageTarget;
	public string messageName;
	
	void OnMouseDown()
	{
		messageTarget.SendMessage(messageName);
	}
}