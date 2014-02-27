using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InControl
{
	public class SwingModeProfile : UnityInputDeviceProfile
	{
		public SwingModeProfile()
		{
			Name = "Swing Mode";
			Meta = "Left thumbstick for aim, Right thumbstick for power. Alternatively can use the mouse x,y axis respectively";
			
			SupportedPlatforms = new[]
			{
				"Windows",
				"Mac",
				"Linux"
			};
			
			Sensitivity = 1.0f;
			LowerDeadZone = 0.0f;
			
			ButtonMappings = new[]
			{
				new InputControlMapping
				{
					Handle = "TakeShot",
					Target = InputControlType.Action1,
					Source = MouseButton0
				},
				new InputControlMapping
				{
					Handle = "TakeShotAlt",
					Target = InputControlType.Action2,
					Source = KeyCodeButton(KeyCode.Space)
				}
			};
			
			AnalogMappings = new[]
			{
				// MOUSE MAPPING
				new InputControlMapping
				{
					Handle = "Power",
					Target = InputControlType.LeftStickX,
					Source = MouseXAxis,
					Raw    = true
				},
				new InputControlMapping
				{
					Handle = "Rotate",
					Target = InputControlType.RightStickY,
					Source = MouseYAxis,
					Raw    = true
				},

				// KEYBOARD MAPPING
				new InputControlMapping
				{
					Handle = "Power Up",
					Target = InputControlType.DPadUp,
					Source = KeyCodeButton(KeyCode.W)
				},
				new InputControlMapping
				{
					Handle = "Power Down",
					Target = InputControlType.DPadDown,
					Source = KeyCodeButton(KeyCode.S)
				},
				new InputControlMapping
				{
					Handle = "Rotate Left",
					Target = InputControlType.DPadLeft,
					Source = KeyCodeButton(KeyCode.A)
				},
				new InputControlMapping
				{
					Handle = "Rotate Right",
					Target = InputControlType.DPadRight,
					Source = KeyCodeButton(KeyCode.D)
				}
			};
		}
	}
}

