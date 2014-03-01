using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarUserControl : MonoBehaviour
{
    private CarController car;  // the car controller we want to use
	public bool isKeyboardControlled = true; //true means only single person, false means split screen

    void Awake ()
    {
        // get the car controller
        car = GetComponent<CarController>();
    }

	void Update()
	{
#if CROSS_PLATFORM_INPUT
		//if(CrossPlatformInput.GetAction3());	//Probably not right
#else
		if(Input.GetKeyDown(KeyCode.Q))
			gameObject.audio.Play();
#endif
	}

    void FixedUpdate()
    {
        // pass the input to the car!
#if CROSS_PLATFORM_INPUT
		float h = CrossPlatformInput.GetAxis("Horizontal");
		float v = CrossPlatformInput.GetAxis("Vertical");
#else
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
#endif
		if ( isKeyboardControlled)
		{
			//Debug.Log ( h + " , " +v );
			car.Move(h,v);
		}
	}
	
	public void directionUpdate( Vector2 direction)
	{
//		Debug.Log ( direction.x + " , " + direction.y );

		car.Move ( direction.x, direction.y);
	}
}
