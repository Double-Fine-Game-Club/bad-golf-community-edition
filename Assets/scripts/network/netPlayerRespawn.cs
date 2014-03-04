using UnityEngine;
using System.Collections;

public class netPlayerRespawn : MonoBehaviour {
	networkVariables nvs;

	//Y-Axis Coordinate Threshold
	float respawnThreshold;
	private Vector3 spawnLocation;


	// Use this for initialization
	void Start () {
		// get variables we need
		nvs = GetComponent("networkVariables") as networkVariables;

		// lowest height can be set in netvars
		respawnThreshold = nvs.lowestHeight;

		// set the spawn location to that of the netobj
		spawnLocation = transform.position;
	}
	
	// reset
	void Update () {
		if (Input.GetKeyDown(KeyCode.R)) {
			nvs.myInfo.cartGameObject.transform.position = new Vector3(0,respawnThreshold - 1,0);
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		// go through all the players buggys
		foreach (PlayerInfo p in nvs.players) {
			//Respawn if y coordinate falls below threshold
			if(p.cartGameObject.transform.position.y < respawnThreshold){
				p.cartGameObject.rigidbody.velocity = Vector3.zero;
				p.cartGameObject.rigidbody.angularVelocity = Vector3.zero;
				p.cartGameObject.transform.rotation = Quaternion.identity;
				p.cartGameObject.transform.position = spawnLocation + Quaternion.AngleAxis(Random.Range(0,360), Vector3.up) * new Vector3(10,2,0);
			}
			// also check the ball location
			if(p.ballGameObject.transform.position.y < respawnThreshold){
				p.ballGameObject.rigidbody.velocity = Vector3.zero;
				p.ballGameObject.rigidbody.angularVelocity = Vector3.zero;
				p.ballGameObject.transform.rotation = Quaternion.identity;
				p.ballGameObject.transform.position = spawnLocation + Quaternion.AngleAxis(Random.Range(0,360), Vector3.up) * new Vector3(10,2,0);
			}
		}
	}
	
	// honks
	[RPC]
	void ResetMe(NetworkMessageInfo info) {
		// find the player
		foreach (PlayerInfo p in nvs.players) {
			if (p.player==info.sender) {
				p.cartGameObject.transform.position = new Vector3(0,respawnThreshold - 1,0);
			}
		}
	}
}
