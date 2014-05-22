using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class netWinCollider : MonoBehaviour {

	//This is something that should exist elsewhere
	public Dictionary<GameObject, NetworkPlayer> ballPlayerMap = new Dictionary<GameObject, NetworkPlayer>();
	//messageTarget is either networkManagerServer or LocalMultiplayerController
	public GameObject messageTarget;
	networkVariables nvs;

	public void initialize()
	{
		// get a reference to NetworkObject
		messageTarget = GameObject.FindWithTag("NetObj");
		nvs = messageTarget.GetComponent ("networkVariables") as networkVariables; 
		if(nvs.gameMode==GameMode.Local)
			messageTarget= GameObject.Find(nvs.levelName).gameObject;
		
		foreach(PlayerInfo player in nvs.players)
		{
			ballPlayerMap.Add(player.ballGameObject, player.player);
		}
	}

	void OnCollisionEnter( Collision coll)
	{
		if(nvs.gameMode==GameMode.Local){
			foreach( GameObject ball in ballPlayerMap.Keys)
			{
				if ( coll.gameObject == ball)
				{
					messageTarget.SendMessage ( "declareWinner", ball.transform.parent.gameObject );
					break;
				}		
			}
		}else if(nvs.gameMode==GameMode.Online){
			if(Network.isClient){return;}	//server decides when it counts

			foreach( GameObject ball in ballPlayerMap.Keys)
			{
				if ( coll.gameObject == ball)
				{
					messageTarget.networkView.RPC ( "DeclareWinner", RPCMode.All, ballPlayerMap[ball] );
					break;
				}		
			}
		}
	}

}
