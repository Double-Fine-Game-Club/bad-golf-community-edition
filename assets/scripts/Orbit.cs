using UnityEngine;
using System.Collections;

public class Orbit : MonoBehaviour {

	public Vector3 Point;
	public Vector3 Axis;
	public float Speed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround(Point, Axis, Speed);
	}
}
