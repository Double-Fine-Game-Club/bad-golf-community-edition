using UnityEngine;
using System.Collections;

public class WheelAnimation : MonoBehaviour
{
	private Quaternion initRotation;
	private float rotationAngle = 0;
	public WheelCollider linkedWheel;

	// Use this for initialization
	void Start ()
	{
		initRotation = transform.localRotation;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Wheel rotation.
		float rot = (linkedWheel.rpm / 60) * Time.deltaTime * 360;
		rotationAngle = Mathf.Repeat (rotationAngle + rot, 360);
		Quaternion qSpin = Quaternion.AngleAxis (rotationAngle, Vector3.up);

		// Steering.
		float steerAngle = linkedWheel.steerAngle;
		Quaternion qSteer = Quaternion.AngleAxis(-steerAngle, Vector3.right);

		// Transforming.
		transform.localRotation = initRotation * qSteer * qSpin ;
	}
}
