using UnityEngine;
using System.Collections;

public class CentreOfMass : MonoBehaviour {

	public Vector3 CenterOfMass = new Vector3 (0, -1, 0);

	// Use this for initialization
	void Start () {
		rigidbody.centerOfMass = CenterOfMass;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
