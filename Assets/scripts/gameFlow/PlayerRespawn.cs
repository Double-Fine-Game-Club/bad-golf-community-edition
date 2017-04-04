using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour {

	//Y-Axis Coordinate Threshold
	public int respawnThreshold = 0;
	private Vector3 spawnLocation;
	private Quaternion spawnRotation;


	// Use this for initialization
	void Start () {

		//set player spawn
		spawnRotation = transform.rotation;
		spawnLocation = transform.position;
	
	}
	
	// Update is called once per frame
	void Update () {

		//Respawn if y coordinate falls below threshold
		if(transform.position.y < respawnThreshold){
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			transform.rotation = spawnRotation;
			transform.position = spawnLocation;

		}
	}
}
