using UnityEngine;
using System.Collections;

public class ScaleMessageReciever : MonoBehaviour 
{
	void scaleToValueX( float value)
	{
		transform.localScale = new Vector3( value,transform.localScale.y,transform.localScale.z);	
	}	

	void scaleToValueY( float value)
	{
		transform.localScale = new Vector3( transform.localScale.x,value,transform.localScale.z);	
	}

	void scaleToValueZ( float value)
	{
		transform.localScale = new Vector3( transform.localScale.x,transform.localScale.y,value);	
	}
}
