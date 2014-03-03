using UnityEngine;
using System.Collections;

public class LocalMultiWinCollider : MonoBehaviour 
{
	public GameObject[] ed_targetBalls;
	public GameObject messageTarget;
	
	void OnCollisionEnter( Collision coll)
	{
		foreach( GameObject ball in ed_targetBalls)
		{
			if ( coll.gameObject == ball)
			{
				messageTarget.SendMessage ( "declareWinner", ball.transform.parent.gameObject );
				break;
			}		
		}

	}
}
