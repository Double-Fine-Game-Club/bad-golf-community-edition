using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
	private Vector3 initPosition;
	private Quaternion initRotation;
	public float rotationLimit = 90;
	public bool inverted = false;

	// Use this for initialization
	void Start ()
	{
		initPosition = transform.localPosition;
		initRotation = transform.localRotation;
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.localPosition = initPosition;
		transform.localRotation = initRotation;

		float angle = -(rotationLimit*Input.GetAxis("CameraHorizontal"));

		if (inverted) angle = -angle;

		// Rotating.
		transform.RotateAround (transform.parent.position, transform.parent.up, angle );	
	}
}
