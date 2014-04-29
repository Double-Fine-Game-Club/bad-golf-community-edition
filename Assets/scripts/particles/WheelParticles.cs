using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Wheel))]
public class WheelParticles : MonoBehaviour {

	[SerializeField] private ParticleSystem kickUpDirtParticles;
	[SerializeField] private ParticleSystem kickUpDustParticles;

	private Wheel wheel;
	private float kickUpEmisionRate;

	// Use this for initialization
	void Awake () 
	{
		wheel = GetComponent<Wheel>();
		kickUpEmisionRate = kickUpDirtParticles.emissionRate;
	}
	
	// Update is called once per frame
	void Update () 
	{
		//Debug.Log(wheel == null  ? "null" : "not null" , gameObject);
		if(wheel.OnGround)
		{
			TurfKickupIntensity();
		}
		else
		{
			StopKickupParticles();
		}
	}

	private void TurfKickupIntensity()
	{
		float normalizedSpeed = wheel.car.CurrentSpeed / wheel.car.MaxSpeed; 
		//turfParticle.transform.localRotation = Quaternion.AngleAxis(Mathf.Sign(car.CurrentSpeed) * (180 - car.CurrentSteerAngle) ,Vector3.up);
		if(normalizedSpeed > 0.4f)
		{
			kickUpDustParticles.enableEmission = true;
			kickUpDirtParticles.enableEmission = true;
			kickUpDirtParticles.emissionRate = kickUpEmisionRate * normalizedSpeed;
		}
		else
		{
			StopKickupParticles();
		}
	}
	
	private void StopKickupParticles()
	{
		kickUpDustParticles.enableEmission = false;
		kickUpDirtParticles.enableEmission = false;
	}
}
