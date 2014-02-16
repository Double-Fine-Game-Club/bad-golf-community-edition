using UnityEngine;
using System.Collections;

public class CartWheelControl : MonoBehaviour
{
	public float motorPower = 100.0f;
	public float brakePower = 100.0f;
	public float angle = 0.0f;

	void Start ()
	{
		gameObject.GetComponent<WheelCollider>().motorTorque = 0;
		gameObject.GetComponent<WheelCollider>().brakeTorque = 0;

		gameObject.GetComponent<WheelCollider> ().steerAngle = 0;
	}

	void Update ()
	{
		// Forward motion.
		if (Input.GetButton ("Accel"))
				gameObject.GetComponent<WheelCollider> ().motorTorque = motorPower;
		else
				gameObject.GetComponent<WheelCollider> ().motorTorque = 0;

		// Braking.
		if (Input.GetButton ("Brake"))
		{
			gameObject.GetComponent<WheelCollider> ().brakeTorque = brakePower;
		}
		else
			gameObject.GetComponent<WheelCollider> ().brakeTorque = 0;

		// Steering.
		gameObject.GetComponent<WheelCollider> ().steerAngle = Input.GetAxis("Horizontal") * angle;

	}
}
