﻿using UnityEngine;

[RequireComponent(typeof(GUIText))]
public class CarGUI : MonoBehaviour
{

    public CarController car;                   // reference to the car controller to get the information needed for the display
    private const float MphtoMps = 2.237f;      // constant for converting miles per hour to metres per second
	private const float KphToMps = 3.6f;
    private string display =          			// template string for GUI info
        	"{0:0} mph \n" +
			"{1:0} kph \n" +
            "Gear: {2:0}/{3:0}\n" +
            "Revs {4:0%}\n" +
            "Throttle: {5:0%}\n";
	
    void Update()
    {
        // setup the args for the string formatting
        object[] args = new object[] { car.CurrentSpeed * MphtoMps, car.CurrentSpeed * KphToMps, car.GearNum + 1, car.NumGears, car.RevsFactor, car.AccelInput };

        // display the car gui information
        GetComponent<GUIText>().text = string.Format(display, args);
    }
}
