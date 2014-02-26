using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour {

	public int respawnThreshold = 0;
	private Vector3 playerSpawn;
	private Quaternion spawnRotation;
	// Use this for initialization
	void Start () {
	//set spawn transform
	playerSpawn = transform.position;
	spawnRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		if(transform.position.y < respawnThreshold){
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			transform.rotation = spawnRotation;
			transform.position = playerSpawn;
		}
	}
}
