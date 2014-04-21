using UnityEngine;
using System.Collections;

public class netPlayerRespawn : MonoBehaviour {
	networkVariables nvs;

	//Y-Axis Coordinate Threshold
	float respawnThreshold;
	private Vector3 spawnLocation;
	private Vector3 spawnScale;
	private bool gotReferences = false;


	// Use this for initialization
	void Start () {
		// get variables we need
		nvs = GameObject.FindWithTag("NetObj").GetComponent("networkVariables") as networkVariables;
	}

	// these references don't exist until the level has loaded
	void GetReferences() {
		GameObject lBottom = GameObject.FindWithTag("LevelBottom");
		GameObject lSpawn = GameObject.FindWithTag("LevelSpawn");
		// check to see if the level's loaded
		if (!(lBottom) || !(lSpawn)) return;
		
		// lowest height is set in the level
		respawnThreshold = lBottom.transform.position.y;
		lBottom.SetActive(false);
		
		// set the spawn location
		spawnLocation = lSpawn.transform.position;
		spawnScale = lSpawn.transform.localScale*5;	//default width of plane is 10, so "radius" is 5
		lSpawn.SetActive(false);

		gotReferences = true;
	}
	
	// reset
	void Update () {
		// keep checking until we have dem references
		if (!gotReferences) GetReferences();
		// check for keypress
		if (Input.GetKeyDown(KeyCode.R) && gotReferences) {
			// move them down a lot to trigger a reset
			if(nvs.myInfo!=null)	//TODO: add player identifier support (offline support)
				nvs.myInfo.cartGameObject.transform.position = new Vector3(0,respawnThreshold - 1,0);
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (gotReferences) {
			//if we're a server
			if (nvs.gameMode==GameMode.Local || Network.isServer) {
				// go through all the players buggys
				foreach (PlayerInfo p in nvs.players) {
					//Respawn if y coordinate falls below threshold
					if(p.cartGameObject.transform.position.y < respawnThreshold){
						p.cartGameObject.rigidbody.velocity = Vector3.zero;
						p.cartGameObject.rigidbody.angularVelocity = Vector3.zero;
						p.cartGameObject.transform.rotation = Quaternion.identity;
						Vector3 sLoc = spawnLocation;
						sLoc += new Vector3(spawnScale.x*Random.Range(-1f,1f),0,spawnScale.z*Random.Range(-1f,1f));
						p.cartGameObject.transform.position = sLoc;
						//p.cartGameObject.transform.position = spawnLocation + Quaternion.AngleAxis(Random.Range(0,360), Vector3.up) * new Vector3(10,2,0);
					}
					// also check the ball location
					if(p.ballGameObject.transform.position.y < respawnThreshold){
						p.ballGameObject.rigidbody.velocity = Vector3.zero;
						p.ballGameObject.rigidbody.angularVelocity = Vector3.zero;
						p.ballGameObject.transform.rotation = Quaternion.identity;
						Vector3 sLoc = spawnLocation;
						sLoc += new Vector3(spawnScale.x*Random.Range(-1f,1f),0,spawnScale.z*Random.Range(-1f,1f));
						p.ballGameObject.transform.position = sLoc;
						//p.ballGameObject.transform.position = spawnLocation + Quaternion.AngleAxis(Random.Range(0,360), Vector3.up) * new Vector3(10,2,0);
					}
				}

			// if we're a client
			} else {
				// do nothing yet - once interpolation is done this will be used (maybe)
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
